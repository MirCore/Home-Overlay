using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Hides UI elements outside the visible area of a ScrollView.
    /// </summary>
    public class HideOutsideScrollView : MonoBehaviour
    {
        [SerializeField] private ScrollRect ScrollRect;
        [SerializeField] private RectTransform RectTransform;
        private RectTransform _viewportRect;
        private readonly Vector3[] _viewportCorners = new Vector3[4];
        private readonly Vector3[] _panelCorners = new Vector3[4];
        private readonly List<GameObject> _children = new();

        private bool _isVisible;
        
        private void OnEnable()
        {
            // Initialize ScrollRect if not set in the inspector
            if (!ScrollRect)
            {
                ScrollRect = GetComponentInParent<ScrollRect>();
            }
            
            // Initialize the viewport RectTransform
            if (!_viewportRect && ScrollRect)
            {
                _viewportRect = ScrollRect.GetComponent<RectTransform>();
            }
            
            // Initialize the content RectTransform if not set in the inspector
            if (!RectTransform)
            {
                RectTransform = GetComponent<RectTransform>();
            }

            // Cache child GameObjects for visibility checks
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(_isVisible);
                if (!_children.Contains(child.gameObject))
                    _children.Add(child.gameObject);
            }

            // Subscribe to the scroll event if ScrollRect is available
            if (ScrollRect)
            {
                ScrollRect.onValueChanged.AddListener(OnScroll);
                UpdateVisibility();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from the scroll event to avoid memory leaks
            if (ScrollRect)
                ScrollRect.onValueChanged.RemoveListener(OnScroll);
            ScrollRect = null;
        }
        
        /// <summary>
        /// Handles the scroll event to update visibility of UI elements.
        /// </summary>
        /// <param name="position">The normalized position of the scroll.</param>
        private void OnScroll(Vector2 position)
        {
            UpdateVisibility();
        }

        /// <summary>
        /// Updates the visibility of UI elements based on their overlap with the viewport.
        /// </summary>
        private void UpdateVisibility()
        {
            // Get the world corners of the viewport
            _viewportRect.GetWorldCorners(_viewportCorners);
            Rect viewportRect = new (_viewportCorners[0], _viewportCorners[2] - _viewportCorners[0]);
        
            // Get the world corners of the content
            RectTransform.GetWorldCorners(_panelCorners);
            Rect rect =  new (_panelCorners[0], _panelCorners[2] - _panelCorners[0]);

            // Check if the content overlaps with the viewport
            bool isVisible = viewportRect.Overlaps(rect);
            
            if (isVisible != _isVisible)
                SetChildrenActive(isVisible);
            
            _isVisible = isVisible;
        }

        /// <summary>
        /// Sets the active state of all child UI elements.
        /// </summary>
        /// <param name="isVisible">Whether the UI elements should be visible.</param>
        private void SetChildrenActive(bool isVisible)
        {
            foreach (GameObject child in _children)
            {
                child.SetActive(isVisible);
            }
        }
    }
}