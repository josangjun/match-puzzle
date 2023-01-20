namespace UnityEngine
{
    public static class SLogger
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.DebuggerHidden]
        public static void LogEditor(object message, Object context = null)
        {
            Debug.Log(message, context);
        }

        public static void LogWarning(object message, Object context = null) => Debug.LogWarning(message, context);

        public static void LogError(object message, Object context = null) => Debug.LogError(message, context);

#if FINAL_BUILD
        [System.Diagnostics.Conditional("__UNDEFINED_CONDITION__")]
#endif
        [System.Diagnostics.DebuggerHidden]
        public static void Log(object message, Object context = null)
        {
            Debug.Log(message, context);
        }
    }
}