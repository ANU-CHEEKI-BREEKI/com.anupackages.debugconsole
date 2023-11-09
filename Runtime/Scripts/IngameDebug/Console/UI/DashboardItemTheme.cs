using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class DashboardItemTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.DashboardItem_Background ?? Color.black;

            foreach (var item in Foreground)
                item.color = obj?.DashboardItem_Foreground ?? Color.gray;

            foreach (var item in Font)
                item.color = obj?.DashboardItem_Font ?? Color.white;

            foreach (var item in ScrollBarBackgroind)
                item.color = obj?.DashboardItem_ScrollBar_Background ?? Color.white;

            foreach (var item in ScrollBarForeground)
                item.color = obj?.DashboardItem_ScrollBar_Foreground ?? Color.white;

            foreach (var item in Selectable)
            {
                var c = item.colors;
                c.selectedColor = obj?.DashboardItem_FontSelected ?? Color.yellow;
                c.pressedColor = obj?.DashboardItem_FontPressed ?? new Color(Color.yellow.r - 0.2f, Color.yellow.g - 0.2f, Color.yellow.b - 0.2f);
                item.colors = c;
            }

            foreach (var item in Selection)
                item.color = obj?.DashboardItem_FontSelected ?? Color.white;

            foreach (var item in Required)
                item.color = obj?.DashboardItem_Required ?? Color.white;

            foreach (var item in NonRequired)
                item.color = obj?.DashboardItem_NonRequired ?? Color.white;

            foreach (var item in Toggle)
                item.color = obj?.DashboardItem_Toggle ?? Color.white;
        }
    }
}