using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class MainContentSectionTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.Main_Background ?? Color.black;

            foreach (var item in Font)
                item.color = obj?.Font ?? Color.white;
         
            foreach (var item in Selection)
                item.color = obj?.Foreground ?? Color.white;
            
            foreach (var item in Header)
                item.color = obj?.Header ?? Color.white;
        }
    }
}