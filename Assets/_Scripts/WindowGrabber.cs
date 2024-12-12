using Structs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class WindowGrabber : MonoBehaviour, IDragHandler
{
    [SerializeField] private Transform WindowTransform;
    [SerializeField] private float ScaleFactor = 1;
    
    private void OnEnable()
    {
        IDragHandler foo = GetComponent<IDragHandler>();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (XRSettings.enabled)
            return;
        
        Vector2 adjustedDelta = eventData.delta * ScaleFactor;

        WindowTransform.localPosition += (Vector3)adjustedDelta;
    }
        
}