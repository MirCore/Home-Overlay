using System;
using System.Collections;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    /// <summary>
    /// Manages the UI for displaying connection status alerts to the user.
    /// </summary>
    public class ConnectionAlertUI : MonoBehaviour
    {
        [SerializeField] private GameObject AlertWindow;
        [SerializeField] private CanvasFader CanvasFader;
        [SerializeField] private float CloseTime = 5f;
        [SerializeField] private TMP_Text TitleText;
        [SerializeField] private TMP_Text DescriptionText;
        [SerializeField] private Image Icon;
        [SerializeField] private Button CloseButton;
        private TMP_Text _buttonText;

        /// <summary>
        /// Initializes event listeners and UI components when the object becomes enabled.
        /// </summary>
        private void OnEnable()
        {
            EventManager.OnConnectionTested += OnConnectionTested;
            CloseButton.onClick.AddListener(CloseAlertWindow);
            _buttonText = CloseButton.GetComponentInChildren<TMP_Text>();
        }

        /// <summary>
        /// Closes the alert window with animation if CanvasFader is available.
        /// </summary>
        private void CloseAlertWindow()
        {
            SoundManager.OnUIPressed();
            if (CanvasFader)
                CanvasFader.FadeOut(AlertWindow);
            else
                AlertWindow.SetActive(false);
        }

        /// <summary>
        /// Removes event listeners when the object becomes disabled.
        /// </summary>
        private void OnDisable()
        {
            EventManager.OnConnectionTested -= OnConnectionTested;
            CloseButton.onClick.RemoveListener(() => AlertWindow.SetActive(false));
        }
        
        /// <summary>
        /// Handles the connection test event and updates the alert UI accordingly.
        /// </summary>
        /// <param name="status">HTTP status code from the connection test</param>
        /// <param name="uri">URI that was tested</param>
        private void OnConnectionTested(int status, Uri uri)
        {
            Icon.gameObject.SetActive(false);
            switch (status)
            {
                case 200 or 201:
                    TitleText.text = "Connection successful!";
                    DescriptionText.text = "You are now connected to Home Assistant!";
                    break;
                case 412:
                    TitleText.text = "Connection failed!";
                    DescriptionText.text = "No connection data found. Please add connection info.";
                    break;
                default:
                    TitleText.text = "Connection failed!";
                    DescriptionText.text = $"Connection error: {status} {HttpStatusCodes.GetDescription(status)}";
                    break;
            }
            
            StartCoroutine(CloseAlertWindowCoroutine());
        }

        /// <summary>
        /// Coroutine that manages the countdown timer for auto-closing the alert window.
        /// </summary>
        /// <returns>An IEnumerator for the coroutine system.</returns>
        private IEnumerator CloseAlertWindowCoroutine()
        {
            int timer = (int)CloseTime;
            while (timer > 0)
            {
                timer--;
                _buttonText.text = $"({timer}) Close";
                yield return new WaitForSeconds(1);
            }
            CloseAlertWindow();
        }
    }
}