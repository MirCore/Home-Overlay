using System;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FriendlyNameHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text Title;
        [SerializeField] private TMP_Text Subtitle;
        [SerializeField] internal Button Button;

        private void OnEnable()
        {
            if (Button == null)
                return;
            ColorBlock colorBlock = Button.colors;
            colorBlock.normalColor = new Color(1, 1, 1, 0f);
            colorBlock.highlightedColor = new Color(1, 1, 1, 0.1f);
            Button.colors = colorBlock;
            
            Button.onClick.AddListener(Highlight);
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
            else if (Subtitle.text.Split('.').Length > 1)
                Title.text = Subtitle.text.Split('.')[1];
        }

        public void SetNewEntity(EntityObject entityObject)
        {
            Subtitle.text = entityObject.EntityID;
            
            UpdateTitle();
        }

        public void SetNewEntity(string entityID)
        {        
                Subtitle.text = entityID;
                
                UpdateTitle(entityID);
        }

        public void Highlight()
        {
            if (Button == null)
                return;
            ColorBlock colorBlock = Button.colors;
            colorBlock.normalColor = new Color(1, 1, 1, 0.1f);
            colorBlock.highlightedColor = new Color(1, 1, 1, 0.2f);
            Button.colors = colorBlock;
        }
    }
}