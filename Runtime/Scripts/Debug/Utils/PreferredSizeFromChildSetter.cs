using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Utils.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LayoutElement))]
    public class PreferredSizeFromChildSetter : MonoBehaviour
    {
        [SerializeField] private RectTransform _child;
        [SerializeField] private bool _preferedHeight = true;
        [SerializeField] private bool _preferredWidth = true;

        private LayoutElement _element;

        private void Awake()
        {
            _element = GetComponent<LayoutElement>();
        }

        private void LateUpdate()
        {
            if (_preferedHeight)
                _element.preferredHeight = LayoutUtility.GetPreferredHeight(_child);
            if (_preferredWidth)
                _element.preferredWidth = LayoutUtility.GetPreferredWidth(_child);
        }
    }
}