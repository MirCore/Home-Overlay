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
        [SerializeField] private OverviewUIPanel EntityPanelPrefab;
        [SerializeField] private Transform ScrollViewContent;
        private readonly List<OverviewUIPanel> _entityPanels = new ();

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
            foreach (PanelData panelData in PanelManager.Instance.PanelDataList)
            {
                // Get the panel, either by reusing an existing one or instantiating a new one
                OverviewUIPanel overviewUIPanel;
                if (index < _entityPanels.Count)
                {
                    // Reuse an existing panel panel
                    overviewUIPanel = _entityPanels[index];
                    overviewUIPanel.gameObject.SetActive(true);
                }
                else
                {
                    // Instantiate a new panel panel
                    overviewUIPanel = Instantiate(EntityPanelPrefab, ScrollViewContent);
                    _entityPanels.Add(overviewUIPanel);
                }

                // Set the panel object for the friendly name handler
                overviewUIPanel.SetNewEntity(panelData);
                Button[] buttons = overviewUIPanel.GetComponentsInChildren<Button>();
                overviewUIPanel.HighlightButton.onClick.AddListener(() => OnHighlightPanelButtonPressed(panelData));
                overviewUIPanel.DeleteButton.onClick.AddListener(() => OnDeletePanelButtonPressed(panelData));

                // Increment the index for the next panel
                index++;
            }

            // Hide any remaining panel panels that are not needed
            for (int i = index; i < _entityPanels.Count; i++)
            {
                _entityPanels[i].gameObject.SetActive(false);
            }
        }

        private static void OnHighlightPanelButtonPressed(PanelData panelData)
        {
            PanelManager.Instance.HighlightPanel(panelData.ID);
        }

        private void OnDeletePanelButtonPressed(PanelData panel)
        {
            PanelManager.Instance.DeletePanel(panel.ID);
            ShowEntityPanels();
        }
    }
}
