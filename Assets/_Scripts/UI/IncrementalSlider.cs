using System;
using Managers;
using Structs;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UI
{
    /// <summary>
    /// IncrementalSlider is a custom XRBaseInteractable that allows for incremental changes based on the movement of the interactor.
    /// </summary>
    public class IncrementalSlider : XRBaseInteractable
    {
        [SerializeField] private float DragAffordance = 0.01f; // Distance threshold for a change
        [SerializeField] private float StepSize = 1f; // Step size for changes
        private float _currentDelta; // Current intensity delta
        private bool _isDragging; // Flag to track if dragging is in progress

        private IXRSelectInteractor _interactor; // Reference to the current interactor
        private Vector3 _lastInteractorPosition; // Last position of the interactor
        
        /// <summary>
        /// Event triggered when the slider value changes.
        /// The first parameter is the intensity delta, and the second parameter indicates if this is the first drag event.
        /// </summary>
        public event Action<float, bool> OnSliderValueChanged;

        public event Action OnClick;

        /// <summary>
        /// Called when the interactor starts selecting this interactable.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            OnClick?.Invoke();
            _currentDelta = 0;
            _interactor = args.interactorObject;
            _lastInteractorPosition = _interactor.transform.position;
        }

        /// <summary>
        /// Called when the interactor stops selecting this interactable.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            
            OnClick?.Invoke();
            // Reset interactor and isDragging
            _interactor = null;
            _isDragging = false;
        }

        private void Update()
        {
            if (_interactor == null)
                return;
            
            HandleDragging();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Handles the dragging logic for the incremental slider.
        /// </summary>
        private void HandleDragging()
        {
            // Get the current position of the interactor and calculate the vertical movement delta since the last frame
            Vector3 currentPos = _interactor.transform.position;
            float deltaY = currentPos.y - _lastInteractorPosition.y;

            // Check if the movement exceeds the drag affordance threshold
            if (Mathf.Abs(deltaY) < DragAffordance)
                return;
            
            // Determine the direction and calculate the new intensity delta based on the direction and step size
            int direction = Math.Sign(deltaY);
            float delta = _currentDelta + (direction * StepSize);
            
            if (Mathf.Approximately(_currentDelta, delta) && delta <= StepSize)
                return;
            
            _currentDelta = direction * StepSize;
            
            
            // Invoke the event to notify listeners of the change in slider value
            OnSliderValueChanged?.Invoke(_currentDelta, !_isDragging);
            
            // Set the flag to indicate that dragging is in progress
            _isDragging = true;

            // Reset reference point to allow continued dragging
            _lastInteractorPosition = currentPos;
        }
    }
}