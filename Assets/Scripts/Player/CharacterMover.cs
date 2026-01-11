using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Simple player movement controller.
    /// WASD movement on XZ plane with configurable speed.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMover : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Movement speed in units per second")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Rotation Settings")]
        [SerializeField] private bool rotateWithMovement = true;
        [SerializeField] private bool useCameraRelativeMovement = true;
        [SerializeField] private PlayerLookController lookController;

        [Header("Per-Player Modifiers")]
        [Tooltip("Additive move speed modifier from upgrades")]
        [SerializeField] private float moveSpeedModifier = 0f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private CharacterController characterController;
        private Vector3 moveDirection;
        private bool movementLocked = false;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (lookController == null)
                lookController = GetComponent<PlayerLookController>();
        }

        private void Update()
        {
            if (movementLocked)
            {
                characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
                return;
            }

            HandleMovement();
        }

        private void HandleMovement()
        {
            // Get input from WASD or arrow keys
            float horizontal = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
            float vertical = Input.GetAxisRaw("Vertical");     // W/S or Up/Down

            // Create movement direction on XZ plane (Y = 0 for horizontal movement)
            if (useCameraRelativeMovement && Camera.main != null)
            {
                Vector3 forward = Camera.main.transform.forward;
                Vector3 right = Camera.main.transform.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();
                moveDirection = (right * horizontal + forward * vertical).normalized;
            }
            else
            {
                moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
            }

            // Apply movement
            if (moveDirection.magnitude > 0.1f)
            {
                // Calculate effective speed with per-player modifier
                float effectiveSpeed = moveSpeed + moveSpeedModifier;

                // Move the character
                Vector3 move = moveDirection * effectiveSpeed * Time.deltaTime;
                characterController.Move(move);

                // Rotate to face movement direction (instant, no smoothing)
                if (rotateWithMovement && (lookController == null || !lookController.IsLookActive))
                {
                    transform.rotation = Quaternion.LookRotation(moveDirection);
                }
            }

            // Apply gravity (keep player grounded)
            characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        /// <summary>
        /// Add to move speed modifier (for upgrades).
        /// </summary>
        public void AddMoveSpeedModifier(float amount)
        {
            moveSpeedModifier += amount;
        }

        /// <summary>
        /// Reset move speed modifier (for new run).
        /// </summary>
        public void ResetModifier()
        {
            moveSpeedModifier = 0f;
        }

        /// <summary>
        /// Get current movement direction (for other systems to query).
        /// </summary>
        public Vector3 GetMovementDirection()
        {
            return moveDirection;
        }

        /// <summary>
        /// Get current movement speed (for other systems to query).
        /// </summary>
        public float GetCurrentSpeed()
        {
            return moveSpeed;
        }

        /// <summary>
        /// Modify movement speed (for cargo penalties, buffs, etc.)
        /// </summary>
        public void SetMoveSpeed(float newSpeed)
        {
            moveSpeed = Mathf.Max(0, newSpeed); // Prevent negative speed
        }

        public void SetMovementLocked(bool locked)
        {
            movementLocked = locked;
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUI.Box(new Rect(10, 550, 300, 80), "Player Controller");
            GUI.Label(new Rect(20, 575, 280, 20), $"Position: {transform.position}");
            GUI.Label(new Rect(20, 595, 280, 20), $"Move Speed: {moveSpeed:F1}");
            GUI.Label(new Rect(20, 615, 280, 20), $"Moving: {(moveDirection.magnitude > 0.1f ? "YES" : "NO")}");
        }
    }
}
