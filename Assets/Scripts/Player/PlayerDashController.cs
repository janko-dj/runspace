using UnityEngine;

namespace GameCore
{
    public class PlayerDashController : MonoBehaviour
    {
        [SerializeField] private float dashSpeed = 18f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 2.5f;
        [SerializeField] private KeyCode dashKey = KeyCode.Space;

        private CharacterController characterController;
        private PlayerLookController lookController;
        private CharacterMover mover;

        private float dashTimer = 0f;
        private float cooldownTimer = 0f;
        private bool isDashing = false;
        private Vector3 dashDirection = Vector3.forward;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            lookController = GetComponent<PlayerLookController>();
            mover = GetComponent<CharacterMover>();
        }

        private void Update()
        {
            UpdateCooldown(Time.deltaTime);
            HandleDashInput();
        }

        private void HandleDashInput()
        {
            if (isDashing || cooldownTimer > 0f)
            {
                return;
            }

            if (Input.GetKeyDown(dashKey))
            {
                StartDash();
            }
        }

        private void StartDash()
        {
            dashDirection = lookController != null
                ? lookController.LookDirection
                : transform.forward;
            dashDirection.y = 0f;
            if (dashDirection.sqrMagnitude < 0.01f)
            {
                dashDirection = transform.forward;
            }
            dashDirection.Normalize();

            isDashing = true;
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;

            if (mover != null)
            {
                mover.SetMovementLocked(true);
            }

            Debug.Log("[Dash] Triggered");
        }

        private void FixedUpdate()
        {
            if (!isDashing)
            {
                return;
            }

            float step = dashSpeed * Time.fixedDeltaTime;
            characterController.Move(dashDirection * step);

            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                if (mover != null)
                {
                    mover.SetMovementLocked(false);
                }
            }
        }

        private void UpdateCooldown(float deltaTime)
        {
            if (cooldownTimer <= 0f)
            {
                return;
            }

            cooldownTimer -= deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                Debug.Log("[Dash] Cooldown ready");
            }
        }
    }
}
