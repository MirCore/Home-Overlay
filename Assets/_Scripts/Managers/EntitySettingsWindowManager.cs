using System.Collections.Generic;
using UI;
using UnityEngine;
using Utils;

namespace Managers
{
    /// <summary>
    /// Manages all Entity Settings Windows. Provides a way to spawn a new window for an entity.
    /// </summary>
    public class EntitySettingsWindowManager : Singleton<EntitySettingsWindowManager>
    {
        [SerializeField] private EntitySettingsUI EntitySettingsWindowPrefab;

        /// <summary>
        /// List of all Entity Settings Windows.
        /// </summary>
        private readonly List<EntitySettingsUI> _entitySettingsWindows = new();

        /// <summary>
        /// Toggles the Entity Settings Window for the given entity.
        /// </summary>
        /// <param name="entity">The entity to toggle the settings window for.</param>
        public void ToggleSettingsWindow(Entity entity)
        {
            EntitySettingsUI entitySettingsUI = _entitySettingsWindows.Find(w => w.Entity == entity);
            if (entitySettingsUI)
            {
                entitySettingsUI.gameObject.SetActive(!entitySettingsUI.gameObject.activeSelf);
            }
            else
            {
                SpawnNewWindow(entity);
            }
        }

        /// <summary>
        /// Spawns a new Entity Settings Window for the given entity.
        /// </summary>
        /// <param name="entity">The entity to spawn the settings window for.</param>
        private void SpawnNewWindow(Entity entity)
        {
            EntitySettingsUI newSettingsWindow = Instantiate(EntitySettingsWindowPrefab, entity.transform.position, entity.transform.rotation);

            // Find the Canvas of the entity
            Canvas entityCanvas = entity.transform.GetComponentInChildren<Canvas>();
            if (entityCanvas)
            {
                RectTransform entityCanvasRect = entityCanvas.GetComponent<RectTransform>();
                RectTransform entitySettingsWindowRect = newSettingsWindow.GetComponent<RectTransform>();

                if (entityCanvasRect != null && entitySettingsWindowRect != null)
                {
                    // Ensure the settings window is within the same Canvas hierarchy
                    newSettingsWindow.transform.SetParent(entity.transform, true);

                    // Calculate the new position: directly to the left of the canvas
                    float offset = entityCanvasRect.rect.width * entityCanvasRect.transform.localScale.x / 2 + entitySettingsWindowRect.rect.width * entitySettingsWindowRect.transform.localScale.x / 2;
                    //float offset = entity.GetComponent<MeshRenderer>().bounds.size.x / 2 + newSettingsWindow.GetComponent<MeshRenderer>().bounds.size.x / 2;
                    entitySettingsWindowRect.anchoredPosition = new Vector2(offset, 0);
                }
            }
            else
            {
                Debug.Log("No Canvas found for entity: " + entity.name);
            }

            _entitySettingsWindows.Add(newSettingsWindow);

            newSettingsWindow.SetEntity(entity);
        }

        public void UpdateEntitySettingsWindow(Entity entity)
        {
            EntitySettingsUI entitySettingsUI = _entitySettingsWindows.Find(w => w.Entity == entity);
            if (entitySettingsUI)
            {
                entitySettingsUI.gameObject.SetActive(false);
                entitySettingsUI.gameObject.SetActive(true);
            }
            else
            {
                SpawnNewWindow(entity);
            }
        }
    }
}