namespace NRKernal.NRExamples
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NRHandMeshVisual : MonoBehaviour
    {
        [SerializeField]
        private NRHandMeshJointConfig m_HandPrefab;
        private NRHandMeshJointConfig m_HandMeshJoint;

        [SerializeField]
        // Use distance between these joints to calculate the length of hand
        private readonly List<HandJointID> m_HandLengthJoints = new List<HandJointID>() {
            HandJointID.MiddleTip, HandJointID.MiddleDistal, HandJointID.MiddleMiddle,
            HandJointID.MiddleProximal, HandJointID.Wrist
        };
        private float m_HandLength;

        private void Start()
        {
            CreateMeshVisuals();
        }

        private void CreateMeshVisuals()
        {
            if (m_HandPrefab != null)
            {
                m_HandMeshJoint = Instantiate(m_HandPrefab, transform);
                InitHandLength();
            }
        }

        private void InitHandLength()
        {
            m_HandLength = 0;
            for (int i = 0; i < m_HandLengthJoints.Count - 1; i++)
            {
                m_HandLength += Vector3.Distance(m_HandMeshJoint.HandJoint[(int)m_HandLengthJoints[i]].position, 
                    m_HandMeshJoint.HandJoint[(int)m_HandLengthJoints[i + 1]].position);
            }
        }

        private void OnEnable()
        {
            NRInput.Hands.OnHandStatesUpdated += OnHandTracking;
            NRInput.Hands.OnHandTrackingStopped += OnHandTrackingStopped;
        }

        private void OnDisable()
        {
            NRInput.Hands.OnHandStatesUpdated -= OnHandTracking;
            NRInput.Hands.OnHandTrackingStopped -= OnHandTrackingStopped;
        }

        private void OnHandTracking()
        {
            if (m_HandMeshJoint != null)
            {
                var handState = NRInput.Hands.GetHandState(m_HandMeshJoint.HandEnum);
                m_HandMeshJoint.gameObject.SetActive(handState.isTracked);
                if (handState.isTracked)
                {
                    UpdateWristByMiddle(handState);
                }
            }
        }

        private void UpdateWristByMiddle(HandState handState)
        {
            Transform wristTransform = m_HandMeshJoint.HandJoint[(int)HandJointID.Wrist];
            var wristPose = handState.GetJointPose(HandJointID.Wrist);
            wristPose.rotation *= Quaternion.Euler(m_HandMeshJoint.RotationOffset);
            wristTransform.rotation = wristPose.rotation;

            var middlePose = handState.GetJointPose(HandJointID.MiddleProximal);
            Transform middleTransform = m_HandMeshJoint.HandJoint[(int)HandJointID.MiddleProximal];
            Vector4 middleLocalPos = middleTransform.localPosition;
            middleLocalPos.w = 1;
            Vector3 tmp = Matrix4x4.Rotate(wristTransform.rotation) * Matrix4x4.Scale(wristTransform.localScale) * middleLocalPos;
            wristTransform.position = middlePose.position - tmp;
        }

/*        private void UpdateFingerJoint(HandState handState)
        {
            for (int i = (int)HandJointID.ThumbMetacarpal; i <= (int)HandJointID.PinkyTip; i++)
            {
                var t = m_HandMeshJoint.HandJoint[i];
                if (t != null)
                {
                    var pose = handState.GetJointPose((HandJointID)i);
                    t.rotation = pose.rotation * Quaternion.Euler(m_HandMeshJoint.RotationOffset);
                }
            }
        }*/

        private void OnHandTrackingStopped()
        {
            if (m_HandMeshJoint != null)
            {
                m_HandMeshJoint.gameObject.SetActive(false);
            }
        }
    }
}
