using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    [TestFixture]
    public class SupportedTypes : TestBase
    {
        public override void SetUpOnce()
        {
            base.SetUpOnce();

            Context.Commands.RegisterCommand<bool, bool>("echo-bool", "", v => v);
            Context.Commands.RegisterCommand<Quaternion, Quaternion>("echo-quat", "", v => v);
            Context.Commands.RegisterCommand<Color32, Color32>("echo-col32", "", v => v);
            Context.Commands.RegisterCommand<Color, Color>("echo-col", "", v => v);
            Context.Commands.RegisterCommand<Vector2Int, Vector2Int>("echo-vec2int", "", v => v);
            Context.Commands.RegisterCommand<Vector2, Vector2>("echo-vec2", "", v => v);
            Context.Commands.RegisterCommand<Vector3Int, Vector3Int>("echo-vec3int", "", v => v);
            Context.Commands.RegisterCommand<Vector3, Vector3>("echo-vec3", "", v => v);
            Context.Commands.RegisterCommand<Vector4, Vector4>("echo-vec4", "", v => v);
            Context.Commands.RegisterCommand<GameObject, GameObject>("echo-go", "", v => v);

            Context.Commands.RegisterCommand<int[], int[]>("echo-arr-int", "", v => v);
            Context.Commands.RegisterCommand<Vector2[], Vector2[]>("echo-arr-vec2", "", v => v);

            Context.Commands.RegisterCommand<List<int>, List<int>>("echo-list-int", "", v => v);
            Context.Commands.RegisterCommand<List<Vector2>, List<Vector2>>("echo-list-vec2", "", v => v);
        }

        public void Echo<T>(T expected, string echo, string str)
        {
            var e = string.IsNullOrEmpty(echo)
                ? "echo"
                : $"echo-{echo}";
            var result = Context.ExecuteCommand($"{e} {str}");
            Assert.AreEqual(expected, result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test] public void Echo_String_DoubleQuotes() => Echo("Hello! And welcome to the los pollos hermanos", "", "\"Hello! And welcome to the los pollos hermanos\"");
        [Test] public void Echo_String_SingleQuotes() => Echo("Hello! And welcome to the los pollos hermanos", "", "'Hello! And welcome to the los pollos hermanos'");

        [Test] public void Echo_Bool_True_One() => Echo(true, "bool", "1");
        [Test] public void Echo_Bool_True_Plus() => Echo(true, "bool", "+");
        [Test] public void Echo_Bool_True_Yes() => Echo(true, "bool", "yes");
        [Test] public void Echo_Bool_True_T() => Echo(true, "bool", "y");
        [Test] public void Echo_Bool_True_Approve() => Echo(true, "bool", "approve");
        [Test] public void Echo_Bool_True_Apply() => Echo(true, "bool", "apply");
        [Test] public void Echo_Bool_True_On() => Echo(true, "bool", "on");

        [Test] public void Echo_Bool_False_Zero() => Echo(false, "bool", "0");
        [Test] public void Echo_Bool_False_Minus() => Echo(false, "bool", "-");
        [Test] public void Echo_Bool_False_No() => Echo(false, "bool", "no");
        [Test] public void Echo_Bool_False_N() => Echo(false, "bool", "n");
        [Test] public void Echo_Bool_False_Discard() => Echo(false, "bool", "discard");
        [Test] public void Echo_Bool_False_Cancel() => Echo(false, "bool", "cancel");
        [Test] public void Echo_Bool_False_Off() => Echo(false, "bool", "off");

        [Test] public void Echo_Quaternion() => Echo(Quaternion.Euler(1.3f, 2, 4.85f), "quat", "[1.3, 2, 4.85]");

        [Test] public void Echo_Color32() => Echo(new Color32(0, 255, 255 / 2, 0), "col32", "[0, 1, 0.5, 0]");

        [Test] public void Echo_Color_Vec3() => Echo(new Color(0.3f, 0.23f, 0.5f), "col", "[0.3  0.23  0.5]");
        [Test] public void Echo_Color_Vec4() => Echo(new Color(0.3f, 0.23f, 0.5f, 0.99f), "col", "[0.3  0.23  0.5, 0.99]");
        [Test] public void Echo_Color_Name_Red() => Echo(Color.red, "col", "Red");
        [Test] public void Echo_Color_Name_Green() => Echo(Color.green, "col", "GREEN");
        [Test] public void Echo_Color_Name_Magenta() => Echo(Color.magenta, "col", "magenta");
        [Test] public void Echo_Color_Html_RGB() => Echo(ColorUtility.TryParseHtmlString("#ABC", out var col) ? col : default, "col", "#ABC");
        [Test] public void Echo_Color_Html_RRGGBBAA() => Echo(ColorUtility.TryParseHtmlString("#AABBCCDD", out var col) ? col : default, "col", "#AABBCCDD");

        [Test] public void Echo_Vec2Int() => Echo(new Vector2Int(1, 2), "vec2int", "(1, 2)");
        [Test] public void Echo_Vec2() => Echo(new Vector2(1.25f, 2), "vec2", "(1.25 2)");

        [Test] public void Echo_Vec3Int() => Echo(new Vector3Int(1, 2, 3), "vec3int", "(1, 2, 3)");
        [Test] public void Echo_Vec3_BracketsSquare() => Echo(new Vector3(1, 2, 3), "vec3", "[1,2,3]");
        [Test] public void Echo_Vec3_BracketsRound() => Echo(new Vector3(1, 2, 3), "vec3", "(1,2,3)");
        [Test] public void Echo_Vec3_BracketsSquare_WSpaces() => Echo(new Vector3(1, 2, 3), "vec3", "[1 ,2 ,  3]");
        [Test] public void Echo_Vec3_BracketsRound_WSpaces() => Echo(new Vector3(1, 2, 3), "vec3", "(1 , 2 , 3)");

        [Test] public void Echo_Vec3_Zero() => Echo(Vector3.zero, "vec3", "[]");
        [Test] public void Echo_Vec3_N() => Echo(Vector3.one * 3, "vec3", "(3)");
        [Test] public void Echo_Vec3_N_NoBrackets() => Echo(Vector3.one * 3, "vec3", "3");

        [Test] public void Echo_Vec4() => Echo(new Vector4(1, 2, 3, 4), "vec4", "[1,2,3 4]");

        [Test] public void Echo_GO_Null_1() => Echo(null as GameObject, "go", "null");
        [Test] public void Echo_GO_Null_2() => Echo(null as GameObject, "go", "NULL");
        [Test] public void Echo_GO_Null_3() => Echo(null as GameObject, "go", "NULL");

        [Test] public void Echo_Arr_Int() => Echo(new int[] { 1, 2, 3 }, "arr-int", "(1, 2, 3)");
        [Test] public void Echo_List_Int() => Echo(new List<int> { 1, 2, 3 }, "list-int", "(1, 2, 3)");

        [Test] public void Echo_Arr_Vec2() => Echo(new Vector2[] { Vector2.one * 1, Vector2.one * 2, Vector2.one * 3 }, "arr-vec2", "(1, 2, 3)");
        [Test] public void Echo_List_Vec2() => Echo(new List<Vector2> { Vector2.one * 1, Vector2.one * 2, Vector2.one * 3 }, "list-vec2", "(1, 2, 3)");

        [Test] public void Echo_Arr_Vec2_Dif() => Echo(new Vector2[] { new Vector2(0.2f, 5), new Vector2(35.4f, 29) }, "arr-vec2", "((0.2, 5) (35.4, 29))");
        [Test] public void Echo_List_Vec2_Dif() => Echo(new List<Vector2> { new Vector2(0.2f, 5), new Vector2(35.4f, 29) }, "list-vec2", "((0.2, 5) (35.4, 29))");
    }
}
