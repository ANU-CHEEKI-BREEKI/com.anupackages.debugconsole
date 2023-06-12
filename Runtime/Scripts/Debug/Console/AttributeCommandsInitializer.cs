using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ANU.IngameDebug.Console;
using ANU.IngameDebug.Console.Commands.Implementations;
using NDesk.Options;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public class AttributeCommandsInitializer : MonoBehaviour
    {
        private void Start()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            DebugConsole.RegisterCommands(
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm => asm.GetTypes())
                    .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .Select(method => new
                    {
                        method = method,
                        attribute = method.GetCustomAttribute<DebugCommandAttribute>(),
                    })
                    .Where(method =>
                        method.attribute != null
                        && method.method.ReturnType == typeof(void)
                        && method.method.GetParameters()
                            .All(paramater => paramater.ParameterType.IsPrimitive
                                || paramater.ParameterType == typeof(string)
                                || paramater.ParameterType.IsEnum
                            )
                    )
                    .Select(method => new
                    {
                        method = method.method,
                        attribute = method.attribute,
                        name = string.IsNullOrEmpty(method.attribute.Name)
                            ? string.Join(
                                "",
                                method.method.Name
                                    .Select(c =>
                                        char.IsUpper(c) ? "-" + char.ToLower(c) : c.ToString()
                                    )
                            ).Trim('-')
                            : method.attribute.Name,
                        description = method.attribute?.Description
                    })
                    .GroupBy(method => method.name)
                    .SelectMany(group =>
                    {
                        // if count greater than 1
                        // add command name prefix as DeclaringType
                        var addPrefix = group.Skip(1).Any();
                        return group.Select(method => new MethodCommand(
                            name: addPrefix ? $"{method.method.DeclaringType.Name}-{method.name}" : method.name,
                            description: method.description,
                            method: method.method,
                            instance: null
                        ));
                    })
                    .ToArray()
            );
        }
    }
}