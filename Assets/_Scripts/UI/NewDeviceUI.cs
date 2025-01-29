using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NewDeviceUI : MonoBehaviour
    {
        /// <summary>
        /// The Button that creates an empty new entity.
        /// </summary>
        [SerializeField] private Button CreateEmtpyEntityButton;
        
        /// <summary>
        /// The EntitySpawner that spawns new entities.
        /// </summary>
        [SerializeField] private EntitySpawner EntitySpawner;
        
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
        /// Spawns a new empty entity at the position of the CreateEmtpyEntityButton with the selected entity ID.
        /// </summary>
        private void OnCreateEmptyEntityButtonClicked()
        {
            // Spawn a new entity at the position of the CreateEmtpyEntityButton with the selected entity ID
            EntitySpawner.SpawnNewEntity(null, CreateEmtpyEntityButton.transform.position);
        }
        
    }
}
