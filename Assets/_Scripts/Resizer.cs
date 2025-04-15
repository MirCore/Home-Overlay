using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Handles resizing of an object through dragging or XR interactions.
/// </summary>
public class Resizer : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform ObjectToTransform;
    private Panels.Panel _panel;

    /// <summary>
    /// The XR interactable component for handling XR interactions.
    /// </summary>
    private XRBaseInteractable _xrInteractable;

    /// <summary>
    /// The transform of the interactor.
    /// </summary>
    private Transform _interactor;

    /// <summary>
    /// The initial scale of the object when resizing starts.
    /// </summary>
    private Vector3 _startScale;

    /// <summary>
    /// The initial distance between the object and the interactor when resizing starts.
    /// </summary>
    private float _startDistance;

    private void OnEnable()
    {
        _panel = ObjectToTransform.GetComponent<Panels.Panel>();

        // Get the XR interactable component
        _xrInteractable = GetComponent<XRBaseInteractable>();
        // Subscribe to XR interaction events
        _xrInteractable.selectEntered.AddListener(OnSelectEntered);
        _xrInteractable.selectExited.AddListener(OnSelectExited);
    }

    /// <summary>
    /// Handles the event when the XR interaction starts.
    /// </summary>
    /// <param name="eventData">The event data for the select entered event.</param>
    private void OnSelectEntered(SelectEnterEventArgs eventData)
    {
        // Cache the interactors transform
        _interactor = eventData.interactorObject.transform;

        // Cache the initial scale and distance
        CacheInitialScaleAndDistance(_interactor.position);

        // Notify the panel that it is being moved
        _panel?.WindowBehaviour.OnDragStarted();
    }

    /// <summary>
    /// Handles the event when the XR interaction ends.
    /// </summary>
    /// <param name="eventData">The event data for the select exited event.</param>
    private void OnSelectExited(SelectExitEventArgs eventData)
    {
        if (!_panel) return;

        // Update the panel's scale
        UpdatePanelScale();

        // Notify the panel that it has stopped moving
        _panel?.WindowBehaviour.OnDragEnded();
    }

    /// <summary>
    /// Updates the object's scale based on the interactors position.
    /// </summary>
    private void Update()
    {
        // Check if the object is currently selected in XR
        if (!_xrInteractable.isSelected)
            return;

        // Update the object's scale
        UpdateObjectScale(_interactor.position);
    }

    #region Non-XR dragging

    /// <summary>
    /// Handles the drag event to resize the object.
    /// </summary>
    /// <param name="eventData">The pointer event data associated with the drag event.</param>
    public void OnDrag(PointerEventData eventData)
    {
        // Check if XR settings are enabled, if so, do not proceed with dragging
        if (XRSettings.enabled)
            return;

        // Update the object's scale based on the pointer position
        UpdateObjectScale(eventData.position);
    }

    /// <summary>
    /// Handles the pointer down event to start resizing the object.
    /// </summary>
    /// <param name="eventData">The pointer event data associated with the pointer down event.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if XR settings are enabled, if so, do not proceed with dragging
        if (XRSettings.enabled)
            return;

        // Cache the initial scale and distance
        CacheInitialScaleAndDistance(eventData.position);

        // Notify the panel that it is being moved
        _panel?.WindowBehaviour.OnDragStarted();
    }

    /// <summary>
    /// Handles the pointer up event to end resizing the object.
    /// </summary>
    /// <param name="eventData">The pointer event data associated with the pointer up event.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        // Check if XR settings are enabled, if so, do not proceed with dragging
        if (XRSettings.enabled)
            return;

        // Update the panel's scale
        UpdatePanelScale();

        // Notify the panel that it has stopped moving
        _panel?.WindowBehaviour.OnDragEnded();
    }

    #endregion

    /// <summary>
    /// Caches the initial scale and distance between the object and the interactor.
    /// </summary>
    /// <param name="interactorPosition">The position of the interactor.</param>
    private void CacheInitialScaleAndDistance(Vector3 interactorPosition)
    {
        _startScale = _panel.transform.localScale;
        _startDistance = Vector3.Distance(_panel.transform.position, interactorPosition);
    }

    /// <summary>
    /// Updates the object's scale based on the current distance from the interactor.
    /// </summary>
    /// <param name="interactorPosition">The position of the interactor.</param>
    private void UpdateObjectScale(Vector3 interactorPosition)
    {
        // Calculate the current distance between the object and the interactor
        float currentDistance = Vector3.Distance(_panel.transform.position, interactorPosition);
        // Update the object's scale based on the ratio of the current distance to the initial distance
        _panel.transform.localScale = _startScale * (currentDistance / _startDistance);
    }

    /// <summary>
    /// Updates the panel's scale data with the current scale of the object.
    /// </summary>
    private void UpdatePanelScale()
    {
        _panel?.WindowBehaviour.OnScaled();
    }
}
