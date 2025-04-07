using System;
using System.Collections;
using System.Threading.Tasks;
using Managers;
using Structs;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Utils;

namespace Panels
{
    public abstract class Panel : MonoBehaviour
    {
        [SerializeField] private Image HighlightImage;
        [SerializeField] internal TMP_Text Icon;
        [SerializeField] internal TMP_Text NameText;
        [SerializeField] internal TMP_Text StateText;
        [SerializeField] private Button SettingsButton;
        [SerializeField] private GameObject WindowControls;
        private Canvas _canvas;
        private RectTransform _canvasRectTransform;
        private RoundedQuadMesh _roundedQuadMesh;
        
        [SerializeField] private Vector2 CompactCanvasSize = new (120, 120);
        [SerializeField] private Vector2 ExpandedCanvasSize = new (320, 120);
        
        private LazyFollow _lazyFollow;
        private XRBaseInteractable _interactable;

        protected HassState HassState;
        public PanelData PanelData { get; private set; }

        /// <summary>
        /// A coroutine that is currently setting the color of the button temporarily.
        /// </summary>
        private IEnumerator _setButtonColorTemporarilyCoroutine;

        protected virtual void OnEnable()
        {
            if (Icon != null) Icon.text = "";
            if (NameText != null) NameText.text = "";
            if (StateText != null) StateText.text = "";

            _canvas = GetComponentInChildren<Canvas>();
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();
            _roundedQuadMesh = _canvas.GetComponent<RoundedQuadMesh>();
            
            _lazyFollow = GetComponent<LazyFollow>();
            SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
            _interactable = GetComponent<XRBaseInteractable>();
            _interactable.selectExited.AddListener(OnSelectExited);
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
        }

        protected virtual void OnDisable()
        {
            SettingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            _interactable.selectExited.RemoveListener(OnSelectExited);
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }

        private void OnSelectExited(SelectExitEventArgs eventData)
        {
            if (PanelData == null)
                return;
            
            // Save the new pose of the panel
            PanelData.Position = transform.position;
            PanelData.Rotation = transform.rotation;
            PanelData.Scale = transform.localScale;

            // Create a new anchor
            // Run async with explicit error handling
            _ = CreateNewAnchor().ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"CreateNewAnchor encountered an error: {task.Exception}");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #region ARAnchors

        private async Task CreateNewAnchor()
        {
            Debug.Log("Creating new anchor");
            try
            {
                ARAnchor newAnchor;
                
                if (SystemInfo.graphicsDeviceName == "Apple visionOS simulator GPU")
                    return;
                
                if (PanelData.Settings.AlignWindowToWall)
                {
                    Debug.Log("Aligning to wall");
                    bool result = AnchorHelper.TryCreateAnchorOnNearestPlane(transform, out newAnchor);
                    if (!result)
                    {
                        Debug.Log("No plane found, turning off AlignWindowToWall");
                        PanelData.Settings.AlignWindowToWall = false;
                        OnSettingsChanged();
                        return;
                    }
                }
                else if (!PanelData.Settings.RotationEnabled)
                {
                    Debug.Log("Creating new fixed anchor");
                    Quaternion rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    Result<ARAnchor> result = await AnchorHelper.CreateAnchorAsync(transform, rotation);
                    if (!result.status.IsSuccess())
                    {
                        Debug.LogWarning("Failed to create fixed anchor");
                        return;
                    }
                    newAnchor = result.value;
                }
                else
                {
                    Debug.Log("Creating new normal anchor");
                    Result<ARAnchor> result = await AnchorHelper.CreateAnchorAsync(transform);
                    if (!result.status.IsSuccess())
                    {
                        Debug.LogWarning("Failed to create anchor");
                        return;
                    }
                    newAnchor = result.value;
                }
                if (newAnchor == null)
                {
                    Debug.LogWarning("Failed to create anchor");
                    return;
                }
                Debug.Log("Anchor created, attaching to anchor: " + newAnchor.trackableId);
                AnchorHelper.AttachTransformToAnotherAnchor(transform, newAnchor, PanelData.AnchorID);
                PanelData.AnchorID = newAnchor.trackableId.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private void ReattachToAnchor()
        {
            // If the panel object does not have an anchor ID, exit the method
            if (string.IsNullOrEmpty(PanelData.AnchorID))
                return;

            if (AnchorHelper.TryGetExistingAnchor(PanelData.AnchorID, out ARAnchor anchor))
            {
                AnchorHelper.AttachTransformToAnchor(transform, anchor);
            }
        }

        #endregion

        /// <summary>
        /// Called when Hass states change.
        /// </summary>
        private void OnHassStatesChanged()
        {
            // Get the current state of the panel.
            HassState ??= HassStates.GetHassState(PanelData.EntityID);

            // Update the Panel UI
            UpdatePanel();
        }

        /// <summary>
        /// Initiates the Panel
        /// </summary>
        /// <param name="panelData">The panel object to be associated with this panel.</param>
        public void InitPanel(PanelData panelData)
        {
            // Assign the provided panel object to the current panel
            PanelData = panelData;
            panelData.Panel = this;
            
            transform.localPosition = panelData.Position;
            transform.localScale = panelData.Scale;
            
            ReattachToAnchor();
            SetWindowControlVisibility();

            // If there is no EntityID, there is nothing to update.
            if (PanelData.EntityID == null)
                return;

            // Get the current state of the panel.
            HassState ??= HassStates.GetHassState(PanelData.EntityID);

            if (HassState != null)
            {
                // Update the Panel to reflect the current state of the entity
                UpdatePanel();
            }

        }

        // Assign a new Panel ID to the panel
        public void AssignNewEntityID(string entityID)
        {
            PanelData.EntityID = entityID;

            // Get the current state of the panel.
            HassState = HassStates.GetHassState(PanelData.EntityID);
            
            UpdatePanel();
            PanelSettingsWindowManager.Instance.UpdatePanelSettingsWindow(this);
        }

        protected virtual void UpdatePanel()
        {
            if (!PanelIsReady())
                return;

            if (NameText != null) NameText.text = HassState.attributes.friendly_name;
        }

        public void DeletePanel()
        {
            PanelManager.Instance.RemovePanel(PanelData);
            Destroy(gameObject);
        }


        /// <summary>
        /// Temporarily highlights the panel by changing the color of its button to red.
        /// The highlighting is done by starting a coroutine that changes the color of the button to red
        /// and then waits for the specified duration and then changes the color back to the original color.
        /// </summary>
        public void HighlightPanel()
        {
            if (_setButtonColorTemporarilyCoroutine != null)
                return;

            _setButtonColorTemporarilyCoroutine = SetButtonColorTemporarily(Color.red, 10f);
            StartCoroutine(_setButtonColorTemporarilyCoroutine);
        }

        /// <summary>
        /// Temporarily sets the color of the panel's button to the given color for the given duration.
        /// </summary>
        /// <param name="color">The color to set the button to.</param>
        /// <param name="duration">The duration of the color change in seconds.</param>
        private IEnumerator SetButtonColorTemporarily(Color color, float duration)
        {
            // Store the original color of the button
            Color originalColor = HighlightImage.color;

            // Change the color of the button to the given color
            HighlightImage.color = color;

            // Wait for the specified duration
            yield return new WaitForSeconds(duration);

            // Change the color of the button back to the original color
            HighlightImage.color = originalColor;

            // Set the flag to indicate that the coroutine has finished
            _setButtonColorTemporarilyCoroutine = null;
        }
        
        

        protected void UpdatePanelLayout()
        {
            if (NameText.gameObject.activeSelf == PanelData.Settings.ShowName && StateText.gameObject.activeSelf == PanelData.Settings.ShowState)
                return;
            
            NameText.gameObject.SetActive(PanelData.Settings.ShowName);
            StateText.gameObject.SetActive(PanelData.Settings.ShowState);
            
            if (PanelData.Settings.ShowName || PanelData.Settings.ShowState)
            {
                _canvasRectTransform.sizeDelta = ExpandedCanvasSize;
            }
            else
            {
                _canvasRectTransform.sizeDelta = CompactCanvasSize;
            }

            if (_roundedQuadMesh)
                _roundedQuadMesh.UpdateMesh();
        }
        
        private void OnSettingsButtonClicked() => PanelSettingsWindowManager.Instance.ToggleSettingsWindow(this);

        public EDeviceType GetDeviceType() => HassState.DeviceType;

        public void ReloadSettingsWindow() => PanelSettingsWindowManager.Instance.UpdatePanelSettingsWindow(this);

        public void SetWindowControlVisibility() => WindowControls.SetActive(!PanelData.Settings.HideWindowControls);
        

        public void OnSettingsChanged()
        {
            // toggle the LazyFollow component on/off
            if (PanelData.Settings.AlignWindowToWall || !PanelData.Settings.RotationEnabled)
            {
                _lazyFollow.enabled = false;
                _ = CreateNewAnchor();
            }
            else
                _lazyFollow.enabled = true;
            
            UpdatePanel();
            ReloadSettingsWindow();
        }

        protected bool PanelIsReady()
        {
            if (PanelData.EntityID == null)
                return false;
            if (HassState == null)
                return false;
            
            return true;
        }

        public void OnEndDrag()
        {
            OnSelectExited(null);
        }
    }
}