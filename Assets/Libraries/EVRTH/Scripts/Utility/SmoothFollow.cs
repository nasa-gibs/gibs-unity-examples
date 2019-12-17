using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    public class SmoothFollow : MonoBehaviour
    {
        public Transform targetTransform;
        public float followSpeed;
        private new Transform transform;

        private void Awake()
        {
            transform = GetComponent<Transform>();
        }

        private void FixedUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position,
                followSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation,
                followSpeed * Time.fixedDeltaTime);
        }
    }
}
