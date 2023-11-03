using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Utils;
using Codice.CM.Common;
using NCalc.Domain;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    public class Dashboard : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private DashboardGroup _groupPrefab;
        [Space]
        [SerializeField] private Transform _categoriesFilterContent;
        [SerializeField] private ToggleGroup _categoryGroup;
        [SerializeField] private CategoryFilterToggle _categoryFilterGroupPrefab;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);

            _content.DeleteAllChild();
            _categoriesFilterContent.DeleteAllChild();

            //TODO: all "favorites" group
            var commands = DebugConsole
                .Commands
                .Commands
                .Values
                .OfType<MemberCommand>()
                .Select(c => new
                {
                    Command = c,
                    Group = c.Name.Contains('.') ? c.Name.Substring(0, c.Name.LastIndexOf('.')) : "other",
                    ShortName = c.Name.Contains('.') ? c.Name.Substring(c.Name.LastIndexOf('.') + 1) : c.Name,
                })
                .OrderBy(c => c.Group)
                .ThenBy(c => c.ShortName)
                .GroupBy(c => c.Group)
                .ToArray();

            foreach (var group in commands)
            {
                var category = Instantiate(_categoryFilterGroupPrefab, _categoriesFilterContent);
                category.Present(group.Key, _categoryGroup);
                category.Toggle.onValueChanged.AddListener(isOn =>
                {
                    if (!isOn)
                        return;

                    _content.DeleteAllChild();
                    var groupContent = Instantiate(_groupPrefab, _content);
                    groupContent.Initialize(group.Key, group.Select(g => g.Command), false);
                });
            }

            var allCategory = Instantiate(_categoryFilterGroupPrefab, _categoriesFilterContent);
            allCategory.transform.SetAsFirstSibling();
            allCategory.Present("All", _categoryGroup);
            allCategory.Toggle.onValueChanged.AddListener(isOn =>
            {
                if (!isOn)
                    return;

                _content.DeleteAllChild();
                foreach (var group in commands)
                {
                    var groupContent = Instantiate(_groupPrefab, _content);
                    groupContent.Initialize(group.Key, group.Select(g => g.Command), true);
                }
            });

            var space = new GameObject("space").AddComponent<LayoutElement>();
            space.transform.SetParent(_categoriesFilterContent);
            space.transform.SetAsLastSibling();
            space.flexibleWidth = 1_000_000;

            _categoriesFilterContent.GetComponentInChildren<Toggle>().isOn = true;
        }
    }
}