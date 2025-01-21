using TMPro;
using UnityEngine;

using UnityEngine.XR.ARFoundation;

namespace PolySpatial.Samples
{
    [RequireComponent(typeof(ARPlane))]
    public class PlaneDataUI : MonoBehaviour
    {
        [SerializeField]
        TMP_Text m_AlignmentText;

        [SerializeField]
        TMP_Text m_ClassificationText;

        [SerializeField]
        TMP_Text m_IDText;

        ARPlane m_Plane;

        void OnEnable()
        {
            m_Plane = GetComponent<ARPlane>();
            m_Plane.boundaryChanged += OnBoundaryChanged;
        }

        void OnDisable()
        {
            m_Plane.boundaryChanged -= OnBoundaryChanged;
        }

        void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs eventArgs)
        {
            m_ClassificationText.text = m_Plane.classifications.ToString();
            m_AlignmentText.text = m_Plane.alignment.ToString();
            m_IDText.text = m_Plane.trackableId.ToString();

            transform.position = m_Plane.center;
        }
    }
}
