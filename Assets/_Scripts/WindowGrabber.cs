using Structs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class WindowGrabber : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private Transform WindowTransform;
    [SerializeField] private float ScaleFactor = 1;
    private Panel.Panel _panel;
    
    private void OnEnable()
    {
        _panel = GetComponentInParent<Panel.Panel>();
        IDragHandler _ = GetComponent<IDragHandler>();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (XRSettings.enabled)
            return;
        
        Vector2 adjustedDelta = eventData.delta * ScaleFactor;

        WindowTransform.localPosition += (Vector3)adjustedDelta;
        }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_panel)
            return;

        _panel.OnEndDrag();
    }
}