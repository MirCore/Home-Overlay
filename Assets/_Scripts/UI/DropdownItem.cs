using System;
using System.Linq;
using TMPro;
using UnityEngine;

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

        public void UpdateTitle()
        {
            HassEntity entity = GameManager.Instance.HassStates.FirstOrDefault(e => e.entity_id == Subtitle.text);
            Title.text = entity != null ? entity.attributes.friendly_name : Subtitle.text.Split('.')[1];;
        }
    }
}