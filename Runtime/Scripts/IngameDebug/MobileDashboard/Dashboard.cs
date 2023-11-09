using System.Collections;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    public class Dashboard : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private DashboardGroup _groupPrefab;
        [Space]
        [SerializeField] private Transform _categoriesFilterContent;
        [SerializeField] private ToggleGroup _categoryGroup;
        [SerializeField] private CategoryFilterToggle _categoryFilterGroupPrefab;
        [Space]
        [SerializeField] private FloatingRectTransform _floatingOpenButton;
        [SerializeField] private Button _closeConsole;
        [Space]
        [SerializeField] private CommandInfoPanel _infoPanel;

        private Coroutine _observer;

        private float FloatingButtonPositionX
        {
            get => PlayerPrefs.GetFloat("FloatingButtonPositionX", 0.5f);
            set => PlayerPrefs.SetFloat("FloatingButtonPositionX", value);
        }
        private float FloatingButtonPositionY
        {
            get => PlayerPrefs.GetFloat("FloatingButtonPositionY", 0.5f);
            set => PlayerPrefs.SetFloat("FloatingButtonPositionY", value);
        }

        private Vector2 FloatingButtonPosition
        {
            get => new Vector2(FloatingButtonPositionX, FloatingButtonPositionY);
            set
            {
                FloatingButtonPositionX = value.x;
                FloatingButtonPositionY = value.y;
            }
        }

        private void Awake()
        {
            _floatingOpenButton.Clicked += args => DebugConsole.Open();
            _floatingOpenButton.DragEnd += args => FloatingButtonPosition = _floatingOpenButton.RT.anchorMin;
            DebugConsole.IsOpenedChanged += () => _floatingOpenButton.gameObject.SetActive(!DebugConsole.IsOpened);
            _floatingOpenButton.gameObject.SetActive(!DebugConsole.IsOpened);
            _closeConsole.onClick.AddListener(DebugConsole.Close);

            _floatingOpenButton.RT.anchorMin = FloatingButtonPosition;
            _floatingOpenButton.RT.anchorMax = FloatingButtonPosition;
            _floatingOpenButton.RT.anchoredPosition = Vector2.zero;
        }

        private void OnEnable() => _observer = StartCoroutine(DeviseOrientationObserver());
        private void OnDisable() => StopCoroutine(_observer);

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);

            _content.DeleteAllChild();
            _categoriesFilterContent.DeleteAllChild();

            //TODO: all "favorites" group
            var commands = DebugConsole
                .Commands
                .Commands
                .Values
                .OfType<MemberCommand>()
                .Select(c => new
                {
                    Command = c,
                    Group = c.Name.Contains('.') ? c.Name.Substring(0, c.Name.LastIndexOf('.')) : "other",
                    ShortName = c.Name.Contains('.') ? c.Name.Substring(c.Name.LastIndexOf('.') + 1) : c.Name,
                })
                .OrderBy(c => c.Group)
                .ThenBy(c => c.ShortName)
                .GroupBy(c => c.Group)
                .ToArray();

            foreach (var group in commands)
            {
                var category = Instantiate(_categoryFilterGroupPrefab, _categoriesFilterContent);
                category.Present(group.Key, _categoryGroup);
                category.Toggle.onValueChanged.AddListener(isOn =>
                {
                    if (!isOn)
                        return;

                    _content.DeleteAllChild();
                    var groupContent = Instantiate(_groupPrefab, _content);
                    groupContent.Initialize(group.Key, group.Select(g => g.Command), false);
                    groupContent.InfoRequested += OpenInfo;
                });
            }

            var allCategory = Instantiate(_categoryFilterGroupPrefab, _categoriesFilterContent);
            allCategory.transform.SetAsFirstSibling();
            allCategory.Present("All", _categoryGroup);
            allCategory.Toggle.onValueChanged.AddListener(isOn =>
            {
                if (!isOn)
                    return;

                _content.DeleteAllChild();
                foreach (var group in commands)
                {
                    var groupContent = Instantiate(_groupPrefab, _content);
                    groupContent.Initialize(group.Key, group.Select(g => g.Command), true);
                    groupContent.InfoRequested += OpenInfo;
                }
            });

            var space = new GameObject("space").AddComponent<LayoutElement>();
            space.transform.SetParent(_categoriesFilterContent);
            space.transform.SetAsLastSibling();
            space.flexibleWidth = 1_000_000;

            _categoriesFilterContent.GetComponentInChildren<Toggle>().isOn = true;
        }

        private void OpenInfo(MemberCommand command) => _infoPanel.Show(command);

        private IEnumerator DeviseOrientationObserver()
        {
            var waiter = new WaitForSecondsRealtime(0.2f);
            var lastScreenHeight = Screen.height;
            var lastScreenWidth = Screen.width;

            while (true)
            {
                var changed = lastScreenHeight != Screen.height || lastScreenWidth != Screen.width;

                lastScreenHeight = Screen.height;
                lastScreenWidth = Screen.width;

                if (changed)
                {
                    DebugConsole.ExecuteCommand("console.refresh-size", silent: true);
                    DebugConsole.ExecuteCommand("console.refresh-scale", silent: true);
                }

                yield return waiter;
            }
        }
    }
}