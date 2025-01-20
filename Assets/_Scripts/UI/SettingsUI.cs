using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Web;
using AOT;
using Managers;
#if QUEST_BUILD && FALSE
using Meta.XR.MRUtilityKit;
#endif
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

#if UNITY_VISIONOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

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
        
        /// <summary>
        /// The button to test the connection.
        /// </summary>
        [SerializeField] private Button TestConnectionButton;

        /// <summary>
        /// The button to save the settings.
        /// </summary>
        [SerializeField] private Button SaveButton;
        
        [SerializeField] private Toggle ToggleEffectMeshButton;

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
            SaveButton.onClick.AddListener(OnSaveButtonClicked);
            ToggleEffectMeshButton.onValueChanged.AddListener(OnToggleEffectMeshButtonClicked);
            EventManager.OnConnectionTested += OnConnectionTested;
            
            SetNativeCallback(SettingsCallbackFromNative);
            OpenSwiftUISettingsWindow("SettingsWindow", GameManager.Instance.HassURL, GameManager.Instance.HassPort.ToString(), GameManager.Instance.HassToken);
        }

        private void OnDisable()
        {
            LoadTokenButton.onClick.RemoveListener(OnLoadTokenButtonClicked);
            PasteTokenButton.onClick.RemoveListener(OnPasteTokenButtonClicked);
            TestConnectionButton.onClick.RemoveListener(OnTestConnectionButtonClicked);
            SaveButton.onClick.RemoveListener(OnSaveButtonClicked);
            ToggleEffectMeshButton.onValueChanged.RemoveListener(OnToggleEffectMeshButtonClicked);
            EventManager.OnConnectionTested -= OnConnectionTested;
            
            SetNativeCallback(null);
            CloseSwiftUIWindow("SettingsWindow");
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
            /*string fileType = NativeFilePicker.ConvertExtensionToFileType("txt");
            // Pick a file
            NativeFilePicker.Permission permission = NativeFilePicker.PickFile( ( path ) =>
            {
                if( path == null )
                    Debug.Log( "Operation cancelled" );
                else
                {
                    byte[] file = System.IO.File.ReadAllBytes(path);
                    OnTokenLoadedFromFile(file);
                    Debug.Log("Picked file: " + path);
                }
            },  fileType );

            Debug.Log( "Permission result: " + permission );*/
            
            FileBrowserUtility.LoadStringFromFile(this);
        }
        
        private void OnToggleEffectMeshButtonClicked(bool value)
        {
#if QUEST_BUILD && FALSE
            GameManager.Instance.EffectMesh.HideMesh = !value;
#endif
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
        
        private void TestConnection(string url, int portInt, string token)
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

        private void SaveConnectionSettings(string url, int port, string token)
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
        
        
        private delegate void SwiftCallbackDelegate(string command, string url, string port, string token);

        // This attribute is required for methods that are going to be called from native code
        // via a function pointer.
        [MonoPInvokeCallback(typeof(SwiftCallbackDelegate))]
        private static void SettingsCallbackFromNative(string command, string url, string port, string token)
        {
            // MonoPInvokeCallback methods will leak exceptions and cause crashes; always use a try/catch in these methods
            try
            {
                Debug.Log($"Received Command: {command}");
                Debug.Log($"Received URL: {url}");
                Debug.Log($"Received Port: {port}");
                Debug.Log($"Received Token: {token}");

                // This could be stored in a static field or a singleton.
                // If you need to deal with multiple windows and need to distinguish between them,
                // you could add an ID to this callback and use that to distinguish windows.
                SettingsUI self = FindFirstObjectByType<SettingsUI>();
                
                
                int.TryParse(port, out int portInt);

                switch (command)
                {
                    case "test connection":
                        self.TestConnection(url, portInt, token);
                        return;
                    case "save connection":
                        self.SaveConnectionSettings(url, portInt, token);
                        return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

#if UNITY_VISIONOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void SetNativeCallback(SwiftCallbackDelegate swiftCallback);

        [DllImport("__Internal")]
        static extern void OpenSwiftUISettingsWindow(string name, string url, string port, string token);

        [DllImport("__Internal")]
        static extern void CloseSwiftUIWindow(string name);

#else
        static void SetNativeCallback(SwiftCallbackDelegate swiftCallback) {}
        static void OpenSwiftUISettingsWindow(string name, string url, string port, string token) {}
        static void CloseSwiftUIWindow(string name) {}

#endif
    }
}