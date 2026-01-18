using UnityEngine;

namespace GameCore
{
    public class SimpleHoverEffect : MonoBehaviour
    {
        [SerializeField] private float hoverHeight = 0.25f;
        [SerializeField] private float hoverSpeed = 2f;

        private Vector3 basePosition;

        private void Awake()
        {
            basePosition = transform.position;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            Vector3 pos = basePosition;
            pos.y += offset;
            transform.position = pos;
        }
    }
}
