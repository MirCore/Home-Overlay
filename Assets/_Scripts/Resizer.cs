using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Resizer : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform ObjectToTransform;
    private Panel.Panel _panel;

    private XRBaseInteractable _interactable;
    private XRBaseInteractor _interactor;

    private Vector3 _scaleOnSelectEntered;
    private float _distanceOnSelectEntered;

    private void OnEnable()
    {
        _panel = ObjectToTransform.GetComponent<Panel.Panel>();
        _interactable = GetComponent<XRBaseInteractable>();
        _interactable.selectEntered.AddListener(OnSelectEntered);
        _interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs eventData)
    {
        Debug.Log("OnSelectEntered");
        _interactor = eventData.interactorObject.transform.GetComponent<XRBaseInteractor>();

        _scaleOnSelectEntered = ObjectToTransform.localScale;

        if (((XRRayInteractor)_interactor).TryGetCurrent3DRaycastHit(out RaycastHit hit)){
        {
            _distanceOnSelectEntered = Vector3.Distance(ObjectToTransform.position, hit.point);
        }}
    }

    private void OnSelectExited(SelectExitEventArgs eventData)
    {
        if (_panel != null) _panel.PanelData.Scale = transform.localScale;
    }

    private void Update()
    {
        if (!_interactable.isSelected)
            return;
        
        if (((XRRayInteractor)_interactor).TryGetCurrent3DRaycastHit(out RaycastHit hit)){
        {
            ObjectToTransform.localScale = _scaleOnSelectEntered * Vector3.Distance(ObjectToTransform.position, hit.point) / _distanceOnSelectEntered;
        }}
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