using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

#if PLATFORM_VISIONOS
using Unity.PolySpatial;
#endif

namespace UI
{
    /// <summary>
    /// Handles the attachment and management of sorting layers for Unity objects,
    /// with special handling for VisionOS platform sorting groups.
    /// </summary>
    public class SortingLayerAttacher : MonoBehaviour
    {
#pragma warning disable 414
        /// <summary>
        /// The sorting layer order value to be applied.
        /// </summary>
        [SerializeField] private int SortingLayer = 0;

        /// <summary>
        /// Whether the sorting layer should be applied to child objects.
        /// </summary>
        [SerializeField] private bool ApplyToChildren = false;
#pragma warning restore 414
        
#if PLATFORM_VISIONOS
        /// <summary>
        /// Reference to the VisionOS-specific sorting group component.
        /// </summary>
        private VisionOSSortingGroup _sortingGroup;
#endif
        
        /// <summary>
        /// Initializes and configures the sorting layer settings when the component is enabled.
        /// For VisionOS platform, it sets up the VisionOSSortingGroup and manages renderer sorting.
        /// </summary>
        private void OnEnable()
        {
#if PLATFORM_VISIONOS
            // Check for any parent objects that also have SortingLayerAttacher with ApplyToChildren enabled
            IEnumerable<SortingLayerAttacher> sortingLayerAttacher = GetComponentsInParent<SortingLayerAttacher>()
                .Where(sla => sla.ApplyToChildren)
                .Where(sla => sla.gameObject != gameObject);

            // Log warnings for potential conflicts
            foreach (SortingLayerAttacher attacher in sortingLayerAttacher)
            {
                Debug.LogWarning("SortingLayerAttacher with enabled ApplyToChildren: " + attacher.gameObject.name, attacher.gameObject);
            }
            
            // Try to find an existing sorting group in the parent hierarchy
            if (_sortingGroup == null)
            {
                _sortingGroup = GetComponentInParent<VisionOSSortingGroup>();
            }

            // Create a new sorting group if none exists
            if (_sortingGroup == null)
            {
                _sortingGroup = transform.root.gameObject.AddComponent<VisionOSSortingGroup>();
                //Debug.Log("Could not find VisionOSSortingGroup, created a new one.", gameObject);
            }
            
            ObservableList<VisionOSSortingGroup.RendererSorting> groupMembers = _sortingGroup.Renderers;
            
            // Update existing renderer sorting if it exists
            for (int i = 0; i < groupMembers.Count; i++)
            {
                if (groupMembers[i].Renderer != gameObject)
                    continue;
                VisionOSSortingGroup.RendererSorting member = groupMembers[i];
                member.Order = SortingLayer;
                member.ApplyToDescendants = ApplyToChildren;
                groupMembers[i] = member;
                return;
            }
            
            // Create a new renderer sorting if none exists
            VisionOSSortingGroup.RendererSorting newMember = new()
            {
                Order = SortingLayer,
                Renderer = gameObject,
                ApplyToDescendants = ApplyToChildren
            };

            groupMembers.Add(newMember);
#endif
        }
    }
}