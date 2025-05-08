using Structs;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OverviewUIPanel : MonoBehaviour
    {
        [SerializeField] private GameObject NotSpawnedNotice;
        [SerializeField] internal Button HighlightButton;
        [SerializeField] internal Button DeleteButton;
        [SerializeField] private FriendlyNameHandler FriendlyNameHandler;

        public void SetNewEntity(PanelData panelData)
        {
            if (panelData.Panel == null)
            {
                NotSpawnedNotice.SetActive(true);
                HighlightButton.gameObject.SetActive(false);
            }
            else
            {
                NotSpawnedNotice.SetActive(false);
                HighlightButton.gameObject.SetActive(true);
            }
            FriendlyNameHandler.SetNewEntity(panelData);
        }
    }
}
