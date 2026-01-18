using UnityEngine;

namespace GameCore
{
    public class SimplePulseEffect : MonoBehaviour
    {
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.1f;

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = baseScale * pulse;
        }
    }
}
