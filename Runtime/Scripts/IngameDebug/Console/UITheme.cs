using UnityEngine;

namespace ANU.IngameDebug.Console
{
    [System.Serializable]
    public struct UITheme
    {
        [field: SerializeField] public Color Log { get; private set; }
        [field: SerializeField] public Color Warnings { get; private set; }
        [field: SerializeField] public Color Errors { get; private set; }
        [field: SerializeField] public Color Exceptions { get; private set; }
        [field: SerializeField] public Color Assert { get; private set; }
    }
}