using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    /// <summary>
    /// Choose one of the values, which always visible
    /// </summary>
    internal class Switch : MonoBehaviour
    {
        [SerializeField] private Toggle[] _toggles;
        [SerializeField] private ToggleGroup _group;

        public void Initialize()
        {
        }
    }
}