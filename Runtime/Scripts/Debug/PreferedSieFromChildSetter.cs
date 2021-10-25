using UnityEngine;
using UnityEngine.UI;

namespace IngameDebug
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LayoutElement))]
    public class PreferedSieFromChildSetter : MonoBehaviour
    {
        [SerializeField] private RectTransform _child;
        [SerializeField] private bool _preferedHeight = true;
        [SerializeField] private bool _preferedWidth = true;

        private LayoutElement _element;

        private void Awake()
        {
            _element = GetComponent<LayoutElement>();
        }

        private void LateUpdate()
        {
            if (_preferedHeight)
                _element.preferredHeight = LayoutUtility.GetPreferredHeight(_child);
            if (_preferedWidth)
                _element.preferredWidth = LayoutUtility.GetPreferredWidth(_child);
        }
    }
}