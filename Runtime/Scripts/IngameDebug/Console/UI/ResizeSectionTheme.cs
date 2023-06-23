using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class ResizeSectionTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.Resize_Background ?? Color.black;

            foreach (var item in Foreground)
                item.color = obj?.Resize_Foreground ?? Color.gray;

            foreach (var item in Font)
                item.color = obj?.Resize_Font ?? Color.white;
        }
    }
}