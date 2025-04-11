using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Resizer : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform ObjectToTransform;
    private Panels.Panel _panel;

    private XRBaseInteractable _interactable;
    private Transform _interactor;

    private Vector3 _scaleOnSelectEntered;
    private float _distanceOnSelectEntered;

    private void OnEnable()
    {
        _panel = ObjectToTransform.GetComponent<Panels.Panel>();
        _interactable = GetComponent<XRBaseInteractable>();
        _interactable.selectEntered.AddListener(OnSelectEntered);
        _interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs eventData)
    {
        _interactor = eventData.interactorObject.transform;

        _scaleOnSelectEntered = ObjectToTransform.localScale;

        _distanceOnSelectEntered = Vector3.Distance(ObjectToTransform.position, _interactor.position);
    }

    private void OnSelectExited(SelectExitEventArgs eventData)
    {
        if (_panel != null) _panel.PanelData.Scale = transform.localScale;
    }

    private void Update()
    {
        if (!_interactable.isSelected)
            return;
        
        ObjectToTransform.localScale = _scaleOnSelectEntered * Vector3.Distance(ObjectToTransform.position, _interactor.position) / _distanceOnSelectEntered;
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (XRSettings.enabled)
            return;

        ObjectToTransform.localScale = _scaleOnSelectEntered * Vector3.Distance(ObjectToTransform.position, eventData.position) / _distanceOnSelectEntered;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (XRSettings.enabled)
            return;
        
        _scaleOnSelectEntered = ObjectToTransform.localScale;
        _distanceOnSelectEntered = Vector3.Distance(ObjectToTransform.position, eventData.position);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_panel != null) _panel.PanelData.Scale = transform.localScale;
    }
}