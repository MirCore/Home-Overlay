using System;
using System.Linq;
using Managers;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class DropdownItem : MonoBehaviour
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
            HassEntity entity = GameManager.Instance.GetHassState(Subtitle.text);
            Title.text = entity != null ? entity.attributes.friendly_name : Subtitle.text.Split('.')[1];;
        }
    }
}