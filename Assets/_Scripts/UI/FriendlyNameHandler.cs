using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
#if UNITY_VISIONOS 
using Unity.PolySpatial;
#endif

namespace UI
{
    public class FriendlyNameHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text Title;
        [SerializeField] private TMP_Text Subtitle;
        [SerializeField] internal Button Button;
        private ColorBlock _colorBlock;
        private readonly Color _colorBlockNormalColor = new (1, 1, 1, 0f);
        private readonly Color _colorBlockHighlightedColor = new (1, 1, 1, 0.1f);
        private readonly Color _colorBlockSelectedHighlightedColor = new (1, 1, 1, 0.2f);

        private void OnEnable()
        {
            if (Button == null)
                return;
            _colorBlock = Button.colors;
            _colorBlock.normalColor = _colorBlockNormalColor;
            _colorBlock.highlightedColor = _colorBlockHighlightedColor;
            Button.colors = _colorBlock;
        }

        private void Start()
        {
            UpdateTitle();
        }

        private void OnDisable()
        {
            if (Button)
                Button.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Updates the Dropdown Title based on the Subtitle, which is the entity_id
        /// </summary>
        /// <param name="entityID"></param>
        private void UpdateTitle(string entityID = "")
        {
            HassState hassState = HassStates.GetHassState(entityID == "" ? Subtitle.text : entityID);
            if (hassState != null)
                Title.text = hassState.attributes.friendly_name;
        }

        public void SetNewEntity(PanelData panelData)
        {
            Subtitle.text = panelData.EntityID;
            
            UpdateTitle();
        }

        public void SetNewEntity(string entityID)
        {        
            Subtitle.text = entityID;
            
            UpdateTitle(entityID);
        }

#if UNITY_VISIONOS 
        public void RemoveFromSortingGroup()
        {
            VisionOSSortingGroup sorting = GetComponentInParent<VisionOSSortingGroup>();
            if (sorting == null) return;
            ObservableList<VisionOSSortingGroup.RendererSorting> groupMembers = sorting.Renderers;

            for (int i = 0; i < groupMembers.Count; i++)
            {
                if (groupMembers[i].Renderer != gameObject)
                    continue;
                groupMembers.RemoveAt(i);
                return;
            }
        }
#endif

        public void Highlight(string entityID)
        {
            if (Button == null)
                return;
            if (entityID == Subtitle.text)
            {
                _colorBlock.normalColor = _colorBlockHighlightedColor;
                _colorBlock.highlightedColor = _colorBlockSelectedHighlightedColor;
            }
            else
            {
                _colorBlock.normalColor = _colorBlockNormalColor;
                _colorBlock.highlightedColor = _colorBlockHighlightedColor;
            }

            Button.colors = _colorBlock;
        }
    }
}