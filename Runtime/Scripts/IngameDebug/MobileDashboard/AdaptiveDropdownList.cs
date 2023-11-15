using System.Linq;
using TMPro;
using UnityEngine;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(RectTransform))]
    public class AdaptiveDropdownList : MonoBehaviour
    {
        private bool _lateUpdate;

        private void OnEnable() => _lateUpdate = true;

        private void LateUpdate()
        {
            if (!_lateUpdate)
                return;
            _lateUpdate = false;

            var text = GetComponentsInChildren<TMP_Text>();
            var max = text.Max(t => t.preferredWidth);
            var rt = transform as RectTransform;

            // get root rect
            var canvas = GetComponentInParent<Canvas>();
            var rootRect = canvas.rootCanvas.transform as RectTransform;

            // prepare for clamp position and size
            var parent = transform.parent;
            rt.SetParent(rootRect);
            rt.anchorMin = Vector2.one / 2f;
            rt.anchorMax = Vector2.one / 2f;

            // get max size and clamp size
            var size = rt.rect.size;
            size.x = max;
            var clampedSize = Vector2.Min(size, rootRect.rect.size);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedSize.x);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedSize.y);

            // clamp position
            var allowedPositionExtends = (rootRect.rect.size - clampedSize) / 2f;
            var localPosition = rt.anchoredPosition;
            localPosition.x = Mathf.Clamp(localPosition.x, rootRect.rect.center.x - allowedPositionExtends.x, rootRect.rect.center.x + allowedPositionExtends.x);
            localPosition.y = Mathf.Clamp(localPosition.y, rootRect.rect.center.y - allowedPositionExtends.y, rootRect.rect.center.y + allowedPositionExtends.y);
            rt.anchoredPosition = localPosition;

            rt.SetParent(parent);
        }
    }
}