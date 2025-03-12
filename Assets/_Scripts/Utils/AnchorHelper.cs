using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Utils
{
    public abstract class AnchorHelper
    {
        private static readonly ARRaycastManager RaycastManager;
        private static readonly ARPlaneManager PlaneMeshManager;
        public static readonly ARAnchorManager ARAnchorManager;
        private static readonly Vector3[] Directions = { Vector3.forward, Vector3.back, Vector3.up, Vector3.down, Vector3.left, Vector3.right }; // Directions to cast rays

        static AnchorHelper()
        {
            RaycastManager = Object.FindFirstObjectByType<ARRaycastManager>();
            ARAnchorManager = Object.FindFirstObjectByType<ARAnchorManager>();
            PlaneMeshManager = Object.FindFirstObjectByType<ARPlaneManager>();
            
        }
        
        
        public static ARPlane FindNearestPlane(Transform transform)
        {
            List<ARRaycastHit> raycastHits = new ();
            ARPlane nearestPlane = null;
            float shortestDistance = float.MaxValue;
            Debug.Log("casting rays");
            if (RaycastManager.descriptor is { supportsWorldBasedRaycast: true })
            {
                foreach (Vector3 direction in Directions)
                {
                    // Perform raycast in the current direction
                    if (!RaycastManager.Raycast(new Ray(transform.position, direction), raycastHits, TrackableType.Planes))
                        continue;

                    foreach (ARRaycastHit hit in raycastHits.Where(hit => hit.distance < shortestDistance))
                    {
                        shortestDistance = hit.distance;
                        nearestPlane = hit.trackable as ARPlane;
                    }
                }

                Debug.Log("nearest plane: " + nearestPlane);
            }
            else
            {
                Debug.Log("World based raycast not supported");
                
                foreach (Vector3 direction in Directions)
                {
                    if (Physics.Raycast(transform.position, direction, out RaycastHit hit, shortestDistance))
                    {
                        Debug.Log("hit distance: " + hit.distance);
                        shortestDistance = hit.distance;
                        nearestPlane = hit.collider.GetComponent<ARPlane>(); // If ARPlane is not available, change this to a relevant component
                        Debug.Log(nearestPlane.trackableId);
                    }
                }
            }

            return nearestPlane;
        }

        /// <summary>
        /// Asynchronously creates an ARAnchor at the entity's current pose.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="anchorRotation"></param>
        
        public static async Task<Result<ARAnchor>> CreateAnchorAsync(Transform transform, Quaternion anchorRotation = default)
        {
            Quaternion rotation = anchorRotation == default ? transform.rotation : anchorRotation;
            // Attempt to add a new anchor at the current position and rotation
            Result<ARAnchor> result = await ARAnchorManager.TryAddAnchorAsync(new Pose(transform.position, rotation));
            if (!result.status.IsSuccess())
            {
                // Log a warning if the anchor creation fails
                Debug.LogWarning("Failed to create anchor");
            }
            
            return result;
        }
        
        /// <summary>
        /// Tries to retrieve an existing ARAnchor by its trackable ID.
        /// </summary>
        public static bool TryGetExistingAnchor(string anchorId, out ARAnchor anchor)
        {
            TrackableId trackableId = new(anchorId);
            anchor = ARAnchorManager.GetAnchor(trackableId);
            return anchor != null;
        }
        
        /// <summary>
        /// Sets the parent of a given transform to an ARAnchor.
        /// </summary>
        public static void AttachTransformToAnchor(Transform target, ARAnchor anchor)
        {
            if (target == null || anchor == null) return;

            target.SetParent(anchor.transform, false);
            target.position = anchor.transform.position;
            target.rotation = anchor.transform.rotation;
        }
        
        /// <summary>
        /// Sets the parent of a given transform to an ARAnchor.
        /// </summary>
        public static void AttachTransformToAnotherAnchor(Transform target, ARAnchor anchor, ARAnchor oldAnchor)
        {
            if (target == null || anchor == null) return;

            AttachTransformToAnchor(target, anchor);
            
            // Remove the old anchor if it exists
            if (oldAnchor != null) 
                TryRemoveAnchor(oldAnchor);
        }
        
        /// <summary>
        /// Tries to attach a transform to the nearest detected ARPlane.
        /// </summary>
        public static bool TryCreateAnchorOnNearestPlane(Transform target, out ARAnchor anchor)
        {
            anchor = null;
            if (target == null) return false;

            ARPlane nearestPlane = FindNearestPlane(target);
            if (nearestPlane == null) return false;

            Quaternion anchorRotation = Quaternion.LookRotation(-nearestPlane.normal, Vector3.up);
            Debug.Log(new Pose(target.position, anchorRotation));
            if (ARAnchorManager.descriptor.supportsTrackableAttachments)
                anchor = ARAnchorManager.AttachAnchor(nearestPlane, new Pose(target.position, anchorRotation));
            else
            {
                Task<Result<ARAnchor>> result = CreateAnchorAsync(target, anchorRotation);
                anchor = result.Result.value;
            }

            
            return anchor != null;
        }
        
        
        public static void TryRemoveAnchor(ARAnchor oldAnchor)
        {
            ARAnchorManager.TryRemoveAnchor(oldAnchor);
        }
    }
}