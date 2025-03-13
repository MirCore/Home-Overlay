using TMPro;
using UnityEngine;

namespace UI
{
    public class DropDown : TMP_Dropdown
    {
        protected override GameObject CreateBlocker(Canvas rootCanvas) {
            GameObject blocker = base.CreateBlocker(rootCanvas);
            blocker.layer = gameObject.layer;
            blocker.AddComponent<SortingLayerAttacher>();
            return blocker;
        }
    }
}
