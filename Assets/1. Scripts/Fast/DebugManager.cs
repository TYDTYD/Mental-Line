using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    static System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
    private void Start()
    {
        
    }
    public static void startCheckTime()
    {
        Debug.Log(watch.ElapsedMilliseconds + " ms");
        watch.Start();
    }

    public static void stopCheckTime()
    {
        Debug.Log(watch.ElapsedMilliseconds + " ms");
        watch.Stop();
    }
}
