using UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Base class for windows in the environment, handling visibility and layout
/// </summary>
public class Window : MonoBehaviour
{
    /// <summary>
    /// The window controls game object of the panel.
    /// </summary>
    [SerializeField] private GameObject WindowControls;
        

    /// <summary>
    /// The XR interactable component for handling XR interactions.
    /// </summary>
    private XRBaseInteractable _xrInteractable;

    /// <summary>
    /// The RectTransform component of the canvas.
    /// </summary>
    protected RectTransform CanvasRectTransform;

    /// <summary>
    /// The component responsible for rendering the rounded quad mesh.
    /// </summary>
    private RoundedQuadMesh _roundedQuadMesh;

    /// <summary>
    /// The component responsible for lazy following behavior.
    /// </summary>
    private LazyFollow _lazyFollow;

    /// <summary>
    /// The component responsible for fading the canvas.
    /// </summary>
    private CanvasFader _canvasFader;
        
    /// <summary>
    /// Initializes the window behavior when the panel is awakened.
    /// </summary>
    private void Awake()
    {
        // Get the XRBaseInteractable component from the window
        _xrInteractable = GetComponent<XRBaseInteractable>();

        // Get the Canvas component from the window
        Canvas canvas = GetComponentInChildren<Canvas>();
        _canvasFader = canvas.GetComponent<CanvasFader>();
        CanvasRectTransform = canvas.GetComponent<RectTransform>();
        _roundedQuadMesh = canvas.GetComponent<RoundedQuadMesh>();

        // Get the LazyFollow component from the window
        _lazyFollow = GetComponent<LazyFollow>();
    }

    protected void OnEnable()
    {
        _xrInteractable.selectEntered.AddListener(OnDragEntered);
        _xrInteractable.selectExited.AddListener(OnDragExited);
    }

    protected void OnDisable()
    {
        _xrInteractable.selectEntered.RemoveListener(OnDragEntered);
        _xrInteractable.selectExited.RemoveListener(OnDragExited);
    }

    /// <summary>
    /// Sets the visibility of the window controls.
    /// </summary>
    /// <param name="showWindowControls">Whether to show the window controls.</param>
    public void SetWindowControlVisibility(bool showWindowControls)
    {
        WindowControls.SetActive(showWindowControls);
    }

    /// <summary>
    /// Handles the event when dragging starts.
    /// </summary>
    /// <param name="eventData">The event data for the drag-entered event.</param>
    private void OnDragEntered(SelectEnterEventArgs eventData) => OnDragStarted();

    /// <summary>
    /// Handles the event when dragging ends.
    /// </summary>
    /// <param name="eventData">The event data for the drag-exited event.</param>
    private void OnDragExited(SelectExitEventArgs eventData) => OnDragEnded();

    /// <summary>
    /// Handles the start of dragging.
    /// </summary>
    public virtual void OnDragStarted()
    {
        _canvasFader.WindowIsMoving(true);
    }

    /// <summary>
    /// Handles the end of dragging.
    /// </summary>
    public virtual void OnDragEnded()
    {
        _canvasFader.WindowIsMoving(false);
    }

    /// <summary>
    /// Updates the window size based on the panel's settings.
    /// </summary>
    /// <param name="expanded">Whether the window is expanded.</param>
    /// <param name="compact">Whether the window is compact.</param>
    protected virtual void UpdateWindowSize(bool expanded, bool compact)
    {
        if (_roundedQuadMesh)
            _roundedQuadMesh.UpdateMesh();
    }

    /// <summary>
    /// Loads the window state based on the panel's settings.
    /// </summary>
    /// <param name="alignWindowToWall">Whether to align the window to the wall.</param>
    /// <param name="rotationEnabled">Whether rotation is enabled.</param>
    /// <param name="showWindowControls">Whether to show the window controls.</param>
    protected void LoadWindowState(bool alignWindowToWall, bool rotationEnabled, bool showWindowControls)
    {
        SetWindowControlVisibility(showWindowControls);
        LoadLazyFollowBehaviour(alignWindowToWall, rotationEnabled);
    }
        
    /// <summary>
    /// Loads the lazy follow behavior based on the panel's settings.
    /// </summary>
    /// <param name="alignWindowToWall">Whether to align the window to the wall.</param>
    /// <param name="rotationEnabled">Whether rotation is enabled.</param>
    private void LoadLazyFollowBehaviour(bool alignWindowToWall, bool rotationEnabled)
    {
        if (!_lazyFollow)
        {
            // Get the LazyFollow component from the window
            _lazyFollow = GetComponent<LazyFollow>();
        }
        
        // Toggle the LazyFollow component on/off
        if (alignWindowToWall || !rotationEnabled)
        {
            _lazyFollow.enabled = false;
        }
        else if (!_lazyFollow.enabled)
        {
            if (Camera.main) 
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

            _lazyFollow.enabled = true;
        }
    }
}