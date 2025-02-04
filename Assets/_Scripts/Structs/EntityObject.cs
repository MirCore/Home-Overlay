using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Utils;

namespace Structs
{
    [Serializable]
    public class EntityObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _entityID;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _rotation;
        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private string _anchorID;
        [SerializeField] private EntitySettings _settings;
        private Entity.Entity _entity;

        public EntityObject(string id, string entityID, Vector3 transformPosition)
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
                SaveFile.SaveEntityObjects();
            }
        }

        public Entity.Entity Entity { 
            get => _entity;
            set => _entity = value;
        }
    
        public Vector3 Position {
            get => _position;
            set
            {
                if (_position == value)
                    return;
            
                _position = value;
                SaveFile.SaveEntityObjects();
            }
        }
    
        public Quaternion Rotation {
            get => _rotation;
            set
            {
                if (_rotation == value)
                    return;
            
                _rotation = value;
                SaveFile.SaveEntityObjects();
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
                SaveFile.SaveEntityObjects();
            }
        }

        public string AnchorID {
            get => _anchorID;
            set
            {
            _anchorID = value;
            SaveFile.SaveEntityObjects();
            }
        }

        public void DeleteEntity()
        {
            if (_entity != null)
            {
                _entity.DeleteEntity();
                _entity = null;
            }
            else
                GameManager.Instance.RemoveEntity(this);
        }

        public EntitySettings Settings
        {
            get => _settings;
            set => _settings = value;
        }
        
        [Serializable]
        public class EntitySettings
        {
            public bool HideWindowControls { get; set; }
            public bool AlignWindowToWall { get; set; }
        }
    }


    [Serializable]
    public class EntityObjectListWrapper
    {
        public List<EntityObject> EntityObjects;
    }
}