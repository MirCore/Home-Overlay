using System.Collections;
using UnityEngine;

namespace UI
{
    public class DynamicScrollRect : MonoBehaviour
    {
        [SerializeField] private RectTransform ScrollRect;
        [SerializeField] private RectTransform Content;
        [SerializeField] private float MaxHeight; // Maximum size of the ScrollRect

        private void OnEnable()
        {
            StartCoroutine(AdjustScrollRectSizeNextFrame());
        }
        
        private IEnumerator AdjustScrollRectSizeNextFrame()
        {
            if (!Content || !ScrollRect) 
                yield break;
            
            // Wait until the end of the current frame
            yield return null;
            
            AdjustScrollRectSize();
        }

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

        public void OnContentSizeChanged()
        {
            StartCoroutine(AdjustScrollRectSizeNextFrame());
        }
    }
}