using System;
using System.Collections;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class ConnectionAlertUI : MonoBehaviour
    {
        [SerializeField] private GameObject AlertWindow;
        [SerializeField] private float CloseTime = 5f;
        [SerializeField] private TMP_Text TitleText;
        [SerializeField] private TMP_Text DescriptionText;
        [SerializeField] private Image Icon;
        [SerializeField] private Button CloseButton;
        private TMP_Text _buttonText;

        private void OnEnable()
        {
            EventManager.OnConnectionTested += OnConnectionTested;
            CloseButton.onClick.AddListener(() => AlertWindow.SetActive(false));
            _buttonText = CloseButton.GetComponentInChildren<TMP_Text>();
        }

        private void OnDisable()
        {
            EventManager.OnConnectionTested -= OnConnectionTested;
            CloseButton.onClick.RemoveListener(() => AlertWindow.SetActive(false));
        }
        
        private void OnConnectionTested(int status)
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
            
            StartCoroutine(CloseAlertWindow());
        }

        

        private IEnumerator CloseAlertWindow()
        {
            int timer = (int)CloseTime;
            while (timer > 0)
            {
                timer--;
                _buttonText.text = $"({timer}) Close";
                yield return new WaitForSeconds(1);
            }
            AlertWindow.SetActive(false);
        }
    }
}