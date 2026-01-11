using UnityEngine;

namespace GameCore
{
    public class PlayerLookController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private bool requireRightMouse = false;
        [SerializeField] private bool instantRotationWhenActive = true;
        [SerializeField] private float groundHeight = 0f;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private bool ensureCursorVisible = true;
        [SerializeField] private bool debugRay = false;
        [SerializeField] private bool freezeY = true;

        [SerializeField] private bool isLookActive;
        public bool IsLookActive => isLookActive;
        public Vector3 LookDirection { get; private set; } = Vector3.forward;

        private void Update()
        {
            RotateTowardMouse();
        }

        private void OnEnable()
        {
            if (ensureCursorVisible && !requireRightMouse)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void RotateTowardMouse()
        {
            isLookActive = !requireRightMouse || Input.GetMouseButton(1);
            if (!isLookActive)
            {
                return;
            }

            if (aimCamera == null)
            {
                aimCamera = FindAimCamera();
            }

            if (aimCamera == null)
            {
                return;
            }

            Vector3 screenPoint = Cursor.lockState == CursorLockMode.Locked
                ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
                : Input.mousePosition;

            Ray ray = aimCamera.ScreenPointToRay(screenPoint);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));

            if (!groundPlane.Raycast(ray, out float enter))
            {
                if (debugRay)
                {
                    Debug.Log("[Look] Ray did not hit ground plane");
                }
                return;
            }

            Vector3 hitPoint = ray.GetPoint(enter);
            if (freezeY)
            {
                hitPoint.y = transform.position.y;
            }
            if (debugRay)
            {
                Debug.DrawLine(ray.origin, hitPoint, Color.cyan);
                Debug.Log($"[Look] hit={hitPoint} cam={aimCamera.name} mouse={Input.mousePosition}"); 
            }
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            if (instantRotationWhenActive)
            {
                transform.rotation = targetRotation;
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            LookDirection = transform.forward;
        }

        private Camera FindAimCamera()
        {
            FollowCamera followCamera = Object.FindFirstObjectByType<FollowCamera>();
            if (followCamera != null)
            {
                Camera cam = followCamera.GetComponent<Camera>();
                if (cam != null)
                {
                    return cam;
                }
            }

            return Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        }
    }
}
