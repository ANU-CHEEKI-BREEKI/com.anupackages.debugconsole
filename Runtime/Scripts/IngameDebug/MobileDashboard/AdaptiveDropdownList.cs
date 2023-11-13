using System.Linq;
using TMPro;
using UnityEngine;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(RectTransform))]
    public class AdaptiveDropdownList : MonoBehaviour
    {
        private void OnEnable()
        {
            var text = GetComponentsInChildren<TMP_Text>();
            var max = text.Max(t => t.preferredWidth);
            var rt = transform as RectTransform;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, max);
        }
    }
}