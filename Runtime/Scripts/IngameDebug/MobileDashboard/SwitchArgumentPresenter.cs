using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class SwitchArgumentPresenter : ArgumentPresenterBase
    {
        [SerializeField] private ToggleGroup _group;
        [SerializeField] private List<Toggle> _toggles;

        public override string Value
        {
            get
            {
                try
                {
                    var index = _toggles.IndexOf(_toggles.First(t => t.isOn));
                    var values = Command.ValueHints[Parameter.Option].ToList();
                    return values.Skip(index).Take(1).Single();
                }
                catch
                {
                    return null;
                }
            }
        }

        protected override void Initialize()
        {
            foreach (var item in _toggles)
                item.group = _group;
        }

        protected override void PresentInternal()
        {
            var values = Command.ValueHints[Parameter.Option].ToList();
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].gameObject.SetActive(i <= values.Count - 1);
                if (_toggles[i].gameObject.activeSelf)
                    _toggles[i].GetComponentInChildren<TextMeshProUGUI>().text = values[i];
            }

            _group.allowSwitchOff = !Parameter.IsRequired;
            if (_group.allowSwitchOff)
            {
                DeselectAll();
            }
            else
            {
                var str = DebugConsole.Converters.ConvertToString(Parameter.DefaultValue);
                var index = values.IndexOf(str);
                SetIsOnWithoutNotify(index);
            }
        }

        private void DeselectAll()
        {
            foreach (var item in _toggles)
                item.SetIsOnWithoutNotify(false);
        }

        private void SetIsOnWithoutNotify(int index)
        {
            index = Mathf.Clamp(index, 0, _toggles.Count);
            DeselectAll();
            _toggles[index].SetIsOnWithoutNotify(true);
        }
    }
}