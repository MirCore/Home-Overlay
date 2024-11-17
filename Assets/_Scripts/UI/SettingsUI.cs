using System.Collections;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Class handling the settings UI.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        /// <summary>
        /// The input field for the URL.
        /// </summary>
        [SerializeField] private TMP_InputField URLInputField;

        /// <summary>
        /// The input field for the port.
        /// </summary>
        [SerializeField] private TMP_InputField PortInputField;

        /// <summary>
        /// The input field for the token.
        /// </summary>
        [SerializeField] private TMP_InputField TokenInputField;

        /// <summary>
        /// The button to test the connection.
        /// </summary>
        [SerializeField] private Button TestConnectionButton;

        /// <summary>
        /// The button to save the settings.
        /// </summary>
        [SerializeField] private Button SaveButton;

        /// <summary>
        /// The game object to show when the connection is successful.
        /// </summary>
        [SerializeField] private GameObject ConnectSuccessField;

        /// <summary>
        /// The game object to show when the connection fails.
        /// </summary>
        [SerializeField] private GameObject ConnectFailField;

        /// <summary>
        /// The text to show the error code.
        /// </summary>
        [SerializeField] private TMP_Text ErrorCodeText;

        /// <summary>
        /// The game object to show when the settings are saved.
        /// </summary>
        [SerializeField] private GameObject SaveSuccessField;
        
        
        private void Awake()
        {
            ConnectSuccessField.SetActive(false);
            ConnectFailField.SetActive(false);
            SaveSuccessField.SetActive(false);
            
            // Load the saved URL
            if (GameManager.Instance.HassURL != null)
                URLInputField.text = GameManager.Instance.HassURL;

            // Load the saved port
            if (GameManager.Instance.HassPort != null)
                PortInputField.text = GameManager.Instance.HassPort;

            // Load the saved token
            TokenInputField.text = GameManager.Instance.HassToken;
        }

        private void OnEnable()
        {
            TestConnectionButton.onClick.AddListener(OnTestConnectionButtonClicked);
            SaveButton.onClick.AddListener(OnSaveButtonClicked);
            EventManager.OnConnectionTested += OnConnectionTested;
        }

        private void OnDisable()
        {
            TestConnectionButton.onClick.RemoveListener(OnTestConnectionButtonClicked);
            SaveButton.onClick.RemoveListener(OnSaveButtonClicked);
            EventManager.OnConnectionTested -= OnConnectionTested;
        }

        /// <summary>
        /// Called when the connection test is done.
        /// </summary>
        /// <param name="status">The status of the connection test.</param>
        private void OnConnectionTested(string status)
        {
            if (status is "200" or "201")
            {
                ConnectSuccessField.SetActive(true);
                ConnectFailField.SetActive(false);
            }
            else
            {
                ConnectSuccessField.SetActive(false);
                ConnectFailField.SetActive(true);
                ErrorCodeText.text = $"Connection error: {status}";
            }
        }

        /// <summary>
        /// Called when the test connection button is clicked.
        /// </summary>
        private void OnTestConnectionButtonClicked()
        {
            int.TryParse(PortInputField.text, out int port);
            
            GameManager.TestConnection(URLInputField.text, port, TokenInputField.text);
            
            ConnectSuccessField.SetActive(false);
            ConnectFailField.SetActive(false);
        }

        /// <summary>
        /// Called when the save button is clicked.
        /// </summary>
        private void OnSaveButtonClicked()
        {
            int.TryParse(PortInputField.text, out int port);
            GameManager.Instance.SaveConnectionSettings(URLInputField.text, port, TokenInputField.text);
            SaveSuccessField.SetActive(true);
            
            // Start coroutine to deactivate the field after 3 seconds
            StartCoroutine(DeactivateAfterDelay(3f));
        }
        
        
        /// <summary>
        /// Deactivates the save success field after a delay.
        /// </summary>
        /// <param name="delay">The delay in seconds.</param>
        private IEnumerator DeactivateAfterDelay(float delay)
        {
            // Wait for the specified time
            yield return new WaitForSeconds(delay);

            // Deactivate the field
            SaveSuccessField.SetActive(false);
        }
    }
}