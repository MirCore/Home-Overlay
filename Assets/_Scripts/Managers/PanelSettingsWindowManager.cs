using System.Collections.Generic;
using Panels;
using UI;
using UnityEngine;
using Utils;

namespace Managers
{
    /// <summary>
    /// Manages all Panel Settings Windows. Provides a way to spawn a new window for an panel.
    /// </summary>
    public class PanelSettingsWindowManager : Singleton<PanelSettingsWindowManager>
    {
        [SerializeField] private PanelSettingsUI PanelSettingsWindowPrefab;
        [SerializeField] private float WindowPositionOffset = 15;

        /// <summary>
        /// List of all Panel Settings Windows.
        /// </summary>
        private readonly List<PanelSettingsUI> _settingsWindows = new();

        /// <summary>
        /// Toggles the Panel Settings Window for the given panel.
        /// </summary>
        /// <param name="panel">The panel to toggle the settings window for.</param>
        public void ToggleSettingsWindow(Panel panel)
        {
            PanelSettingsUI panelSettingsUI = _settingsWindows.Find(w => w.Panel == panel);
            if (panelSettingsUI)
            {
                panelSettingsUI.SetActive(!panelSettingsUI.gameObject.activeSelf);
            }
            else
            {
                SpawnNewWindow(panel);
            }
        }

        /// <summary>
        /// Spawns a new Panel Settings Window for the given panel.
        /// </summary>
        /// <param name="panel">The panel to spawn the settings window for.</param>
        private void SpawnNewWindow(Panel panel)
        {
            PanelSettingsUI newSettingsWindow = Instantiate(PanelSettingsWindowPrefab, panel.transform);

            // Find the Canvas of the panel
            Canvas entityCanvas = panel.transform.GetComponentInChildren<Canvas>();
            if (entityCanvas)
            {
                RectTransform entityCanvasRect = entityCanvas.GetComponent<RectTransform>();
                RectTransform entitySettingsWindowRect = newSettingsWindow.GetComponent<RectTransform>();

                if (entityCanvasRect != null && entitySettingsWindowRect != null)
                {
                    // Ensure the settings window is within the same Canvas hierarchy
                    newSettingsWindow.transform.SetParent(panel.transform, true);

                    // Calculate the new position: directly to the left of the canvas
                    float offset = entityCanvasRect.rect.width * entityCanvasRect.transform.localScale.x / 2 + WindowPositionOffset * entitySettingsWindowRect.transform.localScale.x;
                    entitySettingsWindowRect.anchoredPosition = new Vector2(offset, 0);
                }
            }
            else
            {
                Debug.Log("No Canvas found for panel: " + panel.name);
            }

            _settingsWindows.Add(newSettingsWindow);

            newSettingsWindow.SetPanel(panel);
        }

        /// <summary>
        /// Updates the Panel Settings Window for the specified panel. If the settings window exists for the panel, it reloads its UI; otherwise, it spawns a new settings window.
        /// </summary>
        /// <param name="panel">The panel whose settings window needs to be updated.</param>
        public void UpdatePanelSettingsWindow(Panel panel)
        {
            PanelSettingsUI panelSettingsUI = _settingsWindows.Find(settingsUI => settingsUI.Panel == panel);
            if (panelSettingsUI)
            {
                StartCoroutine(panelSettingsUI.DelayedReloadUI());
            }
            else
            {
                SpawnNewWindow(panel);
            }
        }

        /// <summary>
        /// Updates the moving state of the specified panel.
        /// </summary>
        /// <param name="panel">The panel whose moving state is being updated.</param>
        /// <param name="isMoving">Indicates whether the panel is currently moving.</param>
        public void PanelIsMoving(Panel panel, bool isMoving)
        {
            PanelSettingsUI panelSettingsUI = _settingsWindows.Find(settingsUI => settingsUI.Panel == panel);
            if (panelSettingsUI)
                panelSettingsUI.IsMoving(isMoving);
        }
    }
}