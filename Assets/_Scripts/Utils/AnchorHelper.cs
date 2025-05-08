using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Panels;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Object = UnityEngine.Object;

namespace Utils
{
    /// <summary>
    /// An abstract utility class for managing AR anchors.
    /// </summary>
    public abstract class AnchorHelper
    {
        private static readonly ARRaycastManager RaycastManager;
        private static readonly ARPlaneManager PlaneMeshManager;
        public static readonly ARAnchorManager ARAnchorManager;
        
        /// <summary>
        /// Static constructor to initialize AR managers.
        /// </summary>
        static AnchorHelper()
        {
            RaycastManager = Object.FindFirstObjectByType<ARRaycastManager>();
            ARAnchorManager = Object.FindFirstObjectByType<ARAnchorManager>();
            PlaneMeshManager = Object.FindFirstObjectByType<ARPlaneManager>();
        }
        
        /// <summary>
        /// Checks if AR anchors are supported on the current device.
        /// </summary>
        /// <returns>True if anchors are supported, otherwise false.</returns>
        private static bool IsAnchorSupportAvailable()
        {
            // Check if the device is a simulator, which does not support AR anchors
            if (SystemInfo.graphicsDeviceName.Contains("simulator"))
                return false;
            
            // Retrieve the active AR anchor subsystem and return true if the subsystem is running
            XRAnchorSubsystem anchorSubsystem =  LoaderUtility.GetActiveLoader()?.GetLoadedSubsystem<XRAnchorSubsystem>();
            return anchorSubsystem is { running: true };
        }
        
        #region Anchor Creation and Management
        
        /// <summary>
        /// Asynchronously creates an ARAnchor at the specified transform's current pose.
        /// </summary>
        /// <param name="transform">The transform to use for positioning the anchor.</param>
        /// <param name="anchorRotation">Optional rotation for the anchor.</param>
        /// <returns>The created ARAnchor, or null if anchors are not supported.</returns>
        private static async Task<ARAnchor> TryAddAnchorAsync(Transform transform, Quaternion anchorRotation = default)
        {
            if (!IsAnchorSupportAvailable())
                return null;
            
            // Determine the rotation for the anchor
            Quaternion rotation = anchorRotation != default ? anchorRotation : Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            // Attempt to add a new anchor at the current position and rotation
            Result<ARAnchor> result = await ARAnchorManager.TryAddAnchorAsync(new Pose(transform.position, rotation));
            
            return result.value;
        }
        
        /// <summary>
        /// Tries to create an ARAnchor on the nearest detected ARPlane.
        /// </summary>
        /// <param name="transform">The transform to use for positioning the anchor.</param>
        /// <returns>The created ARAnchor, or null if no plane is found or anchors are not supported.</returns>
        private static ARAnchor TryCreateAnchorOnNearestPlane(Transform transform)
        {
            if (!IsAnchorSupportAvailable())
                return null;
            
            // Find the nearest plane to the transform
            ARPlane nearestPlane = FindNearestPlane(transform);
            if (nearestPlane == null)
                return null;

            ARAnchor anchor;
            
            // Attach the anchor to the plane if supported, otherwise create a new anchor, with the planes normal as rotation
            Quaternion anchorRotation = Quaternion.LookRotation(-nearestPlane.normal, Vector3.up);
            if (ARAnchorManager.descriptor.supportsTrackableAttachments)
                anchor = ARAnchorManager.AttachAnchor(nearestPlane, new Pose(transform.position, anchorRotation));
            else
            {
                Task<ARAnchor> result = TryAddAnchorAsync(transform, anchorRotation);
                anchor = result.Result;
            }
            
            return anchor;
        }
        
        /// <summary>
        /// Tries to retrieve an existing ARAnchor by its trackable ID.
        /// </summary>
        /// <param name="anchorId">The ID of the anchor to retrieve.</param>
        /// <param name="anchor">The retrieved ARAnchor, or null if not found.</param>
        /// <returns>True if the anchor was found, otherwise false.</returns>
        private static bool TryGetExistingAnchorInternal(string anchorId, out ARAnchor anchor)
        {
            // Check if the anchor ID is valid
            if (string.IsNullOrEmpty(anchorId))
            {
                anchor = null;
                return false;
            }
            
            // Retrieve the anchor using its trackable ID
            TrackableId trackableId = new(anchorId);
            anchor = ARAnchorManager.GetAnchor(trackableId);
            return anchor != null;
        }

        /// <summary>
        /// Tries to remove an existing ARAnchor by its ID.
        /// </summary>
        /// <param name="oldAnchorID">The ID of the anchor to remove.</param>
        private static void TryRemoveAnchor(string oldAnchorID)
        {
            if (!IsAnchorSupportAvailable())
                return;

            try
            {
                // Retrieve and remove the anchor if it exists
                if (TryGetExistingAnchor(oldAnchorID, out ARAnchor anchor))
                    ARAnchorManager.TryRemoveAnchor(anchor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// Tries to retrieve an existing ARAnchor by its trackable ID.
        /// </summary>
        /// <param name="anchorId">The ID of the anchor to retrieve.</param>
        /// <param name="anchor">The retrieved ARAnchor, or null if not found.</param>
        /// <returns>True if the anchor was found, otherwise false.</returns>
        private static bool TryGetExistingAnchor(string anchorId, out ARAnchor anchor)
        {
            return TryGetExistingAnchorInternal(anchorId, out anchor);
        }
        
        #endregion
        
        #region Anchor Attachment
        
        /// <summary>
        /// Sets the parent of a given transform to an ARAnchor.
        /// </summary>
        /// <param name="transform">The transform to attach.</param>
        /// <param name="applyRotation">If the transform should copy the anchor rotation</param>
        /// <param name="anchor">The ARAnchor to attach to.</param>
        private static void AttachTransformToAnchor(Transform transform, bool applyRotation, ARAnchor anchor)
        {
            if (anchor == null) 
                return;
            
            // Set the target's parent to the anchor and update its position and rotation
            transform.SetParent(anchor.transform, false);
            transform.position = anchor.transform.position;
            if (applyRotation)
                transform.rotation = anchor.transform.rotation;
        }

        /// <summary>
        /// Sets the parent of a given transform to an ARAnchor and removes the old anchor if it exists.
        /// </summary>
        /// <param name="transform">The transform to attach.</param>
        /// <param name="applyRotation">If the transform should copy the anchor rotation</param>
        /// <param name="anchor">The ARAnchor to attach to.</param>
        /// <param name="oldAnchorID">The ID of the old anchor to remove.</param>
        private static void AttachTransformToAnotherAnchor(Transform transform, bool applyRotation, ARAnchor anchor, string oldAnchorID)
        {
            if (anchor == null) 
                return;
            
            // Attach the target to the new anchor
            AttachTransformToAnchor(transform, applyRotation, anchor);
            
            // Remove the old anchor if it exists
            if (!string.IsNullOrEmpty(oldAnchorID)) 
                TryRemoveAnchor(oldAnchorID);
        }
        
        #endregion

        #region Plane Detection

        /// <summary>
        /// Finds the nearest ARPlane to a given transform by performing raycasts in specific directions.
        /// </summary>
        /// <param name="transform">The transform from which to determine the nearest plane based on position and orientation.</param>
        /// <returns>The nearest ARPlane, or null if no suitable plane is found.</returns>
        private static ARPlane FindNearestPlane(Transform transform)
        {
            ARPlane nearestPlane = RaycastManager.descriptor is { supportsWorldBasedRaycast: true }
                ? FindNearestPlaneUsingWorldBasedRaycast(transform)
                : FindNearestPlaneUsingPhysicsRaycast(transform);

            return nearestPlane;
        }

        /// <summary>
        /// Finds the nearest ARPlane using world-based ray-casting.
        /// </summary>
        /// <param name="transform">The transform from which the raycast originates.</param>
        /// <returns>The nearest ARPlane, or null if no plane is detected.</returns>
        private static ARPlane FindNearestPlaneUsingWorldBasedRaycast(Transform transform)
        {
            ARPlane nearestPlane = null;
            float shortestDistance = float.MaxValue;
            List<ARRaycastHit> raycastHits = new();
            Vector3 direction = transform.forward;
            direction.y = 0;
            
            // Perform raycast in the current direction
            RaycastManager.Raycast(new Ray(transform.position, direction), raycastHits, TrackableType.Planes);
            
            // Find the nearest hit within the shortest distance
            foreach (ARRaycastHit hit in raycastHits.Where(hit => hit.distance < shortestDistance))
            {
                shortestDistance = hit.distance;
                nearestPlane = hit.trackable as ARPlane;
            
            }
            return nearestPlane;
        }


        /// <summary>
        /// Finds the nearest ARPlane using physics-based ray-casting.
        /// </summary>
        /// <param name="transform">The transform representing the position and direction from which the raycast originates.</param>
        /// <returns>The nearest ARPlane if found, otherwise null.</returns>
        private static ARPlane FindNearestPlaneUsingPhysicsRaycast(Transform transform)
        {
            Vector3 direction = transform.forward;
            direction.y = 0;

            // Perform a physics raycast, filtered by shortestDistance
            Physics.Raycast(transform.position, direction, out RaycastHit hit, 2f, LayerMask.GetMask("ARPlane"));
            
            return hit.collider.GetComponent<ARPlane>();
        }

        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Tries to retrieve an existing ARAnchor by its trackable ID.
        /// </summary>
        /// <param name="anchorId">The ID of the anchor to retrieve.</param>
        /// <returns>True if the anchor was found, otherwise false.</returns>
        public static bool TryGetExistingAnchor(string anchorId)
        {
            return TryGetExistingAnchorInternal(anchorId, out _);
        }
        
        /// <summary>
        /// Tries to attach a transform to an existing ARAnchor.
        /// </summary>
        /// <param name="transform">The transform to attach.</param>
        /// <param name="applyRotation">If the transform should copy the anchor rotation</param>
        /// <param name="anchorID">The ID of the anchor to attach to.</param>
        public static void TryAttachToExistingAnchor(Transform transform, bool applyRotation, string anchorID)
        {
            if (TryGetExistingAnchor(anchorID, out ARAnchor anchor))
                AttachTransformToAnchor(transform, applyRotation, anchor);
        }

        /// <summary>
        /// Creates a new ARAnchor and attaches a transform to it using the specified panel.
        /// </summary>
        /// <param name="panel">The panel containing the transform and settings for anchor creation.</param>
        private static async Task<ARAnchor> CreateAnchorAsync(Panel panel)
        {
            if (!IsAnchorSupportAvailable())
                return null;

            ARAnchor anchor;

            try
            {
                if (panel.PanelData.Settings._alignPanelToWall)
                    anchor = TryCreateAnchorOnNearestPlane(panel.transform);
                else
                    anchor = await TryAddAnchorAsync(panel.transform);
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateNewAnchor Error: " + e);
                return null;
            }
            
            return anchor == null ? null : anchor;
        }

        #endregion

        public static void CreateNewAnchor(Panel panel)
        {
            // Run async with explicit error handling
            _ = CreateNewAnchorAsync(panel).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"CreateNewAnchor encountered an error: {task.Exception}");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static async Task CreateNewAnchorAsync(Panel panel)
        {
            ARAnchor anchor = await CreateAnchorAsync(panel);

            AttachTransformToAnotherAnchor(panel.transform, panel.PanelData.Settings.AlignWindowToWall, anchor, panel.PanelData.AnchorID);
            
            if (anchor)
            {
                panel.PanelData.AnchorID = anchor.trackableId.ToString();
            }
            else 
            {
                if (panel.PanelData.Settings.AlignWindowToWall)
                    panel.TurnOffAlignWindowToWall();
            }
        }
    }
}