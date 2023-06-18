using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class SuggestionsSectionTheme : SectionThemeBase
    {
        protected override void UpdateTheme(UITheme obj)
        {
            foreach (var item in Background)
                item.color = obj?.Suggestions_Background ?? Color.black;

            foreach (var item in Font)
                item.color = obj?.Suggestions_Font ?? Color.white;

            foreach (var item in Selectable)
            {
                var c = item.colors;
                c.selectedColor = obj?.Suggestions_FontSelected ?? Color.yellow;
                c.pressedColor = obj?.Suggestions_FontPressed ?? new Color(Color.yellow.r - 0.2f, Color.yellow.g - 0.2f, Color.yellow.b - 0.2f);
                item.colors = c;
            }

            foreach (var item in ScrollBarBackgroind)
                item.color = obj?.Suggestions_ScrollBar_Background ?? Color.black;

            foreach (var item in ScrollBarForeground)
                item.color = obj?.Suggestions_ScrollBar_Foreground ?? Color.gray;
        }
    }
}