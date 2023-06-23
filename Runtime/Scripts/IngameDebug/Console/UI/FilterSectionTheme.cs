using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class FilterSectionTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.Filter_Background ?? Color.black;

            foreach (var item in Foreground)
                item.color = obj?.Filter_Foreground ?? Color.gray;

            foreach (var item in Font)
                item.color = obj?.Filter_Font ?? Color.white;
        }
    }
}