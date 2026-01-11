using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Debug-only input script for testing SharedInventorySystem.
    /// This script should be REMOVED before release.
    /// </summary>
    public class InventoryDebugInput : MonoBehaviour
    {
        private SharedInventorySystem cargo;

        private void Start()
        {
            cargo = Object.FindFirstObjectByType<SharedInventorySystem>();

            if (cargo == null)
            {
                Debug.LogError("[InventoryDebugInput] SharedInventorySystem NOT found in scene!");
            }
            else
            {
                Debug.Log("[InventoryDebugInput] SharedInventorySystem found.");
            }
        }

        private void Update()
        {
            if (cargo == null)
                return;

            // Add ScrapMetal
            if (Input.GetKeyDown(KeyCode.Q))
            {
                bool added = cargo.TryAddItem(InventoryItemType.ScrapMetal);
                Debug.Log($"[InventoryDebugInput] TryAdd ScrapMetal → {added}");
            }

            // Add AlienTech
            if (Input.GetKeyDown(KeyCode.W))
            {
                bool added = cargo.TryAddItem(InventoryItemType.AlienTech);
                Debug.Log($"[InventoryDebugInput] TryAdd AlienTech → {added}");
            }

            // Add PowerCore (critical part)
            if (Input.GetKeyDown(KeyCode.E))
            {
                bool added = cargo.TryAddItem(InventoryItemType.PowerCore);
                Debug.Log($"[InventoryDebugInput] TryAdd PowerCore → {added}");
            }

            // Remove ScrapMetal
            if (Input.GetKeyDown(KeyCode.R))
            {
                bool removed = cargo.RemoveItem(InventoryItemType.ScrapMetal);
                Debug.Log($"[InventoryDebugInput] Remove ScrapMetal → {removed}");
            }
        }
    }
}
