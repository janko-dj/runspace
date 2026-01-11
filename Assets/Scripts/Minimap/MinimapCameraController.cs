using UnityEngine;

namespace GameCore
{
    [RequireComponent(typeof(Camera))]
    public class MinimapCameraController : MonoBehaviour
    {
        [SerializeField] private float height = 50f;
        [SerializeField] private float orthographicSize = 60f;
        [SerializeField] private Vector3 mapCenter = Vector3.zero;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private string minimapLayerName = "Minimap";

        private void Awake()
        {
            ConfigureCamera();
        }

        private void ConfigureCamera()
        {
            Camera cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 1 << LayerMask.NameToLayer(minimapLayerName);
            cam.targetTexture = renderTexture;
            cam.depth = -10f;

            transform.position = new Vector3(mapCenter.x, height, mapCenter.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        public void SetRenderTexture(RenderTexture texture)
        {
            renderTexture = texture;
            Camera cam = GetComponent<Camera>();
            cam.targetTexture = renderTexture;
        }
    }
}
