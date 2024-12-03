using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace Managers
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private MRUK MRUtilityKit; 
        [SerializeField] private OVRCameraRig OVRCameraRig; 
        [SerializeField] private GameObject HassUI; 
        
        public void OnOVRSceneLoaded()
        {
            foreach (MRUKRoom mrukRoom in MRUtilityKit.Rooms)
            {
                Debug.Log(mrukRoom.name);
                Debug.Log(mrukRoom.FloorAnchor.transform.position);
                Debug.Log(mrukRoom.FloorAnchor.transform.rotation);
                //MRUtilityKit.TrackingSpaceOffset = Matrix4x4.TRS(-mrukRoom.FloorAnchor.transform.position, mrukRoom.FloorAnchor.transform.rotation, Vector3.one);
                //OVRCameraRig.transform.position = mrukRoom.FloorAnchor.transform.position;
                //OVRCameraRig.transform.rotation = mrukRoom.FloorAnchor.transform.rotation;
                HassUI.transform.position = mrukRoom.FloorAnchor.transform.position;
                HassUI.transform.rotation = Quaternion.Euler(0, mrukRoom.FloorAnchor.transform.rotation.eulerAngles.y + 180, 0);
            }
        }
    }
}
