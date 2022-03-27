using System.Collections.Generic;
using System.Linq;
using IngameDebug.Commands.Implementations;
using NDesk.Options;
using UnityEngine;

namespace IngameDebug.Commands
{
    [DisallowMultipleComponent]
    public class DebugCommandsContainer : MonoBehaviour
    {
        private Dictionary<string, ADebugCommand> _commands;

        private Dictionary<string, ADebugCommand> CommandsInternal
        {
            get
            {
                if (_commands == null)
                    _commands = Initialize().ToDictionary(c => c.Name);
                return _commands;
            }
        }

        public IReadOnlyDictionary<string, ADebugCommand> Commands => _commands;

        // allow any debug class from this assembly to register new commands
        // you can access container from DebugConsole class static mathod/propery
        public void RegisterCommands(params ADebugCommand[] commands)
        {
            foreach (var c in commands)
                CommandsInternal.Add(c.Name, c);
        }

        private List<ADebugCommand> Initialize()
        {
            // add predefined debug commands from any other assembly/namespace/etc
            return new List<ADebugCommand>()
            {
                new LambdaCommand("time", "", optionValuesHint =>
                {
                    var set = new OptionSet();
                    set.Add<float>("scale=", scale => Time.timeScale = Mathf.Max(0, scale) );
                    return set;
                })
            };
        }

    }
}