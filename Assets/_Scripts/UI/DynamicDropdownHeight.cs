using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class DynamicDropdownHeight : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown Dropdown; // Reference to the TMP_Dropdown
        private RectTransform _template; // Reference to the Template RectTransform
        [SerializeField] private float itemHeight = 30f; // Height of a single dropdown item
        [SerializeField] private float maxHeight = 300f; // Maximum height for the dropdown

        private void OnEnable()
        {
            _template = Dropdown.template;
            
            if (Dropdown == null || _template == null)
            {
                Debug.LogError("Dropdown or Template is not assigned!");
                return;
            }

            AdjustDropdownHeight();
        }

        private void AdjustDropdownHeight()
        {
            // Calculate required height based on the number of items
            float requiredHeight = Dropdown.options.Count * itemHeight;

            // Clamp the height to the maximum allowed height
            requiredHeight = Mathf.Min(requiredHeight, maxHeight);
            requiredHeight = Math.Max(requiredHeight, itemHeight * 2);

            // Set the template's height
            _template.sizeDelta = new Vector2(_template.sizeDelta.x, requiredHeight);
        }
    }
}