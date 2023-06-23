using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class ConsoleListSectionTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.ConsoleList_Background ?? Color.black;
            
            foreach (var item in Foreground)
                item.color = obj?.ConsoleList_Foreground ?? Color.gray;

            foreach (var item in ScrollBarBackgroind)
                item.color = obj?.ConsoleList_ScrollBar_Background ?? Color.black;

            foreach (var item in ScrollBarForeground)
                item.color = obj?.ConsoleList_ScrollBar_Foreground ?? Color.gray;
        }
    }
}