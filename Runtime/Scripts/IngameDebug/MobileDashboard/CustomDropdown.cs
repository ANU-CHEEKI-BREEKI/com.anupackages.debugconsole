using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class CustomDropdown : TMP_Dropdown
    {
        [Space]
        [SerializeField] private Color _blockerColor = Color.clear;

        protected override GameObject CreateBlocker(Canvas rootCanvas)
        {
            var blocker = base.CreateBlocker(rootCanvas);
            blocker.GetComponent<Image>().color = _blockerColor;
            return blocker;
        }
    }
}