using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [ExecuteAlways]
    public class DashboardLayout : LayoutGroup
    {
        [SerializeField] private float _spacing = 15f;

        public IReadOnlyList<RectTransform> RectChildren => rectChildren;

        private Grid<RectTransform> _grid = new();

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            _grid.Clear();
            var maxWidth = rectTransform.rect.width - padding.left - padding.right;
            var width = 0f;

            foreach (var item in rectChildren)
            {
                var minWidth = LayoutUtility.GetMinWidth(item);
                if (minWidth + width > maxWidth)
                {
                    _grid.AddToNewRow(item);
                    width = 0;
                }
                else
                {
                    _grid.AddToCurrentRow(item);
                }
                width += minWidth + _spacing;
            }

            var x = (float)padding.left;
            var y = (float)padding.top;
            var lastMinY = 0f;

            for (int r = 0; r < _grid.Rows.Count; r++)
            {
                var row = _grid.Rows[r];

                // go foreach item in fow and calc sum of sizes
                var sumMinWidth = 0f;
                var sumPrefWidth = 0f;
                var sumFlexWidth = 0f;

                for (int c = 0; c < row.Count; c++)
                {
                    var item = row[c];
                    sumMinWidth += LayoutUtility.GetMinWidth(item);
                    sumPrefWidth += LayoutUtility.GetPreferredWidth(item);
                    sumFlexWidth += Mathf.Max(1, LayoutUtility.GetFlexibleWidth(item));
                }

                var totalWidth = maxWidth - (row.Count - 1) * _spacing;
                if (sumMinWidth < totalWidth / 3f * 2f)
                    sumMinWidth += (totalWidth - sumMinWidth) * 0.7f;

                // go foreach item in fow and set positions
                for (int c = 0; c < row.Count; c++)
                {
                    var item = row[c];
                    lastMinY = LayoutUtility.GetMinHeight(item);

                    var minX = LayoutUtility.GetMinWidth(item);
                    var prefX = LayoutUtility.GetPreferredWidth(item);
                    var flexX = Mathf.Max(1, LayoutUtility.GetFlexibleWidth(item));

                    var reqX = Mathf.Clamp(totalWidth, minX, prefX);
                    reqX += (totalWidth - sumMinWidth) * (flexX / sumFlexWidth);

                    SetChildAlongAxis(item, 0, x, reqX);
                    SetChildAlongAxis(item, 1, y, lastMinY);

                    x += reqX + _spacing;
                }

                y += lastMinY + _spacing;
                x = padding.left;
            }
            SetLayoutInputForAxis(y, y, 0, 1);
        }

        public override void CalculateLayoutInputVertical() { }
        public override void SetLayoutHorizontal() { }
        public override void SetLayoutVertical() { }

        private class Grid<T>
        {
            private List<List<T>> _rows = new();

            public IReadOnlyList<IReadOnlyList<T>> Rows => _rows;

            public void Clear() => _rows.Clear();

            public void AddToCurrentRow(T item)
            {
                if (!_rows.Any())
                    _rows.Add(new List<T>());

                _rows.Last().Add(item);
            }

            public void AddToNewRow(T item)
            {
                _rows.Add(new List<T>());
                AddToCurrentRow(item);
            }
        }
    }
}
