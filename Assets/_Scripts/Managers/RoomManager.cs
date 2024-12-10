#if QUEST_BUILD && FALSE
using Meta.XR.MRUtilityKit;
#endif
using UnityEngine;

namespace Managers
{
    public class RoomManager : MonoBehaviour
    {
#if QUEST_BUILD && FALSE
        [SerializeField] private MRUK MRUtilityKit; 
        [SerializeField] private GameObject HassUI; 
        
        public void OnOVRSceneLoaded()
        {
            foreach (MRUKRoom mrukRoom in MRUtilityKit.Rooms)
            {
                //     Debug.Log(mrukRoom.name);
                //     Debug.Log(mrukRoom.FloorAnchor.transform.position);
                //     Debug.Log(mrukRoom.FloorAnchor.transform.rotation);
                HassUI.transform.position = mrukRoom.FloorAnchor.transform.position;
                HassUI.transform.rotation = Quaternion.Euler(0, mrukRoom.FloorAnchor.transform.rotation.eulerAngles.y + 180, 0);
            }
        }
#endif

    }
}