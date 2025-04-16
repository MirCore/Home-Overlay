using System.Collections;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Manages fading effects for a canvas group and mesh renderer.
    /// Fades the canvas group and mesh renderer in (onEnabled) and out. 
    /// </summary>
    public class CanvasFader : MonoBehaviour
    {
        /// <summary>
        /// The CanvasGroup component to apply fading effects to.
        /// </summary>
        [SerializeField] private CanvasGroup CanvasGroup;

        /// <summary>
        /// The MeshRenderer component to apply fading effects to.
        /// </summary>
        [SerializeField] private Renderer MeshRenderer;

        /// <summary>
        /// The material to use for swapping to the VisionOS shader.
        /// </summary>
        [SerializeField] private Material VisionOSSwapMaterial;

        /// <summary>
        /// The GameObject associated with this fader.
        /// </summary>
        [SerializeField] private GameObject GameObject;

        /// <summary>
        /// The shader property ID for the alpha value.
        /// </summary>
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        /// <summary>
        /// The duration of the fade effect.
        /// </summary>
        private const float FadeDuration = 0.5f;

        /// <summary>
        /// The coroutine for handling the fade effect.
        /// </summary>
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            // Initialize the CanvasGroup if not assigned
            if (!CanvasGroup)
                CanvasGroup = GetComponent<CanvasGroup>();

            // Initialize the MeshRenderer if not assigned
            if (!MeshRenderer)
                MeshRenderer = GetComponent<MeshRenderer>();

            // Initialize the GameObject if not assigned
            if (!GameObject)
                GameObject = gameObject;

#if UNITY_VISIONOS
            // Apply VisionOS swap material if available
            if (VisionOSSwapMaterial)
                MeshRenderer.material = VisionOSSwapMaterial;
#endif
        }

        /// <summary>
        /// Sets the alpha value to 0 and starts the fade-in effect when the object is enabled.
        /// </summary>
        private void OnEnable()
        {
            SetAlpha(0f);
            StartFadeIn();
        }

        /// <summary>
        /// Starts the fade-in effect.
        /// </summary>
        private void StartFadeIn()
        {
            // Check if the GameObject is active in the hierarchy
            if (!gameObject.activeInHierarchy)
                return;

            // Stop any existing fade routine
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            // Start the fade-in coroutine
            _fadeRoutine = StartCoroutine(Fade(0f, 1f));
        }

        /// <summary>
        /// Starts the fade-out effect.
        /// </summary>
        public void FadeOut()
        {
            // Check if the GameObject is active in the hierarchy
            if (!gameObject.activeInHierarchy)
                return;

            // Stop any existing fade routine
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            // Start the fade-out coroutine
            _fadeRoutine = StartCoroutine(Fade(1f, 0f));
        }

        /// <summary>
        /// Starts the fade-out effect for a specific GameObject.
        /// </summary>
        /// <param name="go">The GameObject to apply the fade-out effect to.</param>
        public void FadeOut(GameObject go)
        {
            GameObject = go;
            FadeOut();
        }

        /// <summary>
        /// Coroutine to handle the fade effect between start and end alpha values.
        /// </summary>
        /// <param name="start">The starting alpha value.</param>
        /// <param name="end">The ending alpha value.</param>
        private IEnumerator Fade(float start, float end)
        {
            float elapsed = 0f;

            // Waits for one frame for layout to settle, to avoid a jumping layout
            if (start == 0f) yield return null;

            // Gradually change the alpha value over the fade duration
            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FadeDuration);
                float alpha = Mathf.Lerp(start, end, t);

                SetAlpha(alpha);
                yield return null;
            }

            // Ensure the final alpha value is set
            SetAlpha(end);

            // Deactivate the GameObject if the end alpha is 0
            if (end == 0f)
                GameObject.SetActive(false);
        }

        /// <summary>
        /// Sets the alpha value for the CanvasGroup and MeshRenderer.
        /// </summary>
        /// <param name="value">The alpha value to set.</param>
        private void SetAlpha(float value)
        {
            // Set the alpha value for the CanvasGroup
            if (CanvasGroup)
            {
                CanvasGroup.alpha = value;
                CanvasGroup.interactable = value > 0.99f;
                CanvasGroup.blocksRaycasts = value > 0.99f;
            }

            // Set the alpha value for the MeshRenderer if the material has the alpha property
            if (MeshRenderer && MeshRenderer.material.HasProperty(Alpha))
            {
                MeshRenderer.material.SetFloat(Alpha, value);
            }
        }
    }
}
