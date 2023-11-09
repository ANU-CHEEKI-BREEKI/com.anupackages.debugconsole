using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class DashboardTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Header)
                item.color = obj?.Dashboard_Header ?? Color.black;

            foreach (var item in Background)
                item.color = obj?.Dashboard_Background ?? Color.black;

            foreach (var item in Foreground)
                item.color = obj?.Dashboard_Foreground ?? Color.gray;

            foreach (var item in Font)
                item.color = obj?.Dashboard_Font ?? Color.white;

            foreach (var item in ScrollBarBackgroind)
                item.color = obj?.Dashboard_ScrollBar_Background ?? Color.white;

            foreach (var item in ScrollBarForeground)
                item.color = obj?.Dashboard_ScrollBar_Foreground ?? Color.white;

            // foreach (var item in Selectable)
            // {
            //     var c = item.colors;
            //     c.selectedColor = obj?.Suggestions_FontSelected ?? Color.yellow;
            //     c.pressedColor = obj?.Suggestions_FontPressed ?? new Color(Color.yellow.r - 0.2f, Color.yellow.g - 0.2f, Color.yellow.b - 0.2f);
            //     item.colors = c;
            // }
        }
    }
}