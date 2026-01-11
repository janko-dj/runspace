using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Represents a single ship problem that needs repair.
    /// Tracks whether the problem has been resolved.
    /// </summary>
    [System.Serializable]
    public class SystemIssue
    {
        public SystemIssueType problemType;
        public bool isResolved;

        public SystemIssue(SystemIssueType type)
        {
            problemType = type;
            isResolved = false;
        }

        /// <summary>
        /// Mark this problem as resolved.
        /// </summary>
        public void Resolve()
        {
            if (isResolved)
            {
                Debug.LogWarning($"[SystemIssue] {problemType} already resolved!");
                return;
            }

            isResolved = true;
            Debug.Log($"[SystemIssue] {problemType} has been RESOLVED!");
        }

        /// <summary>
        /// Reset problem to unresolved state (for new runs).
        /// </summary>
        public void Reset()
        {
            isResolved = false;
        }

        public override string ToString()
        {
            string status = isResolved ? "✓ RESOLVED" : "⚠ NEEDS REPAIR";
            return $"{problemType}: {status}";
        }
    }
}
