using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Structs;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OverviewUI : MonoBehaviour
    {
        [SerializeField] private GameObject EntityPanelPrefab;
        private readonly List<GameObject> _entityPanels = new ();

        private void OnEnable()
        {
            ShowEntityPanels();
            EventManager.OnAppStateLoaded += ShowEntityPanels;
        }

        private void OnDisable()
        {
            EventManager.OnAppStateLoaded -= ShowEntityPanels;
        }

        /// <summary>
        /// Shows the panel panels by enabling or instantiating them as needed.
        /// </summary>
        private void ShowEntityPanels()
        {
            // Initialize an index to keep track of the current panel
            int index = 0;

            // Iterate over all panel objects in the game manager
            foreach (PanelData panelData in PanelManager.Instance.PanelDatas)
            {
                // Get the panel, either by reusing an existing one or instantiating a new one
                GameObject overviewUIEntityPanel;
                if (index < _entityPanels.Count)
                {
                    // Reuse an existing panel panel
                    overviewUIEntityPanel = _entityPanels[index];
                    overviewUIEntityPanel.SetActive(true);
                }
                else
                {
                    // Instantiate a new panel panel
                    overviewUIEntityPanel = Instantiate(EntityPanelPrefab, transform);
                    _entityPanels.Add(overviewUIEntityPanel);
                }

                // Set the panel object for the friendly name handler
                overviewUIEntityPanel.GetComponent<FriendlyNameHandler>().SetNewEntity(panelData);
                Button[] buttons = overviewUIEntityPanel.GetComponentsInChildren<Button>();
                buttons[0].onClick.AddListener(() => OnHighlightPanelButtonPressed(panelData));
                buttons[1].onClick.AddListener(() => OnDeletePanelButtonPressed(panelData));

                // Increment the index for the next panel
                index++;
            }

            // Hide any remaining panel panels that are not needed
            for (int i = index; i < _entityPanels.Count; i++)
            {
                _entityPanels[i].SetActive(false);
            }
        }

        private void OnHighlightPanelButtonPressed(PanelData panelData)
        {
            panelData.Panel.HighlightPanel();
        }

        private void OnDeletePanelButtonPressed(PanelData entity)
        {
            PanelManager.Instance.PanelDatas.FirstOrDefault(e => e.ID == entity.ID)?.DeletePanel();
            ShowEntityPanels();
        }
    }
}
