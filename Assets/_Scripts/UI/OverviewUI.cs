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
        /// Shows the entity panels by enabling or instantiating them as needed.
        /// </summary>
        private void ShowEntityPanels()
        {
            // Initialize an index to keep track of the current entity panel
            int index = 0;

            // Iterate over all entity objects in the game manager
            foreach (EntityObject entity in GameManager.Instance.EntityObjects)
            {
                // Get the entity panel, either by reusing an existing one or instantiating a new one
                GameObject overviewUIEntityPanel;
                if (index < _entityPanels.Count)
                {
                    // Reuse an existing entity panel
                    overviewUIEntityPanel = _entityPanels[index];
                    overviewUIEntityPanel.SetActive(true);
                }
                else
                {
                    // Instantiate a new entity panel
                    overviewUIEntityPanel = Instantiate(EntityPanelPrefab, transform);
                    _entityPanels.Add(overviewUIEntityPanel);
                }

                // Set the entity object for the friendly name handler
                overviewUIEntityPanel.GetComponent<FriendlyNameHandler>().SetNewEntity(entity);
                Button[] buttons = overviewUIEntityPanel.GetComponentsInChildren<Button>();
                buttons[0].onClick.AddListener(() => OnHighlightEntityButtonPressed(entity));
                buttons[1].onClick.AddListener(() => OnDeleteEntityButtonPressed(entity));

                // Increment the index for the next entity panel
                index++;
            }

            // Hide any remaining entity panels that are not needed
            for (int i = index; i < _entityPanels.Count; i++)
            {
                _entityPanels[i].SetActive(false);
            }
        }

        private void OnHighlightEntityButtonPressed(EntityObject entity)
        {
            entity.Entity.HighlightEntity();
        }

        private void OnDeleteEntityButtonPressed(EntityObject entity)
        {
            GameManager.Instance.EntityObjects.FirstOrDefault(e => e.ID == entity.ID)?.DeleteEntity();
            ShowEntityPanels();
        }
    }
}
