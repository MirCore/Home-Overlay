using Structs;
using TMPro;
using UnityEngine;

namespace UI
{
    public class FriendlyNameHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text Title;
        [SerializeField] private TMP_Text Subtitle;

        private void Start()
        {
            UpdateTitle();
        }

        /// <summary>
        /// Updates the Dropdown Title based on the Subtitle, which is the entity_id
        /// </summary>
        public void UpdateTitle()
        {
            HassEntity entity = HassStates.GetHassState(Subtitle.text);
            if (entity != null)
                Title.text = entity.attributes.friendly_name;
            else if (Subtitle.text.Split('.').Length > 1)
                Title.text = Subtitle.text.Split('.')[1];
        }

        public void SetNewEntity(EntityObject entityObject)
        {
            Subtitle.text = entityObject.EntityID;
            
            UpdateTitle();
        }
    }
}