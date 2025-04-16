using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Structs;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Utils;

namespace Managers
{
    /// <summary>
    /// Manages the panels, including loading, spawning, and removing panels.
    /// </summary>
    [RequireComponent(typeof(PanelSpawner)), RequireComponent(typeof(PanelSettingsWindowManager))]
    public class PanelManager : Singleton<PanelManager>
    {
        /// <summary>
        /// The list of panel data representing the panels in the scene.
        /// </summary>
        public List<PanelData> PanelDataList { get; private set; } = new ();

        /// <summary>
        /// A list of Panels from the apps saved state that are not yet spawned
        /// </summary>
        private List<PanelData> _panelsToLoad;

        private PanelSpawner _panelSpawner;

        private void OnEnable()
        {
            _panelSpawner = GetComponent<PanelSpawner>();
            AnchorHelper.ARAnchorManager.trackablesChanged.AddListener(OnAnchorsChanged);
            EventManager.OnAppStateLoaded += OnAppStateLoadedCallback;
        }

        private void OnDisable()
        {
            AnchorHelper.ARAnchorManager.trackablesChanged.RemoveListener(OnAnchorsChanged);
            EventManager.OnAppStateLoaded -= OnAppStateLoadedCallback;
        }
        
        /// <summary>
        /// Callback method for when the app state is loaded.
        /// </summary>
        private void OnAppStateLoadedCallback()
        {
            // Load entity objects when the app state is loaded
            LoadEntityObjects();
        }

        /// <summary>
        /// Handles changes to AR anchors, spawning panels for added anchors and removing unassociated anchors.
        /// </summary>
        /// <param name="changes">The event data containing changes to AR anchors.</param>
        private void OnAnchorsChanged(ARTrackablesChangedEventArgs<ARAnchor> changes)
        {
            if (PanelDataList.Count > 0)
            {
                // Spawn saved panels for added anchors
                foreach (PanelData panelData in changes.added.Select(arAnchor => _panelsToLoad.FirstOrDefault(a => a.AnchorID == arAnchor.trackableId.ToString())))
                {
                    if (panelData == null) continue;
                    _panelSpawner.SpawnSavedPanel(panelData);
                    _panelsToLoad.Remove(panelData);
                }
            }
            
            // Remove any anchors that are not associated with a panel
            foreach (ARAnchor addedAnchor in changes.added.Where(anchor => !PanelDataList.Exists(e => e.AnchorID == anchor.trackableId.ToString())))
            {
                StartCoroutine(DeleteAnchorNextFrame(addedAnchor));
            }
        }
        
        /// <summary>
        /// Deletes an anchor on the next frame after it is added, ensuring it is fully initialized before removal.
        /// </summary>
        /// <param name="anchor">The anchor to delete.</param>
        private static IEnumerator DeleteAnchorNextFrame(ARAnchor anchor)
        {
            yield return new WaitForEndOfFrame();
            AnchorHelper.ARAnchorManager.TryRemoveAnchor(anchor);
        }

        /// <summary>
        /// Loads entity objects from saved data and initializes the panels to load.
        /// </summary>
        private void LoadEntityObjects()
        {
            // Read the saved panel data from a file
            PanelDataList = SaveFile.ReadFile();
            if (PanelDataList == null)
                return;

            // Initialize the list of panels to load
            _panelsToLoad = PanelDataList.ToList();
            LoadPanels();
        }

        /// <summary>
        /// Loads panels that either have no anchor ID set or whose anchors already exist.
        /// </summary>
        private void LoadPanels()
        {
            if (_panelsToLoad.Count == 0)
                return;
            
            // Spawn panels that are ready to be loaded
            foreach (PanelData panelData in _panelsToLoad.Where(p => string.IsNullOrEmpty(p.AnchorID) || AnchorHelper.TryGetExistingAnchor(p.AnchorID)).ToList())
            {
                _panelSpawner.SpawnSavedPanel(panelData);
                _panelsToLoad.Remove(panelData);
            }
        }

        /// <summary>
        /// Adds new panel data to the list and saves the changes.
        /// </summary>
        /// <param name="panelData">The panel data to add.</param>
        public void AddNewPanelData(PanelData panelData)
        {
            PanelDataList.Add(panelData);
            SaveFile.SetPanelDataDirty();
        }

        /// <summary>
        /// Spawns a new entity at the current position of the PanelManager.
        /// </summary>
        /// <param name="selectedEntityID">The ID of the entity to spawn.</param>
        public void SpawnNewEntity(string selectedEntityID)
        {
            _panelSpawner.SpawnNewEntity(selectedEntityID, transform.position);
        }

        /// <summary>
        /// Deletes a panel by its ID.
        /// </summary>
        /// <param name="panelID">The ID of the panel to delete.</param>
        public void DeletePanel(string panelID)
        {
            PanelData panelData = PanelDataList.FirstOrDefault(p => p.ID == panelID);
            if (panelData == null)
                return;
            Destroy(panelData.Panel.gameObject);
            PanelDataList.Remove(panelData);
            SaveFile.SetPanelDataDirty();
        }

        /// <summary>
        /// Highlights a panel by its ID.
        /// </summary>
        /// <param name="panelID">The ID of the panel to highlight.</param>
        public void HighlightPanel(string panelID)
        {
            PanelData panelData = PanelDataList.FirstOrDefault(p => p.ID == panelID);
            panelData?.Panel.WindowHighlighter.HighlightWindow(panelData.Panel);
        }
    }
}