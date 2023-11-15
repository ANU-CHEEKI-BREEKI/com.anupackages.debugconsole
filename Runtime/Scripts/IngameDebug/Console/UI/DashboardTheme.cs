using System;
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

            foreach (var item in Info)
                item.color = obj?.Message_Log ?? Color.white;

            foreach (var item in Warning)
                item.color = obj?.Message_Warnings ?? Color.white;

            foreach (var item in Error)
                item.color = obj?.Message_Errors ?? Color.white;
        }
    }
}