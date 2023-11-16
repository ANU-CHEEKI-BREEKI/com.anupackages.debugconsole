using UnityEngine;
using UnityEngine.Assertions;

public class DemoMessages : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("This is the info log");
        Debug.LogWarning("This is the waring log");
        Debug.LogError("This is the error log");
        Debug.LogException( new System.Exception("This is the exception log"));
        Assert.AreEqual(1, 2);
    }
}
