using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    public class TurnTable : MonoBehaviour
    {
        private new Transform transform;
        public Vector3 rotationSpeeds;

        private void Awake()
        {
            transform = GetComponent<Transform>();
        }

        private void Update()
        {
            transform.Rotate(rotationSpeeds * Time.deltaTime,Space.World);
        }
    }
}
