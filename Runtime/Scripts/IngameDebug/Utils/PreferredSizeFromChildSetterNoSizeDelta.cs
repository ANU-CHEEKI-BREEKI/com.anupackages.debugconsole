using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Utils.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LayoutElement))]
    public class PreferredSizeFromChildSetterNoSizeDelta : MonoBehaviour
    {
        [SerializeField] private RectTransform _child;
        [SerializeField] private bool _preferredHeight = true;
        [SerializeField] private bool _preferredWidth = true;
        [Space]
        [SerializeField] private int _paddingHeight = 0;
        [SerializeField] private int _paddingWidth = 0;

        private LayoutElement _element;

        private void Awake() => _element = GetComponent<LayoutElement>();

        private void LateUpdate()
        {
            if (_preferredHeight)
                _element.preferredHeight = LayoutUtility.GetPreferredHeight(_child) + _paddingHeight;

            if (_preferredWidth)
                _element.preferredWidth = LayoutUtility.GetPreferredWidth(_child) + _paddingWidth;
        }
    }
}