using UnityEngine;
using UnityEngine.UI;

namespace JetXR.VisionUI
{
    public class ToggleAnimation : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Animator animator;

        private readonly int isOnID = Animator.StringToHash("IsOn");

        private void OnEnable()
        {
            CheckReferences();
            OnToggleValueChanged(toggle.isOn);

            if (toggle != null)
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void Reset()
        {
            CheckReferences();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void CheckReferences()
        {
            if (!toggle)
                toggle = GetComponent<Toggle>();

            if (!animator)
                animator = GetComponent<Animator>();
        }

        private void OnToggleValueChanged(bool newValue)
        {
            if (!animator)
                return;

            animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);  // Reset the current animation state
            animator.SetBool(isOnID, newValue);
        }

        public void Changed()
        {
            if (!toggle || !animator)
                CheckReferences();
            OnToggleValueChanged(toggle.isOn);
        }
    }
}