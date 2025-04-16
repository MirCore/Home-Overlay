using UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Utils;

public class WindowBehaviour
{
    private readonly Panels.Panel _panel;
    private readonly GameObject _windowControls;
    private readonly WindowBackgroundUtils _backgroundUtils;
    private readonly XRBaseInteractable _xrInteractable;
    private readonly RectTransform _canvasRectTransform;
    private readonly RoundedQuadMesh _roundedQuadMesh;
    private readonly LazyFollow _lazyFollow;
    
    public WindowBehaviour(Panels.Panel panel, GameObject windowControls)
    {
        _panel = panel;
        _windowControls = windowControls;
        Renderer meshRenderer = _panel.GetComponentInChildren<MeshRenderer>();
        _backgroundUtils = new WindowBackgroundUtils(meshRenderer);
        _xrInteractable = _panel.GetComponent<XRBaseInteractable>();
        Canvas canvas = _panel.GetComponentInChildren<Canvas>();
        _canvasRectTransform = canvas.GetComponent<RectTransform>();
        _roundedQuadMesh = canvas.GetComponent<RoundedQuadMesh>();
        _lazyFollow = _panel.GetComponent<LazyFollow>();
    }

    public void RegisterEventListeners()
    {
        _xrInteractable.selectEntered.AddListener(OnDragEntered);
        _xrInteractable.selectExited.AddListener(OnDragExited);
    }

    public void UnregisterEventListeners()
    {
        _xrInteractable.selectEntered.RemoveListener(OnDragEntered);
        _xrInteractable.selectExited.RemoveListener(OnDragExited);
    }

    public void SetWindowControlVisibility(bool showWindowControls)
    {
        _windowControls.SetActive(showWindowControls);
    }

    private void OnDragEntered(SelectEnterEventArgs eventData) => OnDragStarted();
    private void OnDragExited(SelectExitEventArgs eventData) => OnDragEnded();

    public void OnDragStarted()
    {
        _backgroundUtils.WindowIsMoving(_panel, true);
    }

    public void OnDragEnded()
    {
        _backgroundUtils.WindowIsMoving(_panel, false);
            
        if (_panel.PanelData == null)
            return;
            
        // Save the new pose of the panel
        _panel.PanelData.Position = _panel.transform.position;
        _panel.PanelData.Rotation = _panel.transform.rotation;
        _panel.PanelData.Scale = _panel.transform.localScale;

        // Create a new anchor
        AnchorHelper.CreateNewAnchor(_panel);
    }

    public void OnScaled() => _panel.PanelData.Scale = _panel.transform.localScale;

    public void HighlightPanel(Panels.Panel panelDataPanel)
    {
        _backgroundUtils.HighlightPanel(panelDataPanel);
    }
    
    private void UpdateWindowSize(bool expanded, bool compact)
    {
        _canvasRectTransform.sizeDelta = expanded || compact ? _panel.ExpandedCanvasSize : _panel.CompactCanvasSize;

        if (_roundedQuadMesh)
            _roundedQuadMesh.UpdateMesh();
    }


    private void OnChangeLazyFollowBehaviour(bool alignWindowToWall, bool rotationEnabled)
    {
        // toggle the LazyFollow component on/off
        if (alignWindowToWall || rotationEnabled)
        {
            _lazyFollow.enabled = false;
            AnchorHelper.CreateNewAnchor(_panel);
        }
        else
        {
            if (Camera.main)
            {
                Vector3 directionToCamera = _panel.transform.position - Camera.main.transform.position;
                _panel.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }

            _lazyFollow.enabled = true;
        }
    }

    public void LoadWindowState(bool alignWindowToWall, bool rotationEnabled, bool showWindowControls)
    {
        SetWindowControlVisibility(showWindowControls);
        OnChangeLazyFollowBehaviour(alignWindowToWall, rotationEnabled);
    }

    #region Panel-specific

    public void UpdatePanelLayout()
    {
        bool showName = _panel.PanelData.Settings.ShowName;
        bool showState = _panel.PanelData.Settings.ShowState;
            
        if ((!_panel.NameText || _panel.NameText.gameObject.activeSelf == showName) &&
            (!_panel.StateText || _panel.StateText.gameObject.activeSelf == showState))
            return;
            
        _panel.NameText?.gameObject.SetActive(showName);
        _panel.StateText?.gameObject.SetActive(showState);

        UpdateWindowSize(showName, showState);
    }

    #endregion
}