
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANU.IngameDebug.Console;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: RegisterDebugCommandTypes(typeof(DefaultCommands))]
[assembly: RegisterDebugCommandTypes(typeof(DefaultCommandsNoPrefix))]

namespace ANU.IngameDebug.Console
{
    [DebugCommandPrefix("default")]
    internal class DefaultCommands
    {
        [DebugCommand]
        private static float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = Mathf.Clamp01(value);
        }

        [DebugCommand(Description = "Print all Scenes GameObjects hierarchy")]
        private static void DisplayHierarchy(
            [OptAltNames("i")]
            [OptDesc("Print hierarchy on internal DontDestroyOnLoad scene too")]
            bool includeDontDestroyOnLoad = true
        )
        {
            var sb = new StringBuilder();
            var scenesCount = SceneManager.sceneCount;

            sb.AppendLine();

            for (int i = 0; i < scenesCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                DisplayHierarchy(scene, sb);
            }

            if (includeDontDestroyOnLoad)
            {
                var dontDestryScene = DebugConsole.Instance.transform.root.gameObject.scene;
                DisplayHierarchy(dontDestryScene, sb);
            }

            DebugConsole.Logger.LogReturnValue(sb.ToString());
        }

        private static void DisplayHierarchy(Scene scene, StringBuilder sb)
        {
            sb.AppendLine(scene.name);

            var rootObjects = scene.GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                sb.Append("|--");
                sb.AppendLine(obj.name);

                DisplayHierarchy(obj, sb, "|  ");
            }

        }
        private static void DisplayHierarchy(GameObject gameObject, StringBuilder sb, string intent)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);
                sb.Append(intent);
                sb.Append("|--");
                sb.AppendLine(child.gameObject.name);

                DisplayHierarchy(child.gameObject, sb, intent + (gameObject.transform.childCount > 1 ? "|  " : "   "));
            }
        }

        [DebugCommand]
        private static void ObjectInfo(GameObject[] gameObjects)
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            foreach (var item in gameObjects)
            {
                sb.Append(item.name);
                sb.Append("[");
                sb.Append(item.GetInstanceID());
                sb.AppendLine("]:");

                sb.Append("|--parent: ");
                sb.AppendLine(item.transform.parent.name);

                sb.Append("|--child count: ");
                sb.AppendLine(item.transform.childCount.ToString());
                for (int i = 0; i < item.transform.childCount; i++)
                {
                    sb.Append("   |--");
                    sb.AppendLine(item.transform.GetChild(i).name);
                }

                sb.AppendLine("|--components:");
                foreach (var c in item.GetComponents<Component>())
                    ComponentInfo(c, sb, "   ");
            }

            DebugConsole.Logger.LogReturnValue(sb.ToString());
        }

        private static void ComponentInfo(Component component, StringBuilder sb, string intent)
        {
            sb.Append(intent);
            sb.Append("|--");
            sb.AppendLine(component.GetType().Name);

            if (component is Transform transform)
                ComponentInfo(transform, sb, intent + "|  ");
        }

        private static void ComponentInfo(Transform transform, StringBuilder sb, string intent)
        {
            Append("Position", transform.position, sb, intent);
            Append("Local Position", transform.localPosition, sb, intent);

            Append("Rotaion", transform.eulerAngles, sb, intent);
            Append("Local Rotation", transform.localEulerAngles, sb, intent);

            Append("Lossy Scale", transform.lossyScale, sb, intent);
            Append("Local Scale", transform.localScale, sb, intent);
        }

        private static void Append(string propName, Vector3 v3, StringBuilder sb, string intent)
        {
            sb.Append(intent);
            sb.Append("|--");
            sb.Append(propName);
            sb.Append(": ");
            sb.AppendLine(v3.ToString());
        }

        [DebugCommand]
        private static void LoadScene(
            [OptAltNames("n")]
            [OptDesc("Load scene by name")]
            [OptValDynamic("default.list-scene-names")]
            string name = "",
            [OptAltNames("i")]
            [OptDesc("Load scene by index")]
            [OptValDynamic("default.list-scene-indices")]
            int index = -1,
            [OptAltNames("r")]
            [OptDesc("Set only this flag to reload current scene")]
            bool reload = false
        )
        {
            if (!string.IsNullOrEmpty(name))
                SceneManager.LoadScene(name);
            else if (index >= 0)
                SceneManager.LoadScene(index);
            else if (reload)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            else
                throw new System.Exception("Pass at least one parameter");
        }

        [DebugCommand]
        private static IEnumerable<string> ListSceneNames()
            => ListSceneIndices().Select(i => SceneUtility
                .GetScenePathByBuildIndex(i)
                .Split('/')
                .LastOrDefault()
                ?.Split('.')
                ?.FirstOrDefault()
            );

        [DebugCommand]
        private static IEnumerable<int> ListSceneIndices()
        {
            var cnt = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < cnt; i++)
                yield return i;
        }
    }

    internal class DefaultCommandsNoPrefix
    {
        [DebugCommand]
        private static string Echo(string value) => value;
    }
}