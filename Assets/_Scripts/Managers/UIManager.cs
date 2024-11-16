using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text HeaderText;
        [SerializeField] private GameObject NewDeviceTab;
        [SerializeField] private GameObject SettingsTab;
        [SerializeField] private Button NewDeviceButton;
        [SerializeField] private Button SettingsButton;

        private void Start()
        {
            ShowNewDeviceTab();
        }

        private void OnEnable()
        {
            NewDeviceButton.onClick.AddListener(ShowNewDeviceTab);
            SettingsButton.onClick.AddListener(ShowSettingsTab);
        }

        private void OnDisable()
        {
            NewDeviceButton.onClick.RemoveListener(ShowNewDeviceTab);
            SettingsButton.onClick.RemoveListener(ShowSettingsTab);
        }
        
        private void ShowNewDeviceTab()
        {
            NewDeviceTab.SetActive(true);
            SettingsTab.SetActive(false);
            
            HeaderText.text = "Add new Device";
        }
        
        private void ShowSettingsTab()
        {
            NewDeviceTab.SetActive(false);
            SettingsTab.SetActive(true);
            
            HeaderText.text = "Settings";
        }
    
    }

}
