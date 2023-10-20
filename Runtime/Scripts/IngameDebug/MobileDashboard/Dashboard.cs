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

namespace ANU.IngameDebug.Console.Dashboard
{
    public class Dashboard : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private DashboardGroup _groupPrefab;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);

            _content.DeleteAllChild();

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
                .GroupBy(c => c.Group);

            foreach (var group in commands)
            {
                var groupContent = GameObject.Instantiate(_groupPrefab, _content);
                groupContent.Initialize(group.Key, group.Select(g => g.Command));
            }
        }
    }
}