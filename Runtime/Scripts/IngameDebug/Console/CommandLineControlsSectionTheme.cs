using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class CommandLineControlsSectionTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.CommandLineControls_Background ?? Color.gray;

            foreach (var item in Font)
                item.color = obj?.CommandLineControls_Font ?? Color.white;
        }
    }
}