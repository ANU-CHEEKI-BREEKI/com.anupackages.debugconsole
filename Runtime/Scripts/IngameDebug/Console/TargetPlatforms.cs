using System;

namespace ANU.IngameDebug.Console
{
    [Flags]
    public enum TargetPlatforms
    {
        Any = 1 << 0,
        PC = 1 << 1,
        Mobile = 1 << 2,
        Editor = 1 << 3
    }
}