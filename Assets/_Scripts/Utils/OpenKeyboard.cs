using UnityEngine;

namespace Utils
{
    public class OpenKeyboard : MonoBehaviour
    {
        public void Open()
        {
            TouchScreenKeyboard.Open("Test");
            Debug.Log("Open Keyboard");
        }
    }
}