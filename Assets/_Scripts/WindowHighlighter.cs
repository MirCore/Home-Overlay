using System.Collections;
using UnityEngine;

/// <summary>
/// Utility class for managing window background effects.
/// </summary>
public class WindowHighlighter
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
    /// The coroutine for handling the highlight effect.
    /// </summary>
    private Coroutine _highlightCoroutine;

    /// <summary>
    /// The renderer component of the window background.
    /// </summary>
    private readonly Renderer _meshRenderer;

    /// <summary>
    /// The shader property ID for the highlight fader.
    /// </summary>
    private readonly int _highlightFader = Shader.PropertyToID("_HighlightFader");

    /// <summary>
    /// Initializes a new instance of the WindowHighlighter class.
    /// </summary>
    /// <param name="meshRenderer">The renderer component of the window background.</param>
    public WindowHighlighter(Renderer meshRenderer)
    {
        _meshRenderer = meshRenderer;
    }

    /// <summary>
    /// Highlights the panel temporarily.
    /// </summary>
    /// <param name="coroutineRunner">The MonoBehaviour instance that will run the coroutine.</param>
    public void HighlightWindow(MonoBehaviour coroutineRunner)
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