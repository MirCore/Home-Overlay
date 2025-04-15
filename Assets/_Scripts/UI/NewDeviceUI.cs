using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NewDeviceUI : MonoBehaviour
    {
        /// <summary>
        /// The Button that creates an empty new panel.
        /// </summary>
        [SerializeField] private Button CreateEmtpyEntityButton;
        
        /// <summary>
        /// The currently selected device type.
        /// </summary>
        private EDeviceType _selectedEDeviceType;


        private void OnEnable()
        {
            CreateEmtpyEntityButton.onClick.AddListener(OnCreateEmptyEntityButtonClicked);
        }

        private void OnDisable()
        {
            CreateEmtpyEntityButton.onClick.RemoveListener(OnCreateEmptyEntityButtonClicked);
        }

        /// <summary>
        /// Handles the click event of the OnCreateEmptyEntityButtonClicked.
        /// Spawns a new empty panel at the position of the CreateEmtpyEntityButton with the selected panel ID.
        /// </summary>
        private void OnCreateEmptyEntityButtonClicked()
        {
            // Spawn a new panel at the position of the CreateEmtpyEntityButton with the selected panel ID
            //PanelManager.Instance.SpawnNewEmptyEntity(CreateEmtpyEntityButton.transform.position);
            
            //UIManager.Instance.CloseMainMenu();

            Debug.LogWarning("Method is disabled");
        }
        
    }
}
