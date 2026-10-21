using System;
using UnityEngine;
using UnityEngine.Scripting;

public static class GarbageCollectionManager
{
    public static void DisableGC()
    {
        // Exclude both the Editor and WebGL platforms
#if !UNITY_EDITOR && !UNITY_WEBGL
        GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
#endif
    }

    public static void CollectGarbage()
    {
        // Exclude both the Editor and WebGL platforms
#if !UNITY_EDITOR && !UNITY_WEBGL
        GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        GC.Collect();
        GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
#endif
    }
}