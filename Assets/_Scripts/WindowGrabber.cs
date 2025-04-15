using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

/// <summary>
/// Handles dragging and moving of a window or panel in non XR-Environments
/// </summary>
public class WindowGrabber : MonoBehaviour, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// The transform of the window to be moved.
    /// </summary>
    [SerializeField] private Transform WindowTransform;

    /// <summary>
    /// The scale factor to adjust the movement delta.
    /// </summary>
    [SerializeField] private float ScaleFactor = 1;

    /// <summary>
    /// The panel associated with this window grabber.
    /// </summary>
    private Panels.Panel _panel;

    private void OnEnable()
    {
        _panel = GetComponentInParent<Panels.Panel>();
    }

    /// <summary>
    /// Handles the drag event to move the window.
    /// </summary>
    /// <param name="eventData">The pointer event data associated with the drag event.</param>
    public void OnDrag(PointerEventData eventData)
    {
        // Check if XR settings are enabled, if so, do not proceed with dragging
        if (XRSettings.enabled)
            return;

        // Calculate the adjusted delta movement based on the scale factor
        Vector2 adjustedDelta = eventData.delta * ScaleFactor;

        // Update the local position of the window transform
        WindowTransform.localPosition += (Vector3)adjustedDelta;

        // Notify the panel that it is being moved
        if (_panel)
            _panel.WindowBehaviour.OnDragStarted();
    }

    /// <summary>
    /// Handles the end of the drag event.
    /// </summary>
    /// <param name="eventData">The pointer event data associated with the end drag event.</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        // Check if the panel is not null
        if (!_panel)
            return;

        // Notify the panel that it has stopped moving
        _panel.WindowBehaviour.OnDragEnded();
    }
}
