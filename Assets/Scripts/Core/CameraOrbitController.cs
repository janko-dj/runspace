using UnityEngine;

namespace GameCore
{
    public class CameraOrbitController : MonoBehaviour
    {
        [SerializeField] private FollowCamera followCamera;
        [SerializeField] private float yawSpeed = 180f;
        [SerializeField] private bool requireRightMouse = false;
        [SerializeField] private bool lockCursor = true;

        private float currentYaw;
        private Vector3 baseOffset;

        private void Awake()
        {
            if (followCamera == null)
            {
                followCamera = GetComponent<FollowCamera>();
            }
        }

        private void Start()
        {
            if (followCamera == null)
            {
                return;
            }

            Vector3 offset = followCamera.Offset;
            baseOffset = offset;

            Vector3 flat = new Vector3(offset.x, 0f, offset.z);
            if (flat.sqrMagnitude < 0.001f)
            {
                flat = Vector3.back;
            }

            currentYaw = Quaternion.LookRotation(flat).eulerAngles.y;
        }

        private void OnEnable()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void LateUpdate()
        {
            if (followCamera == null || followCamera.Target == null)
            {
                return;
            }

            if (PlayerSettingsService.Instance != null && !PlayerSettingsService.Instance.CameraOrbitEnabled)
            {
                return;
            }

            if (requireRightMouse && !Input.GetMouseButton(1))
            {
                return;
            }

            float mouseX = Input.GetAxis("Mouse X");
            if (Mathf.Abs(mouseX) < 0.001f)
            {
                return;
            }

            float sensitivity = PlayerSettingsService.Instance != null ? PlayerSettingsService.Instance.MouseSensitivity : 1f;
            float invert = PlayerSettingsService.Instance != null && PlayerSettingsService.Instance.InvertYaw ? -1f : 1f;
            currentYaw += mouseX * yawSpeed * sensitivity * invert * Time.deltaTime;

            Vector3 flat = new Vector3(baseOffset.x, 0f, baseOffset.z);
            float distance = flat.magnitude;
            Vector3 rotatedFlat = Quaternion.Euler(0f, currentYaw, 0f) * new Vector3(0f, 0f, distance);
            Vector3 newOffset = new Vector3(rotatedFlat.x, baseOffset.y, rotatedFlat.z);

            followCamera.SetOffset(newOffset);
        }
    }
}
