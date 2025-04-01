using System.Collections;
using System.Text;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

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

        
        [SerializeField] private Button LoadTokenButton;
        [SerializeField] private Button PasteTokenButton;
        
        [SerializeField] private Button ChangeConnectionSettingsButton;
        
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
            if (GameManager.Instance.HassURL != "")
                URLInputField.text = GameManager.Instance.HassURL;

            // Load the saved port
            if (GameManager.Instance.HassPort != 0)
                PortInputField.text = GameManager.Instance.HassPort.ToString();

            // Load the saved token
            TokenInputField.text = GameManager.Instance.HassToken;
        }

        private void OnEnable()
        {
            LoadTokenButton.onClick.AddListener(OnLoadTokenButtonClicked);
            PasteTokenButton.onClick.AddListener(OnPasteTokenButtonClicked);
            TestConnectionButton.onClick.AddListener(OnTestConnectionButtonClicked);
            ChangeConnectionSettingsButton.onClick.AddListener(ChangeConnectionSettings);
            SaveButton.onClick.AddListener(OnSaveButtonClicked);
            EventManager.OnConnectionTested += OnConnectionTested;

#if UNITY_VISIONOS
            URLInputField.interactable = false;
            PortInputField.interactable = false;
            TokenInputField.interactable = false;
            SwiftUIDriver.OpenSwiftUIWindow("MainMenu");
#endif
        }

        private void OnDisable()
        {
            LoadTokenButton.onClick.RemoveListener(OnLoadTokenButtonClicked);
            PasteTokenButton.onClick.RemoveListener(OnPasteTokenButtonClicked);
            TestConnectionButton.onClick.RemoveListener(OnTestConnectionButtonClicked);
            ChangeConnectionSettingsButton.onClick.RemoveListener(ChangeConnectionSettings);
            SaveButton.onClick.RemoveListener(OnSaveButtonClicked);
            EventManager.OnConnectionTested -= OnConnectionTested;
           
#if UNITY_VISIONOS 
            SwiftUIDriver.CloseSwiftUIWindow("MainMenu");
#endif
        }

        private void ChangeConnectionSettings()
        {
#if UNITY_VISIONOS
            SwiftUIDriver.OpenSwiftUIWindow("MainMenu");
#endif
        }

        private void OnPasteTokenButtonClicked()
        {
            string clipboard = GUIUtility.systemCopyBuffer;

            if (string.IsNullOrEmpty(clipboard))
            {
                TextEditor te = new ();
                te.Paste();
                clipboard = te.text;
            }
            TokenInputField.text = clipboard;
        }

        private void OnLoadTokenButtonClicked()
        {
            FileBrowserUtility.LoadStringFromFile(this);
        }

        /// <summary>
        /// Called when the connection test is done.
        /// </summary>
        /// <param name="status">The status of the connection test.</param>
        private void OnConnectionTested(int status)
        {
            if (status is 200 or 201)
            {
                ConnectSuccessField.SetActive(true);
                ConnectFailField.SetActive(false);
            }
            else
            {
                ConnectSuccessField.SetActive(false);
                ConnectFailField.SetActive(true);
                ErrorCodeText.text = $"Connection error: {status} {HttpStatusCodes.GetDescription(status)}";
            }
        }

        /// <summary>
        /// Called when the test connection button is clicked.
        /// </summary>
        private void OnTestConnectionButtonClicked()
        {
            int.TryParse(PortInputField.text, out int port);
            if (port == 0)
                port = 8123;
            
            string url = URLInputField.text;
            if (url == "")
                url = "http://homeassistant.local/";
            
            TestConnection(url, port, TokenInputField.text);
        }

        internal void TestConnection(string url, int portInt, string token)
        {
            //Debug.Log("Testing connection at " + url + ":" + portInt + " with token " + token);
            RestHandler.TestConnection(url, portInt, token);
            
            ConnectSuccessField.SetActive(false);
            ConnectFailField.SetActive(false);
        }

        /// <summary>
        /// Called when the save button is clicked.
        /// </summary>
        private void OnSaveButtonClicked()
        {
            int.TryParse(PortInputField.text, out int port);
            if (port == 0)
                port = 8123;
            
            string url = URLInputField.text;
            if (url == "")
                url = "http://homeassistant.local/";
            
            SaveConnectionSettings(url, port, TokenInputField.text);
        }

        internal void SaveConnectionSettings(string url, int port, string token)
        {
            GameManager.Instance.SaveConnectionSettings(url, port, token);
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

        public void OnTokenLoadedFromFile(byte[] bytes)
        {
            TokenInputField.text = Encoding.UTF8.GetString(bytes);
        }
    }
}