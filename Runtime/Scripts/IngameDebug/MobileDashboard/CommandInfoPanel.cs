using System;
using ANU.IngameDebug.Console.Commands.Implementations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(Canvas))]
    internal class CommandInfoPanel : MonoBehaviour
    {
        [SerializeField] private Color _blockerColor;

        private MemberCommand _command;
        private GameObject _blocker;

        private void Awake() => Hide();

        public void Show(MemberCommand command)
        {
            Hide();

            var rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

            var dropdownCanvas = this.GetComponent<Canvas>();
            dropdownCanvas.overrideSorting = true;
            dropdownCanvas.sortingOrder = rootCanvas.sortingOrder + 100;
            dropdownCanvas.sortingLayerID = rootCanvas.sortingLayerID;

            _command = command;

            _blocker = CreateBlocker(rootCanvas);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (_blocker != null)
                Destroy(_blocker);

            gameObject.SetActive(false);
        }

        protected virtual GameObject CreateBlocker(Canvas rootCanvas)
        {
            var blocker = new GameObject("Blocker");

            var blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            var blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;

            var dropdownCanvas = this.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

            Canvas parentCanvas = null;
            var parentTransform = transform.parent;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;

                parentTransform = parentTransform.parent;
            }

            if (parentCanvas != null)
            {
                var components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    var raycasterType = components[i].GetType();
                    if (blocker.GetComponent(raycasterType) == null)
                        blocker.AddComponent(raycasterType);
                }
            }
            else
            {
                GetOrAddComponent<GraphicRaycaster>(blocker);
            }

            var blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = _blockerColor;

            var blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            return blocker;
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (!comp)
                comp = go.AddComponent<T>();
            return comp;
        }
    }
}