using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class CommandLineSectionTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.CommandLine_Background ?? Color.black;

            foreach (var item in Foreground)
                item.color = obj?.CommandLine_Foreground ?? Color.gray;

            foreach (var item in Font)
                item.color = obj?.CommandLine_Font ?? Color.white;
        }
    }
}