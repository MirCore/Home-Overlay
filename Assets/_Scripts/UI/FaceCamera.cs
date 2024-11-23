using UnityEditor;
using UnityEngine;

namespace UI
{
    [ExecuteInEditMode]
    public class FaceCamera : MonoBehaviour
    {
        [SerializeField] private bool ExecuteInEditMode = true;
        private Camera _camera;
        private Transform _cameraTransform;
        
        private void Awake()
        {
            // Find and store a reference to the main camera's transform
            if (!Camera.main)
                return;
            _cameraTransform = Camera.main.transform;
            _camera = Camera.main;
        }

        private void Update()
        {
            // Use SceneView camera when in edit mode, otherwise use mainCamera
#if UNITY_EDITOR
            if (!ExecuteInEditMode)
                return;
            if (!Application.isPlaying && SceneView.lastActiveSceneView)
            {
                _camera = SceneView.lastActiveSceneView.camera;
                _cameraTransform = _camera.transform;
            }
#endif
            if (!_camera)
                return;
        
            Vector3 cameraPosition = _cameraTransform.position;
            Vector3 position = transform.position;
            
            transform.rotation = Quaternion.LookRotation(position - cameraPosition, Vector3.up);
        }
    }
}
