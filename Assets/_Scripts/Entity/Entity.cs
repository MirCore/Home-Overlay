using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Managers;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Utils;

namespace Entity
{
    public abstract class Entity : MonoBehaviour
    {
        [SerializeField] private Image HighlightImage;
        [SerializeField] internal TMP_Text Icon;
        [SerializeField] private Button SettingsButton;
        [SerializeField] private GameObject WindowControls;
        private LazyFollow _lazyFollow;
        private XRBaseInteractable _interactable;

        protected HassState HassState;
        public EntityObject EntityObject { get; private set; }

        private ARAnchor _anchor;    

        /// <summary>
        /// A coroutine that is currently setting the color of the button temporarily.
        /// </summary>
        private IEnumerator _setButtonColorTemporarilyCoroutine;

        protected virtual void OnEnable()
        {
            _lazyFollow = GetComponent<LazyFollow>();
            AnchorHelper.ARAnchorManager.trackablesChanged.AddListener(OnAnchorChanged);

            SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
            _interactable = GetComponent<XRBaseInteractable>();
            _interactable.selectExited.AddListener(OnSelectExited);
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
        }

        protected virtual void OnDisable()
        {
            AnchorHelper.ARAnchorManager.trackablesChanged.RemoveListener(OnAnchorChanged);

            SettingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            _interactable.selectExited.RemoveListener(OnSelectExited);
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }

        /// <summary>
        /// Listens for when AR anchors are added to the scene, and parents this entity to the anchor that matches the EntityObject's AnchorID.
        /// </summary>
        private void OnAnchorChanged(ARTrackablesChangedEventArgs<ARAnchor> changes)
        {
            // if the entity already has an anchor, exit the method
            if (_anchor != null)
                return;

            ARAnchor addedAnchor = changes.added.FirstOrDefault(a => a.trackableId.ToString() == EntityObject.AnchorID);
            if (addedAnchor == null)
                return;
            
            AnchorHelper.AttachTransformToAnchor(transform, addedAnchor);
            //Debug.Log("Entity parented to added anchor");
        }

        private void OnSelectExited(SelectExitEventArgs eventData)
        {
            Debug.Log("Select exited");
            if (EntityObject == null)
                return;
            
            // Save the new pose of the entity
            EntityObject.Position = transform.position;
            EntityObject.Rotation = transform.rotation;
            EntityObject.Scale = transform.localScale;

            // Create a new anchor
            // Run async with explicit error handling
            _ = CreateNewAnchor().ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"CreateNewAnchor encountered an error: {task.Exception}");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task CreateNewAnchor()
        {
            Debug.Log("Creating new anchor");
            ARAnchor newAnchor;
            try
            {
                if (EntityObject.Settings.AlignWindowToWall)
                {
                    Debug.Log("Aligning to wall");
                    if (!AnchorHelper.TryCreateAnchorOnNearestPlane(transform, out  newAnchor))
                    {
                        Debug.Log("No plane found, turning off AlignWindowToWall");
                        EntityObject.Settings.AlignWindowToWall = false;
                        UpdateRotationBehaviour();
                    }
                }
                else if (!EntityObject.Settings.RotationEnabled)
                {
                    Debug.Log("Creating new fixed anchor");
                    Quaternion rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    Result<ARAnchor> result = await AnchorHelper.CreateAnchorAsync(transform, rotation);
                    if (!result.status.IsSuccess())
                    {
                        Debug.LogWarning("Failed to create anchor");
                    }
                    newAnchor = result.value;
                }
                else
                {
                    Debug.Log("Creating new anchor");
                    Result<ARAnchor> result = await AnchorHelper.CreateAnchorAsync(transform);
                    if (!result.status.IsSuccess())
                    {
                        Debug.LogWarning("Failed to create anchor");
                    }
                    newAnchor = result.value;
                }
                
                Debug.Log("Anchor created, attaching to anchor: " + newAnchor.trackableId);
                AnchorHelper.AttachTransformToAnotherAnchor(transform, newAnchor, _anchor);
                _anchor = newAnchor;
                EntityObject.AnchorID = _anchor.trackableId.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private void ReattachToAnchor()
        {
            // If the entity object does not have an anchor ID, exit the method
            if (string.IsNullOrEmpty(EntityObject.AnchorID))
                return;

            if (AnchorHelper.TryGetExistingAnchor(EntityObject.AnchorID, out ARAnchor anchor))
            {
                AnchorHelper.AttachTransformToAnchor(transform, anchor);
            }
            else
            {
                //Debug.Log("No anchor with ID " + EntityObject.AnchorID + " was loaded");
            }
        }

        /// <summary>
        /// Called when Hass states change.
        /// </summary>
        private void OnHassStatesChanged()
        {
            // Get the current state of the entity.
            HassState ??= HassStates.GetHassState(EntityObject.EntityID);

            // Update the Entity UI
            UpdateEntity();
        }

        /// <summary>
        /// Sets the entity object and updates the icon and anchor accordingly.
        /// </summary>
        /// <param name="entityObject">The entity object to be associated with this entity.</param>
        public void InitEntityObject(EntityObject entityObject)
        {
            // Assign the provided entity object to the current entity
            EntityObject = entityObject;
            // Add the entity object to the GameManager's list of entities
            GameManager.Instance.AddEntity(EntityObject, this);
            ReattachToAnchor();
            SetWindowControlVisibility();

            // If there is no entity ID, there is nothing to update.
            if (EntityObject.EntityID == null)
            {
                Icon.text = "";
                return;
            }

            // Get the current state of the entity.
            HassState ??= HassStates.GetHassState(EntityObject.EntityID);

            if (HassState != null)
            {
                // Update the icon to reflect the current state of the entity
                UpdateEntity();
            }

        }

        // Assign a new Entity ID to the entity
        public void AssignNewEntityID(string entityID)
        {
            EntityObject.EntityID = entityID;

            // Get the current state of the entity.
            HassState = HassStates.GetHassState(EntityObject.EntityID);

            UpdateEntity();
            EntitySettingsWindowManager.Instance.UpdateEntitySettingsWindow(this);
        }

        protected abstract void UpdateEntity();

        public void DeleteEntity()
        {
            GameManager.Instance.RemoveEntity(EntityObject);
            Destroy(gameObject);
        }


        /// <summary>
        /// Temporarily highlights the entity by changing the color of its button to red.
        /// The highlighting is done by starting a coroutine that changes the color of the button to red
        /// and then waits for the specified duration and then changes the color back to the original color.
        /// </summary>
        public void HighlightEntity()
        {
            if (_setButtonColorTemporarilyCoroutine != null)
                return;

            _setButtonColorTemporarilyCoroutine = SetButtonColorTemporarily(Color.red, 10f);
            StartCoroutine(_setButtonColorTemporarilyCoroutine);
        }

        /// <summary>
        /// Temporarily sets the color of the entity's button to the given color for the given duration.
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
        private void OnSettingsButtonClicked() => EntitySettingsWindowManager.Instance.ToggleSettingsWindow(this);

        public EDeviceType GetDeviceType() => HassState.DeviceType;

        public void ReloadSettingsWindow() => EntitySettingsWindowManager.Instance.UpdateEntitySettingsWindow(this);

        public void SetWindowControlVisibility() => WindowControls.SetActive(!EntityObject.Settings.HideWindowControls);
        

        public void UpdateRotationBehaviour()
        {
            // toggle the LazyFollow component on/off
            if (EntityObject.Settings.AlignWindowToWall || !EntityObject.Settings.RotationEnabled)
            {
                _lazyFollow.enabled = false;
                _ = CreateNewAnchor();
            }
            else
                _lazyFollow.enabled = true;
            
            ReloadSettingsWindow();
        }
    }
}