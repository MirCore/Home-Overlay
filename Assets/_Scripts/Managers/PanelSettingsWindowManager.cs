using System.Collections.Generic;
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
        private readonly List<PanelSettingsUI> _entitySettingsWindows = new();

        /// <summary>
        /// Toggles the Panel Settings Window for the given panel.
        /// </summary>
        /// <param name="panel">The panel to toggle the settings window for.</param>
        public void ToggleSettingsWindow(Panels.Panel panel)
        {
            PanelSettingsUI panelSettingsUI = _entitySettingsWindows.Find(w => w.Panel == panel);
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
        private void SpawnNewWindow(Panels.Panel panel)
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

            _entitySettingsWindows.Add(newSettingsWindow);

            newSettingsWindow.SetPanel(panel);
        }

        public void UpdatePanelSettingsWindow(Panels.Panel panel)
        {
            PanelSettingsUI panelSettingsUI = _entitySettingsWindows.Find(w => w.Panel == panel);
            if (panelSettingsUI)
            {
                panelSettingsUI.gameObject.SetActive(false);
                panelSettingsUI.gameObject.SetActive(true);
            }
            else
            {
                SpawnNewWindow(panel);
            }
        }
    }
}