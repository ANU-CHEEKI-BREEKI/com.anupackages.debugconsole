using ANU.IngameDebug.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [RequireComponent(typeof(Toggle))]
    public class FoldoutToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;

        private Transform _graphic;

        private Coroutine _coroutine;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(isOn =>
            {
                if (_coroutine != null)
                    StopCoroutine(_coroutine);
                _coroutine = this.TweenAnimation(UpdateState);
            });
            _graphic = _toggle.graphic.transform;
            _toggle.graphic = null;
        }

        private void OnEnable() => UpdateState(1f);
        private void OnDisable() => UpdateState(1f);

        private void UpdateState(float v)
        {
            var isOn = _toggle.isOn;
            var on = Quaternion.Euler(0, 0, 0);
            var off = Quaternion.Euler(0, 0, 90);

            var from = isOn ? off : on;
            var to = isOn ? on : off;

            _graphic.transform.localRotation = Quaternion.Lerp(from, to, v);
        }
    }
}
