using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Object = UnityEngine.Object;

namespace Utils
{
    public abstract class AnchorHelper
    {
        private static readonly ARRaycastManager RaycastManager;
        private static readonly ARPlaneManager PlaneMeshManager;
        public static readonly ARAnchorManager ARAnchorManager;
        private static readonly Vector3[] Directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right }; // Directions to cast rays

        static AnchorHelper()
        {
            RaycastManager = Object.FindFirstObjectByType<ARRaycastManager>();
            ARAnchorManager = Object.FindFirstObjectByType<ARAnchorManager>();
            PlaneMeshManager = Object.FindFirstObjectByType<ARPlaneManager>();
        }
        
        private static ARPlane FindNearestPlane(Transform transform)
        {
            ARPlane nearestPlane = null;
            float shortestDistance = float.MaxValue;
            
            foreach (Vector3 direction in Directions)
            {
                if (RaycastManager.descriptor is { supportsWorldBasedRaycast: true })
                {
                    List<ARRaycastHit> raycastHits = new ();
                    // Perform raycast in the current direction
                    if (!RaycastManager.Raycast(new Ray(transform.position, direction), raycastHits, TrackableType.Planes))
                        continue;

                    foreach (ARRaycastHit hit in raycastHits.Where(hit => hit.distance < shortestDistance))
                    {
                        shortestDistance = hit.distance;
                        nearestPlane = hit.trackable as ARPlane;
                    }
                }
                else
                {
                    if (!Physics.Raycast(transform.position, direction, out RaycastHit hit, shortestDistance))
                        continue;
                    shortestDistance = hit.distance;
                    nearestPlane = hit.collider.GetComponent<ARPlane>();
                }
            }

            return nearestPlane;
        }

        /// <summary>
        /// Asynchronously creates an ARAnchor at the panel's current pose.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="anchorRotation"></param>
        private static async Task<ARAnchor> CreateAnchorAsync(Transform transform, Quaternion anchorRotation = default)
        {
            if (!AnchorsAreSupported())
                return null;
            
            Quaternion rotation = anchorRotation != default ? anchorRotation : Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            // Attempt to add a new anchor at the current position and rotation
            Result<ARAnchor> result = await ARAnchorManager.TryAddAnchorAsync(new Pose(transform.position, rotation));
            
            return result.value;
        }

        private static bool AnchorsAreSupported()
        {
            if (SystemInfo.graphicsDeviceName.Contains("simulator"))
                return false;
            
            XRAnchorSubsystem anchorSubsystem =  LoaderUtility.GetActiveLoader()?.GetLoadedSubsystem<XRAnchorSubsystem>();
            return anchorSubsystem is { running: true };
        }

        /// <summary>
        /// Tries to retrieve an existing ARAnchor by its trackable ID.
        /// </summary>
        public static bool TryGetExistingAnchor(string anchorId, out ARAnchor anchor)
        {
            if (string.IsNullOrEmpty(anchorId))
            {
                anchor = null;
                return false;
            }
            TrackableId trackableId = new(anchorId);
            anchor = ARAnchorManager.GetAnchor(trackableId);
            return anchor != null;
        }
        
        /// <summary>
        /// Sets the parent of a given transform to an ARAnchor.
        /// </summary>
        private static void AttachTransformToAnchor(Transform target, ARAnchor anchor)
        {
            if (target == null || anchor == null) return;

            target.SetParent(anchor.transform, false);
            target.position = anchor.transform.position;
            target.rotation = anchor.transform.rotation;
        }
        
        /// <summary>
        /// Sets the parent of a given transform to an ARAnchor.
        /// </summary>
        private static void AttachTransformToAnotherAnchor(Transform target, ARAnchor anchor, string oldAnchorID)
        {
            if (target == null || anchor == null) return;

            AttachTransformToAnchor(target, anchor);
            
            // Remove the old anchor if it exists
            if (!string.IsNullOrEmpty(oldAnchorID)) 
                TryRemoveAnchor(oldAnchorID);
        }
        
        /// <summary>
        /// Tries to attach a transform to the nearest detected ARPlane.
        /// </summary>
        private static ARAnchor TryCreateAnchorOnNearestPlane(Transform target)
        {
            ARAnchor anchor;
            
            if (!AnchorsAreSupported() || target == null)
                return null;

            ARPlane nearestPlane = FindNearestPlane(target);
            if (nearestPlane == null)
                return null;

            Quaternion anchorRotation = Quaternion.LookRotation(-nearestPlane.normal, Vector3.up);
            if (ARAnchorManager.descriptor.supportsTrackableAttachments)
                anchor = ARAnchorManager.AttachAnchor(nearestPlane, new Pose(target.position, anchorRotation));
            else
            {
                Task<ARAnchor> result = CreateAnchorAsync(target, anchorRotation);
                anchor = result.Result;
            }
            
            return anchor;
        }

        private static void TryRemoveAnchor(string oldAnchorID)
        {
            if (!AnchorsAreSupported())
                return;
            if (TryGetExistingAnchor(oldAnchorID, out ARAnchor anchor))
                ARAnchorManager.TryRemoveAnchor(anchor);
        }

        public static void TryAttachToExistingAnchor(Transform transform, string anchorID)
        {
            if (TryGetExistingAnchor(anchorID, out ARAnchor anchor))
                AttachTransformToAnchor(transform, anchor);
        }
        
        public static async Task<TrackableId?> CreateNewAnchor(Transform transform, bool attachToPlane, string oldAnchor)
        {
            if (!AnchorsAreSupported())
                return null;
            
            ARAnchor anchor;
            
            try
            {
                if (attachToPlane)
                    anchor = TryCreateAnchorOnNearestPlane(transform);
                else
                    anchor = await CreateAnchorAsync(transform);
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateNewAnchor Error: " + e);
                return null;
            }
            
            if (anchor == null)
                return null;
            
            AttachTransformToAnotherAnchor(transform, anchor, oldAnchor);
            return anchor.trackableId;
        }
    }
}