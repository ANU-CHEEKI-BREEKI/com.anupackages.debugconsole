using System;

namespace ANU.IngameDebug.Console
{
    [Flags]
    public enum CommandDisplayOptions
    {
        Console = 1 << 0,
        Dashboard = 1 << 1,
    }
}