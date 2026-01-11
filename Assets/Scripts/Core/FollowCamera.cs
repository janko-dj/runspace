using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Simple camera follow script that tracks the player with a fixed offset.
    /// Useful for third-person and top-down views.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [Header("Follow Settings")]
        [Tooltip("Target to follow (usually the player)")]
        [SerializeField] private Transform target;

        [Tooltip("Offset from target position")]
        [SerializeField] private Vector3 offset = new Vector3(0, 8, -8);

        [Tooltip("Smooth follow speed (0 = instant, higher = smoother)")]
        [SerializeField] private float smoothSpeed = 5f;

        [Tooltip("Camera looks at target if enabled")]
        [SerializeField] private bool lookAtTarget = true;

        [Header("Debug")]
        [SerializeField] private bool autoFindPlayer = true;

        private void Start()
        {
            // Auto-find player if not assigned
            if (target == null && autoFindPlayer)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("[FollowCamera] Auto-found player target");
                }
                else
                {
                    Debug.LogWarning("[FollowCamera] No target assigned and no Player tag found!");
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            // Calculate desired position
            Vector3 desiredPosition = target.position + offset;

            // Smooth movement
            if (smoothSpeed > 0)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = desiredPosition;
            }

            // Look at target
            if (lookAtTarget)
            {
                transform.LookAt(target);
            }
        }

        /// <summary>
        /// Set the follow target at runtime.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Set camera offset at runtime.
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        public Transform Target => target;
        public Vector3 Offset => offset;
    }
}
