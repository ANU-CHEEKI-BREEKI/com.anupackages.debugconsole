using System.Collections;
using System.Collections.Generic;
using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    public class Dashboard : MonoBehaviour
    {
        [SerializeField] private ButtonCommandPresenter _buttonPrefab;
        [SerializeField] private SwitchCommandPresenter _switchPrefab;
        [SerializeField] private DropDownCommandPresenter _dropdownPrefab;
        [SerializeField] private InputCommandPresenter _inputPrefab;
        [Space]
        [SerializeField] private Transform _content;

        private void Start()
        {
            _content.DeleteAllChild();

            //TODO: group commands by Prefix


            foreach (var item in DebugConsole.Commands.Commands)
            {
                // ignore reserved parameters

                // if we have 0 arguments - just show button
                // if we have 1 argument
                //      bool        - switch
                //      less than 4 vals - switch
                //      more than 4 vals - dropdown 
                //      generic val - input field
                // if we have more than 1 argument - show 2 buttons - with command name, and "..." for opening window to enter arguments. entered arguments should remember last input
            }
        }
    }

    internal abstract class ArgumentPresenterBase : MonoBehaviour
    {
        public abstract void Present(ADebugCommand command);
    }

    internal class ButtonCommandPresenter : ArgumentPresenterBase
    {
        public override void Present(ADebugCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
    internal class SwitchCommandPresenter : ArgumentPresenterBase
    {
        public override void Present(ADebugCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
    internal class DropDownCommandPresenter : ArgumentPresenterBase
    {
        public override void Present(ADebugCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
    internal class InputCommandPresenter : ArgumentPresenterBase
    {
        public override void Present(ADebugCommand command)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// Choose one of the values, which always visible
    /// </summary>
    internal class Switch : MonoBehaviour
    {

    }
}