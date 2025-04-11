using System;
using System.Collections;
using UnityEngine;

namespace UI
{
    public class CanvasFader : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        private Renderer _meshRenderer;

        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        private const float FadeDuration = 0.5f;

        private Coroutine _fadeRoutine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        void OnEnable()
        {
            SetAlpha(0f);
            StartFadeIn();
        }

        public void StartFadeIn()
        {
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(Fade(0f, 1f));
        }

        public void FadeOut()
        {
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(Fade(1f, 0f));
        }

        private IEnumerator Fade(float start, float end)
        {
            float elapsed = 0f;

            // Optional: Wait one frame for layout settle
            if (start == 0f) yield return null;

            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FadeDuration);
                float alpha = Mathf.Lerp(start, end, t);

                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(end);
            if (end == 0f)
                gameObject.SetActive(false);
        }

        private void SetAlpha(float value)
        {
            if (_canvasGroup)
            {
                _canvasGroup.alpha = value;
                _canvasGroup.interactable = value > 0.99f;
                _canvasGroup.blocksRaycasts = value > 0.99f;
            }

            if (_meshRenderer && _meshRenderer.material.HasProperty(Alpha))
            {
                _meshRenderer.material.SetFloat(Alpha, value);
            }
        }
    }
}