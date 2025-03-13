using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Utils;

namespace Structs
{
    [Serializable]
    public class PanelData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _entityID;
        [SerializeField] private string _anchorID;
        
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _rotation;
        [SerializeField] private Vector3 _scale = Vector3.one;
        
        [SerializeField] private PanelSettings _settings = new ();
        
        // current instance of the Panel
        private Panels.Panel _panel;

        public PanelData(string id, string entityID, Vector3 transformPosition)
        {
            _id = id;
            EntityID = entityID;
            Position = transformPosition;
        }

        public string ID { 
            get => _id;
            set => _id = value;
        }

        public string EntityID { 
            get => _entityID;
            set
            {
                if (_entityID == value)
                    return;
            
                _entityID = value;
                SaveFile.SavePanelDatas();
            }
        }

        public Panels.Panel Panel { 
            get => _panel;
            set => _panel = value;
        }
    
        public Vector3 Position {
            get => _position;
            set
            {
                if (_position == value)
                    return;
            
                _position = value;
                SaveFile.SavePanelDatas();
            }
        }
    
        public Quaternion Rotation {
            get => _rotation;
            set
            {
                if (_rotation == value)
                    return;
            
                _rotation = value;
                SaveFile.SavePanelDatas();
            }
        }
    
        public Vector3 Scale
        {
            get
            {
                if (_scale == Vector3.zero) 
                    Scale = Vector3.one; 
                
                return _scale;
            }
            set
            {
                if (_scale == value)
                    return;
            
                _scale = value;
                SaveFile.SavePanelDatas();
            }
        }

        public string AnchorID {
            get => _anchorID;
            set
            {
            _anchorID = value;
            SaveFile.SavePanelDatas();
            }
        }

        public void DeletePanel()
        {
            if (_panel != null)
            {
                _panel.DeletePanel();
                _panel = null;
            }
            else
                PanelManager.Instance.RemovePanel(this);
        }

        public PanelSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }
        
        [Serializable]
        public class PanelSettings
        {
            public bool HideWindowControls;
            public bool AlignWindowToWall;
            public bool RotationEnabled = true;
        }
    }


    [Serializable]
    public class PanelDataListWrapper
    {
        public List<PanelData> PanelDatas;
    }
}