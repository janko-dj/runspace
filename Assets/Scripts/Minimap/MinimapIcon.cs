using UnityEngine;

namespace GameCore
{
    public class MinimapIcon : MonoBehaviour
    {
        [SerializeField] private Color iconColor = Color.white;
        [SerializeField] private float iconSize = 1f;
        [SerializeField] private float iconHeight = 0.5f;
        [SerializeField] private string minimapLayerName = "Minimap";

        private GameObject iconObject;

        private void Awake()
        {
            CreateIcon();
        }

        private void LateUpdate()
        {
            if (iconObject == null)
            {
                return;
            }

            iconObject.transform.localPosition = new Vector3(0f, iconHeight, 0f);
            iconObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            iconObject.transform.localScale = new Vector3(iconSize, iconSize, iconSize);
        }

        private void CreateIcon()
        {
            if (iconObject != null)
            {
                return;
            }

            iconObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            iconObject.name = "MinimapIcon";
            iconObject.transform.SetParent(transform, false);
            iconObject.transform.localPosition = new Vector3(0f, iconHeight, 0f);
            iconObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            iconObject.transform.localScale = new Vector3(iconSize, iconSize, iconSize);
            iconObject.layer = LayerMask.NameToLayer(minimapLayerName);

            Collider col = iconObject.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }

            Renderer renderer = iconObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Color"));
                mat.color = iconColor;
                renderer.sharedMaterial = mat;
            }
        }

        public void SetIcon(Color color, float size)
        {
            iconColor = color;
            iconSize = size;
            if (iconObject != null)
            {
                Renderer renderer = iconObject.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial.color = iconColor;
                }
            }
        }
    }
}
