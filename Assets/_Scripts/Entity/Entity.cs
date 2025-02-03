using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Utils;

namespace Entity
{
    public abstract class Entity : MonoBehaviour
    {
        [SerializeField] private Image HighlightImage;
        [SerializeField] internal TMP_Text Icon;
        [SerializeField] private Button SettingsButton;
        private XRBaseInteractable _interactable;

        protected HassEntity EntityState;
        public EntityObject EntityObject { get; private set; }

        private ARAnchorManager _arAnchorManager;
        private ARAnchor _anchor;

        /// <summary>
        /// A coroutine that is currently setting the color of the button temporarily.
        /// </summary>
        private IEnumerator _setButtonColorTemporarilyCoroutine;

        protected virtual void OnEnable()
        {
            _arAnchorManager = GameManager.Instance.ARAnchorManager;
            _arAnchorManager.trackablesChanged.AddListener(OnAnchorChanged);

            SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
            _interactable = GetComponent<XRBaseInteractable>();
            _interactable.selectExited.AddListener(OnSelectExited);
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
        }

        protected virtual void OnDisable()
        {
            _arAnchorManager.trackablesChanged.RemoveListener(OnAnchorChanged);

            SettingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            _interactable.selectExited.RemoveListener(OnSelectExited);
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }

        /// <summary>
        /// Listens for when AR anchors are added to the scene, and parents this entity to the first anchor that matches the EntityObject's AnchorID.
        /// </summary>
        private void OnAnchorChanged(ARTrackablesChangedEventArgs<ARAnchor> changes)
        {
            if (_anchor != null)
                return;

            foreach (ARAnchor addedAnchor in changes.added.Where(addedAnchor => EntityObject.AnchorID == addedAnchor.trackableId.ToString()))
            {
                SetParentToAnchor(addedAnchor);
                Debug.Log("Entity parented to added anchor");
            }
        }

        private void OnSelectExited(SelectExitEventArgs eventData)
        {
            if (EntityObject == null)
                return;

            CreateAnchorAsync();

            EntityObject.Position = transform.position;
            EntityObject.Rotation = transform.rotation;
            EntityObject.Scale = transform.localScale;
        }

        /// <summary>
        /// Sets the parent of the entity to the specified ARAnchor.
        /// </summary>
        /// <param name="newAnchor">The new anchor to attach to.</param>
        private void SetParentToAnchor(ARAnchor newAnchor)
        {
            _anchor = newAnchor;
            // Set the entity's parent to the anchor
            transform.SetParent(_anchor.transform, false);
            // Align the entity's position with the anchor
            transform.position = _anchor.transform.position;
            // Update the AnchorID in the entity object
            EntityObject.AnchorID = _anchor.trackableId.ToString();
            //Debug.Log("Attached to anchor: " + EntityObject.AnchorID);
        }

        /// <summary>
        /// Asynchronously creates an ARAnchor at the entity's current pose.
        /// </summary>
        private async void CreateAnchorAsync()
        {
            // Attempt to add a new anchor at the current position and rotation
            Result<ARAnchor> result = await _arAnchorManager.TryAddAnchorAsync(new Pose(transform.position, transform.rotation));
            if (!result.status.IsSuccess())
            {
                // Log a warning if the anchor creation fails
                Debug.LogWarning("Failed to create anchor");
                return;
            }

            ARAnchor oldAnchor = _anchor; // Store reference to the old anchor
            ARAnchor newAnchor = result.value; // Get the newly created anchor

            SetParentToAnchor(newAnchor); // Re-parent the entity to the new anchor

            if (oldAnchor != null)
            {
                // Remove the old anchor if it exists
                bool deleted = _arAnchorManager.TryRemoveAnchor(oldAnchor);
                //if (deleted)
                //    Debug.Log("Deleted old anchor");
            }
        }

        private void OnSettingsButtonClicked()
        {
            EntitySettingsWindowManager.Instance.ToggleSettingsWindow(this);
        }

        /// <summary>
        /// Called when Hass states change. Updates the icon.
        /// </summary>
        private void OnHassStatesChanged()
        {
            // Get the current state of the entity.
            EntityState ??= HassStates.GetHassState(EntityObject.EntityID);

            UpdateEntity();
        }


        /// <summary>
        /// Sets the entity object and updates the icon and anchor accordingly.
        /// </summary>
        /// <param name="entityObject">The entity object to be associated with this entity.</param>
        public void SetEntityObject(EntityObject entityObject)
        {
            // Assign the provided entity object to the current entity
            EntityObject = entityObject;
            // Add the entity object to the GameManager's list of entities
            GameManager.Instance.AddEntity(EntityObject, this);
            AttachToAnchor();

            // If there is no entity ID, there is nothing to update.
            if (EntityObject.EntityID == null)
            {
                Icon.text = "";
                return;
            }

            // Get the current state of the entity.
            EntityState ??= HassStates.GetHassState(EntityObject.EntityID);

            if (EntityState != null)
            {
                // Update the icon to reflect the current state of the entity
                UpdateEntity();
            }

        }

        private void AttachToAnchor()
        {
            // If the entity object does not have an anchor ID, exit the method
            if (string.IsNullOrEmpty(EntityObject.AnchorID))
                return;

            // Retrieve the anchor using the anchor ID
            TrackableId trackableId = new(EntityObject.AnchorID);
            ARAnchor anchor = _arAnchorManager.GetAnchor(trackableId);

            // If an anchor is found, set the entity's parent to the anchor
            if (anchor != null)
            {
                SetParentToAnchor(anchor);
            }
            else
            {
                // Log a message if no anchor is found
                Debug.Log("No anchor loaded");
            }
        }

        public void UpdateEntityID(string entityID)
        {
            EntityObject.EntityID = entityID;

            // Get the current state of the entity.
            EntityState = HassStates.GetHassState(EntityObject.EntityID);

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

        public EDeviceType GetDeviceType()
        {
            return EntityState.DeviceType;
        }

        public void ReloadSettingsWindow()
        {
            EntitySettingsWindowManager.Instance.UpdateEntitySettingsWindow(this);
        }

        public void OnScaled()
        {
            EntityObject.Scale = transform.localScale;
        }
    }
}