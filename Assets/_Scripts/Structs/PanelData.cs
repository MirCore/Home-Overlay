using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
// ReSharper disable InconsistentNaming

namespace Structs
{
    /// <summary>
    /// Represents the data for a panel in the environment, including its settings and transform data.
    /// </summary>
    [Serializable]
    public class PanelData
    {
        /// <summary>
        /// The unique ID of the panel.
        /// </summary>
        [SerializeField] private string _id;

        /// <summary>
        /// The entity ID associated with the panel.
        /// </summary>
        [SerializeField] private string _entityID;

        /// <summary>
        /// The anchor ID associated with the panel.
        /// </summary>
        [SerializeField] private string _anchorID;

        /// <summary>
        /// The position of the panel in the environment.
        /// </summary>
        [SerializeField] private Vector3 _position;

        /// <summary>
        /// The rotation of the panel in the environment.
        /// </summary>
        [SerializeField] private Quaternion _rotation;

        /// <summary>
        /// The scale of the panel in the environment.
        /// </summary>
        [SerializeField] private Vector3 _scale = Vector3.one;

        /// <summary>
        /// The settings for the panel.
        /// </summary>
        [SerializeField] private PanelSettings _settings = new();
        
        /// <summary>
        /// If the Panel is a demo panel
        /// </summary>
        [SerializeField] private bool _isDemoPanel = false;

        /// <summary>
        /// The panel object associated with this data.
        /// </summary>
        private Panels.Panel _panel;

        /// <summary>
        /// Initializes a new instance of the PanelData class with the specified ID, entity ID, and position.
        /// </summary>
        /// <param name="id">The unique ID of the panel.</param>
        /// <param name="entityID">The entity ID associated with the panel.</param>
        /// <param name="transformPosition">The position of the panel in the environment.</param>
        /// <param name="isDemo">Whether the panel is a demo panel</param>
        public PanelData(string id, string entityID, Vector3 transformPosition, bool isDemo = false)
        {
            _id = id;
            EntityID = entityID;
            Position = transformPosition;
            IsDemoPanel = isDemo;
        }

        /// <summary>
        /// Gets or sets the unique ID of the panel.
        /// </summary>
        public string ID
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        /// <summary>
        /// Gets or sets the entity ID associated with the panel.
        /// </summary>
        public string EntityID
        {
            get => _entityID;
            set => SetField(ref _entityID, value);
        }

        /// <summary>
        /// Gets or sets the anchor ID associated with the panel.
        /// </summary>
        public string AnchorID
        {
            get => _anchorID;
            set => SetField(ref _anchorID, value);
        }

        /// <summary>
        /// Gets or sets the position of the panel in the environment.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set => SetField(ref _position, value);
        }

        /// <summary>
        /// Gets or sets the rotation of the panel in the environment.
        /// </summary>
        public Quaternion Rotation
        {
            get => _rotation;
            set => SetField(ref _rotation, value);
        }

        /// <summary>
        /// Gets or sets the scale of the panel in the environment.
        /// </summary>
        public Vector3 Scale
        {
            get => _scale == Vector3.zero ? Vector3.one : _scale;
            set => SetField(ref _scale, value);
        }

        /// <summary>
        /// Indicates whether the panel is a demo panel.
        /// </summary>
        public bool IsDemoPanel
        {
            get => _isDemoPanel;
            set => SetField(ref _isDemoPanel, value);
        }

        /// <summary>
        /// Gets or sets the settings for the panel.
        /// </summary>
        public PanelSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        /// <summary>
        /// Gets or sets the panel object associated with this data.
        /// </summary>
        public Panels.Panel Panel
        {
            get => _panel;
            set => _panel = value;
        }

        /// <summary>
        /// Sets a field value and saves the panel data if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">The field to set.</param>
        /// <param name="value">The value to set the field to.</param>
        /// <returns>The value that was set.</returns>
        private static T SetField<T>(ref T field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
                SavePanelData();

            field = value;
            return value;
        }

        /// <summary>
        /// Marks the panel data as dirty, indicating that it needs to be saved.
        /// </summary>
        private static void SavePanelData()
        {
            SaveFile.SetPanelDataDirty();
        }

        /// <summary>
        /// Represents the settings for a panel.
        /// </summary>
        [Serializable]
        public class PanelSettings
        {
            /// <summary>
            /// Whether to show the name of the panel.
            /// </summary>
            public bool _showName = true;

            /// <summary>
            /// Whether to show the state of the panel.
            /// </summary>
            public bool _showState = true;

            /// <summary>
            /// Whether to hide the window controls of the panel.
            /// </summary>
            public bool _hideWindowControls;

            /// <summary>
            /// Whether to align the panel to the wall.
            /// </summary>
            public bool _alignPanelToWall;

            /// <summary>
            /// Whether rotation is enabled for the panel.
            /// </summary>
            public bool _rotationEnabled = true;

            /// <summary>
            /// Gets or sets whether to show the name of the panel.
            /// </summary>
            public bool ShowName
            {
                get => _showName;
                set => SetField(ref _showName, value);
            }

            /// <summary>
            /// Gets or sets whether to show the state of the panel.
            /// </summary>
            public bool ShowState
            {
                get => _showState;
                set => SetField(ref _showState, value);
            }

            /// <summary>
            /// Gets or sets whether to hide the window controls of the panel.
            /// </summary>
            public bool HideWindowControls
            {
                get => _hideWindowControls;
                set => SetField(ref _hideWindowControls, value);
            }

            /// <summary>
            /// Gets or sets whether to align the panel to the wall.
            /// </summary>
            public bool AlignWindowToWall
            {
                get => _alignPanelToWall;
                set => SetField(ref _alignPanelToWall, value);
            }

            /// <summary>
            /// Gets or sets whether rotation is enabled for the panel.
            /// </summary>
            public bool RotationEnabled
            {
                get => _rotationEnabled;
                set => SetField(ref _rotationEnabled, value);
            }
        }
    }

    /// <summary>
    /// A wrapper class for a list of PanelData objects, used for serialization.
    /// </summary>
    [Serializable]
    public class PanelDataListWrapper
    {
        /// <summary>
        /// The list of PanelData objects.
        /// </summary>
        public List<PanelData> PanelDatas;
    }
}
