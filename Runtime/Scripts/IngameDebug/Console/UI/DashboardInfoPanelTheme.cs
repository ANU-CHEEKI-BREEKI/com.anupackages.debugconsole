using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class DashboardInfoPanelTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Header)
                item.color = obj?.Header ?? Color.black;

            foreach (var item in Background)
                item.color = obj?.DashboardInfo_Background ?? Color.black;

            foreach (var item in Foreground)
                item.color = obj?.DashboardInfo_Foreground ?? Color.gray;

            foreach (var item in Font)
                item.color = obj?.DashboardInfo_Font ?? Color.white;

            foreach (var item in ScrollBarBackgroind)
                item.color = obj?.DashboardInfo_ScrollBar_Background ?? Color.white;

            foreach (var item in ScrollBarForeground)
                item.color = obj?.DashboardInfo_ScrollBar_Foreground ?? Color.white;
        }
    }
}