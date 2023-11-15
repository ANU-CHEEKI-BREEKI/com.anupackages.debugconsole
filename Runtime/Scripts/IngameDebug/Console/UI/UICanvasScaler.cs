using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [DebugCommandPrefix("console")]
    public class UICanvasScaler : MonoBehaviour
    {
        private const string PrefsSaveUIScalePrefix = nameof(DebugConsole) + nameof(UICanvasScaler);
        private const string PrefsSaveUIScale_ScaleValue = PrefsSaveUIScalePrefix + nameof(CurrentScale);
        private const string PrefsSaveUIScale_ScaleStep = PrefsSaveUIScalePrefix + nameof(ScaleStep);

        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private Vector2 _initialResolutionReference;
        [Space]
        [SerializeField] private Button _plus;
        [SerializeField] private Button _minus;
        [SerializeField] private TMP_InputField _exact;
        [SerializeField] private int _step = 10;

        private int DefaultScale =>
#if !UNITY_EDITOR &&(UNITY_ANDROID || UNITY_IOS)
            150
#else
            100
#endif
            ;

        private int ScaleStep
        {
            get => PlayerPrefs.GetInt(PrefsSaveUIScale_ScaleStep, _step);
            set => PlayerPrefs.SetInt(PrefsSaveUIScale_ScaleStep, value);
        }
        private int CurrentScale
        {
            get => PlayerPrefs.GetInt(PrefsSaveUIScale_ScaleValue, DefaultScale);
            set => PlayerPrefs.SetInt(PrefsSaveUIScale_ScaleValue, value);
        }

        private void Awake()
        {
            _exact.onValidateInput += (string text, int charIndex, char addedChar) => char.IsDigit(addedChar) ? addedChar : '\0';
            _exact.onSelect.AddListener(s => _exact.text = _exact.text.Replace('%', '\0'));
            _exact.onSubmit.AddListener(s =>
            {
                if (!int.TryParse(s, out var scale))
                    ResetInput();
                else
                    ConsoleScale(scale);
            });
            _exact.onDeselect.AddListener(s => ResetInput());

            _plus.onClick.AddListener(() => ConsoleScale(CurrentScale + ScaleStep));
            _minus.onClick.AddListener(() => ConsoleScale(CurrentScale - ScaleStep));
        }

        private void OnEnable() => ConsoleScale(CurrentScale);
        public void RefreshConsoleScale() => ConsoleScale(CurrentScale);
        private void ResetInput() => _exact.text = CurrentScale.ToString() + "%";

        [DebugCommand(Name = "scale", Description = "Set console ui scale.", DisplayOptions = CommandDisplayOptions.Console)]
        private void ConsoleScale(
            [OptVal("50", "75", "100", "125", "150", "200")]
            [OptAltNames("v")]
            int value,
            [OptDesc("Set this flag to apply value as \"scale step\" for ui buttons '-' and '+' instead of actual ui scale")]
            [OptAltNames("s")]
            bool step = false
        )
        {
            if (step)
                ScaleStep = Mathf.Clamp(value, 1, 50);
            else
                SetScale(value);
        }

        [DebugCommand(Description = "Set console ui scale.", DisplayOptions = CommandDisplayOptions.Dashboard)]
        private void SetScale([OptVal("100", "125", "150", "200")][OptAltNames("v")] int value)
        {
            CurrentScale = Mathf.Clamp(value, 50, 200);
            ResetInput();
            _scaler.referenceResolution = _initialResolutionReference / (CurrentScale / 100f);
            DebugConsole.ExecuteCommand("console.refresh-size", silent: true);
        }
    }
}