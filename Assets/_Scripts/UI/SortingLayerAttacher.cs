using UnityEngine;
using UnityEngine.Rendering;

#if PLATFORM_VISIONOS
using Unity.PolySpatial;
#endif

namespace UI
{
    public class SortingLayerAttacher : MonoBehaviour
    {
#pragma warning disable 414
        [SerializeField] private int SortingLayer = 0;
        [SerializeField] private bool ApplyToChildren = false;
#pragma warning restore 414
        
#if PLATFORM_VISIONOS
        private VisionOSSortingGroup _sortingGroup;
#endif
        
        private void OnEnable()
        {
#if PLATFORM_VISIONOS
            if (_sortingGroup == null)
            {
                _sortingGroup = GetComponentInParent<VisionOSSortingGroup>();
            }

            if (_sortingGroup == null)
            {
                _sortingGroup = transform.root.gameObject.AddComponent<VisionOSSortingGroup>();
                Debug.Log("Could not find VisionOSSortingGroup, created a new one.");
            }
            
            ObservableList<VisionOSSortingGroup.RendererSorting> groupMembers = _sortingGroup.Renderers;

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
