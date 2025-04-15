using System.Collections;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Utility class for managing window background effects such as highlighting and alpha adjustments.
    /// </summary>
    public class WindowBackgroundUtils
    {
        /// <summary>
        /// The duration for which the highlight effect is active.
        /// </summary>
        private const float HighlightDuration = 5f;

        /// <summary>
        /// The duration over which the highlight effect fades in and out.
        /// </summary>
        private const float HighlightFadeDuration = 0.5f;

        /// <summary>
        /// The intensity of the highlight effect.
        /// </summary>
        private const float HighlightIntensity = 0.5f;

        /// <summary>
        /// The duration over which the alpha value fades.
        /// </summary>
        private const float AlphaFadeDuration = 0.2f;

        /// <summary>
        /// The default alpha value of the window background.
        /// </summary>
        private const float DefaultAlpha = 1f;

        /// <summary>
        /// The coroutine for handling the highlight effect.
        /// </summary>
        private Coroutine _highlightCoroutine;

        /// <summary>
        /// The coroutine for handling alpha adjustments.
        /// </summary>
        private Coroutine _alphaCoroutine;

        /// <summary>
        /// The renderer component of the window background.
        /// </summary>
        private readonly Renderer _meshRenderer;

        /// <summary>
        /// The shader property ID for the highlight fader.
        /// </summary>
        private readonly int _highlightFader = Shader.PropertyToID("_HighlightFader");

        /// <summary>
        /// The shader property ID for the alpha value.
        /// </summary>
        private readonly int _alpha = Shader.PropertyToID("_Alpha");

        /// <summary>
        /// Initializes a new instance of the WindowBackgroundUtils class.
        /// </summary>
        /// <param name="meshRenderer">The renderer component of the window background.</param>
        public WindowBackgroundUtils(Renderer meshRenderer)
        {
            _meshRenderer = meshRenderer;
        }

        /// <summary>
        /// Adjusts the alpha value of the window background based on whether it is being moved.
        /// </summary>
        /// <param name="coroutineRunner">The MonoBehaviour instance that will run the coroutine.</param>
        /// <param name="isMoving">True if the window is moving, false otherwise.</param>
        public void WindowIsMoving(MonoBehaviour coroutineRunner, bool isMoving)
        {
            if (!IsMeshRendererValid(_alpha))
                return;

            // Stop any existing alpha coroutine
            if (_alphaCoroutine != null)
                coroutineRunner.StopCoroutine(_alphaCoroutine);

            // Start a new coroutine to adjust the alpha value
            _alphaCoroutine = coroutineRunner.StartCoroutine(SetAlpha(isMoving ? HighlightIntensity : DefaultAlpha, _alpha));
        }

        /// <summary>
        /// Coroutine to set the alpha value of the window background.
        /// </summary>
        /// <param name="alphaTarget">The target alpha value.</param>
        /// <param name="propertyID">The shader property ID for the alpha value.</param>
        private IEnumerator SetAlpha(float alphaTarget, int propertyID)
        {
            yield return FadeTo(alphaTarget, AlphaFadeDuration, propertyID);
            _alphaCoroutine = null;
        }

        /// <summary>
        /// Highlights the panel temporarily.
        /// </summary>
        /// <param name="coroutineRunner">The MonoBehaviour instance that will run the coroutine.</param>
        public void HighlightPanel(MonoBehaviour coroutineRunner)
        {
            if (_highlightCoroutine != null)
                return;

            if (!IsMeshRendererValid(_highlightFader))
                return;

            // Start a new coroutine to highlight the panel
            _highlightCoroutine = coroutineRunner.StartCoroutine(SetHighlightColorTemporarily(HighlightDuration));
        }

        /// <summary>
        /// Temporarily sets the highlight color of the panel for the given duration.
        /// </summary>
        /// <param name="duration">The duration of the highlight effect in seconds.</param>
        private IEnumerator SetHighlightColorTemporarily(float duration)
        {
            yield return FadeTo(HighlightIntensity, HighlightFadeDuration, _highlightFader);
            yield return new WaitForSeconds(duration);
            yield return FadeTo(0f, HighlightFadeDuration, _highlightFader);

            _highlightCoroutine = null;
        }

        /// <summary>
        /// Fades the specified property to the target value over the given duration.
        /// </summary>
        /// <param name="targetValue">The target value to fade to.</param>
        /// <param name="duration">The duration of the fade.</param>
        /// <param name="propertyID">The shader property ID to fade.</param>
        private IEnumerator FadeTo(float targetValue, float duration, int propertyID)
        {
            float elapsed = 0f;
            float startValue = _meshRenderer.material.GetFloat(propertyID);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float value = Mathf.Lerp(startValue, targetValue, t);

                _meshRenderer.material.SetFloat(propertyID, value);

                yield return null;
            }

            _meshRenderer.material.SetFloat(propertyID, targetValue);
        }

        /// <summary>
        /// Checks if the mesh renderer is valid and has the specified shader property.
        /// </summary>
        /// <param name="propertyID">The shader property ID to check.</param>
        /// <returns>True if the mesh renderer is valid and has the property, false otherwise.</returns>
        private bool IsMeshRendererValid(int propertyID)
        {
            if (_meshRenderer && _meshRenderer.material.HasProperty(propertyID)) return true;
            Debug.Log($"MeshRenderer is null or does not have the property with ID {propertyID}");
            return false;
        }
    }
}
