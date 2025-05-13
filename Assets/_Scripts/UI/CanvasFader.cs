using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Manages fading effects for a canvas group and mesh renderer.
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
        /// A collection of Images with shaders that will have their alpha values adjusted during fade operations.
        /// </summary>
        [SerializeField] private List<Image> Images;

        /// <summary>
        /// The duration of the fade effect for spawning.
        /// </summary>
        [Header("(De)spawn values")]
        private const float SpawnFadeDuration = 0.2f;

        /// <summary>
        /// The duration of the fade effect for movement.
        /// </summary>
        [Header("Movement values")]
        private const float MovementFadeDuration = 0.2f;

        /// <summary>
        /// The alpha value to use when the object is moving.
        /// </summary>
        private const float MovementAlpha = 0.5f;

        /// <summary>
        /// The coroutine for handling the fade effect.
        /// </summary>
        private Coroutine _fadeRoutine;

        /// <summary>
        /// Initializes the CanvasFader components when the object is awakened.
        /// </summary>
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
            FadeIn();
        }

        /// <summary>
        /// Starts the fade-in effect.
        /// </summary>
        private void FadeIn()
        {
            if (!IsReady()) return;

            // Start the fade-in coroutine
            _fadeRoutine = StartCoroutine(Fade(1f, SpawnFadeDuration));
        }

        /// <summary>
        /// Starts the fade-out effect.
        /// </summary>
        /// <param name="disableGameObject">Whether to deactivate the GameObject after fading out.</param>
        public void FadeOut(bool disableGameObject)
        {
            if (!IsReady()) return;

            // Start the fade-out coroutine
            _fadeRoutine = StartCoroutine(Fade(0f, SpawnFadeDuration, disableGameObject));
        }

        /// <summary>
        /// Starts the fade-out effect for a specific GameObject.
        /// </summary>
        /// <param name="go">The GameObject to apply the fade-out effect to.</param>
        public void FadeOut(GameObject go)
        {
            GameObject = go;
            FadeOut(true);
        }

        /// <summary>
        /// Coroutine to handle the fade effect between start and end alpha values.
        /// </summary>
        /// <param name="end">The ending alpha value.</param>
        /// <param name="duration">The duration of the fade effect.</param>
        /// <param name="disableGameObject">Whether to deactivate the GameObject after fading out.</param>
        private IEnumerator Fade(float end, float duration, bool disableGameObject = false)
        {
            float elapsed = 0f;
            float start = CanvasGroup.alpha;

            // Wait one frame for layout to settle, to avoid a jumping layout
            if (start == 0f) yield return null;

            // Multiply the duration by the delta, to ensure a correct fade speed, if the coroutine starts mid-fade
            duration *= Mathf.Abs(start - end);

            // Gradually change the alpha value over the fade duration
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float alpha = Mathf.Lerp(start, end, t);

                SetAlpha(alpha);
                yield return null;
            }

            // Ensure the final alpha value is set
            SetAlpha(end);

            // Deactivate the GameObject if requested
            if (disableGameObject)
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
            }

            // Set the alpha value for the MeshRenderer if the material has the alpha property
            if (MeshRenderer && MeshRenderer.material.HasProperty(Alpha))
            {
                MeshRenderer.material.SetFloat(Alpha, value);
            }

            foreach (Image image in Images)
            {
                //image.material.SetFloat(Alpha, value);
                image.gameObject.SetActive(value > 0.2f);
            }
        }

        /// <summary>
        /// Fades in or out based on the visibility flag.
        /// </summary>
        /// <param name="makeVisible">Whether the object should be visible.</param>
        public void FadeInOut(bool makeVisible)
        {
            if (makeVisible)
                FadeIn();
            else
                FadeOut(false);
        }

        /// <summary>
        /// Adjusts the alpha value of the window background based on whether it is moving.
        /// </summary>
        /// <param name="isMoving">True if the window is moving, false otherwise.</param>
        public void WindowIsMoving(bool isMoving)
        {
            if (!IsReady()) return;

            float alpha = isMoving ? MovementAlpha : 1f;

            // Start the fade-out coroutine
            _fadeRoutine = StartCoroutine(Fade(alpha, MovementFadeDuration));
        }

        /// <summary>
        /// Checks if the CanvasFader is ready to perform fade operations.
        /// </summary>
        /// <returns>True if the CanvasFader is ready, false otherwise.</returns>
        private bool IsReady()
        {
            // Check if the GameObject is active in the hierarchy
            if (!gameObject.activeInHierarchy)
                return false;

            // Stop any existing fade routine
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);
            return true;
        }
    }
}
