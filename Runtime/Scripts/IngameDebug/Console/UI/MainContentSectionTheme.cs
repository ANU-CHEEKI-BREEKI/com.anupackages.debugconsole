using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class MainContentSectionTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.Main_Background ?? Color.black;
        }
    }
}