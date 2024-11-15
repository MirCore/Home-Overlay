using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField URLInputField;
        [SerializeField] private TMP_InputField PortInputField;
        [SerializeField] private TMP_InputField TokenInputField;
        [SerializeField] private Button TestConnectionButton;
        [SerializeField] private Button SaveButton;
        [SerializeField] private GameObject ConnectSuccessField;
        [SerializeField] private GameObject ConnectFailField;
        [SerializeField] private TMP_Text ErrorCodeText;
        [SerializeField] private GameObject SaveSuccessField;
        
        private void Awake()
        {
            ConnectSuccessField.SetActive(false);
            ConnectFailField.SetActive(false);
            SaveSuccessField.SetActive(false);
            
            if (GameManager.Instance.HassURL != null)
                URLInputField.text = GameManager.Instance.HassURL;
            if (GameManager.Instance.HassPort != null)
                PortInputField.text = GameManager.Instance.HassPort;
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

        private void OnTestConnectionButtonClicked()
        {
            int.TryParse(PortInputField.text, out int port);
            
            GameManager.Instance.TestConnection(URLInputField.text, port, TokenInputField.text);
            
            ConnectSuccessField.SetActive(false);
            ConnectFailField.SetActive(false);
        }

        private void OnSaveButtonClicked()
        {
            int.TryParse(PortInputField.text, out int port);
            GameManager.Instance.SaveConnectionSettings(URLInputField.text, port, TokenInputField.text);
            SaveSuccessField.SetActive(true);
            
            // Start coroutine to deactivate the field after 3 seconds
            StartCoroutine(DeactivateAfterDelay(3f));
        }
        
        
        private IEnumerator DeactivateAfterDelay(float delay)
        {
            // Wait for the specified time
            yield return new WaitForSeconds(delay);

            // Deactivate the field
            SaveSuccessField.SetActive(false);
        }
    }
}
