using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Structs;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Utils;

namespace Managers
{
    [RequireComponent(typeof(PanelSpawner)), RequireComponent(typeof(PanelSettingsWindowManager))]
    public class PanelManager : Singleton<PanelManager>
    {
        public List<PanelData> PanelDataList { get; private set; } = new ();

        [field: SerializeField] private List<PanelData> PanelsToLoad { get; set; }

        /// <summary>
        /// The PanelSpawner that spawns new entities.
        /// </summary>
        private PanelSpawner _panelSpawner;

        private void OnEnable()
        {
            _panelSpawner = GetComponent<PanelSpawner>();
            AnchorHelper.ARAnchorManager.trackablesChanged.AddListener(OnAnchorsChanged);
        }

        private void OnDisable()
        {
            AnchorHelper.ARAnchorManager.trackablesChanged.RemoveListener(OnAnchorsChanged);
        }

        private void OnAnchorsChanged(ARTrackablesChangedEventArgs<ARAnchor> changes)
        {
            if (PanelDataList.Count > 0)
            {
                foreach (PanelData panelData in changes.added.Select(arAnchor => PanelsToLoad.FirstOrDefault(a => a.AnchorID == arAnchor.trackableId.ToString())))
                {
                    if (panelData == null) continue;
                    _panelSpawner.SpawnSavedPanel(panelData);
                    PanelsToLoad.Remove(panelData);
                }
            }
            
            // Listens for when AR anchors are added to the scene, and deletes any anchors that aren't associated with an PanelData.
            foreach (ARAnchor addedAnchor in changes.added.Where(addedAnchor => !PanelDataList.Exists(e => e.AnchorID == addedAnchor.trackableId.ToString())))
            {
                StartCoroutine(DeleteAnchorNextFrame(addedAnchor));
            }
        }

        /// <summary>
        /// Removes a deleted Panel from the List and saves the changes
        /// </summary>
        /// <param name="panelData"></param>
        public void RemovePanel(PanelData panelData)
        {
            PanelDataList.Remove(panelData);
            SaveFile.SavePanelDatas();
        }

        /// <summary>
        /// Deletes an anchor on the next frame after it is added, after the anchor has been fully initialized.
        /// This is necessary because the anchor isn't fully initialized until the next frame after it is added,
        /// and attempting to delete it immediately will fail.
        /// </summary>
        /// <param name="anchor">The anchor to delete.</param>
        private static IEnumerator DeleteAnchorNextFrame(ARAnchor anchor)
        {
            yield return new WaitForEndOfFrame();
            AnchorHelper.ARAnchorManager.TryRemoveAnchor(anchor);
        }
        
        internal void LoadEntityObjects()
        {
            PanelDataList = SaveFile.ReadFile();
            if (PanelDataList == null)
                return;

            PanelsToLoad = PanelDataList.ToList();
            LoadPanels();
        }

        /// <summary>
        /// Checks if the anchors for PanelsToLoad already exists, or if there is no anchorID set in the panelData.
        /// Spawns the Panels if true and removes them from the list.
        /// </summary>
        private void LoadPanels()
        {
            if (PanelsToLoad.Count == 0)
                return;

            foreach (PanelData panelData in PanelsToLoad.Where(p => AnchorHelper.TryGetExistingAnchor(p.AnchorID, out ARAnchor _) || string.IsNullOrEmpty(p.AnchorID)).ToList())
            {
                _panelSpawner.SpawnSavedPanel(panelData);
                PanelsToLoad.Remove(panelData);
            }
        }

        public void AddNewPanelData(PanelData panelData)
        {
            PanelDataList.Add(panelData);
            SaveFile.SavePanelDatas();
        }

        public void SpawnNewEmptyEntity(Vector3 transformPosition)
        {
            _panelSpawner.SpawnNewEntity(null, transformPosition);
        }

        public void SpawnNewEntity(string selectedEntityID, Vector3 transformPosition)
        {
            _panelSpawner.SpawnNewEntity(selectedEntityID, transformPosition);
        }

        public void SpawnNewEntity(string selectedEntityID)
        {
            _panelSpawner.SpawnNewEntity(selectedEntityID, transform.position);
        }

        public void DeletePanel(string panelID)
        {
            PanelDataList.FirstOrDefault(p => p.ID == panelID)?.DeletePanel();
        }
    }
}