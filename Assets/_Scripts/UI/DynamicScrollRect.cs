using System.Collections;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Manages a scroll rect that dynamically adjusts its size based on its content,
    /// while respecting a maximum height constraint.
    /// </summary>
    public class DynamicScrollRect : MonoBehaviour
    {
        [SerializeField] private RectTransform ScrollRect;
        [SerializeField] private RectTransform Content;
        [SerializeField] private float MaxHeight; // Maximum size of the ScrollRect

        /// <summary>
        /// Initiates the scroll rect size adjustment when the component becomes enabled.
        /// </summary>
        private void OnEnable()
        {
            StartCoroutine(AdjustScrollRectSizeNextFrame());
        }
        
        /// <summary>
        /// Coroutine that waits for the next frame before adjusting the scroll rect size.
        /// This ensures all content is properly laid out before sizing calculations.
        /// </summary>
        /// <returns>An IEnumerator for the coroutine system.</returns>
        private IEnumerator AdjustScrollRectSizeNextFrame()
        {
            if (!Content || !ScrollRect) 
                yield break;
            
            // Wait until the end of the current frame
            yield return null;
            
            AdjustScrollRectSize();
        }

        /// <summary>
        /// Adjusts the scroll rect's size based on its content size while respecting the MaxHeight constraint.
        /// The width remains unchanged while the height is clamped to the maximum allowed value.
        /// </summary>
        private void AdjustScrollRectSize()
        {
            // Calculate the size of the content
            Vector2 contentSize = Content.sizeDelta;

            // Clamp the content size to the maximum size
            Vector2 newSize = new (
                contentSize.x,
                Mathf.Min(contentSize.y, MaxHeight)
            );

            // Set the size of the ScrollRects viewport to the new size
            ScrollRect.sizeDelta = newSize;
        }

        /// <summary>
        /// Called when the content size changes to trigger a recalculation of the scroll rect size.
        /// This method should be invoked whenever the content's dimensions are modified.
        /// </summary>
        public void OnContentSizeChanged()
        {
            StartCoroutine(AdjustScrollRectSizeNextFrame());
        }
    }
}