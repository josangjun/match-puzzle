using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Studio.Common
{
    public static class AppInit
    {
        public static ILogger log => Debug.unityLogger;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterUnhandledLog()
        {
            AppDomain.CurrentDomain.UnhandledException -= PrintUnhandledLog;
            TaskScheduler.UnobservedTaskException -= PrintUnhandledTaskLog;
            AppDomain.CurrentDomain.UnhandledException += PrintUnhandledLog;
            TaskScheduler.UnobservedTaskException += PrintUnhandledTaskLog;

            void PrintUnhandledLog(object sender, UnhandledExceptionEventArgs e)
            {
                switch (e.ExceptionObject)
                {
                    case Exception ex:
                        log.Log(LogType.Error, ex.GetInnermostException());
                        break;
                    default:
                        log.Log(LogType.Error, e.ExceptionObject);
                        break;
                }
            }

            void PrintUnhandledTaskLog(object sender, UnobservedTaskExceptionEventArgs e)
            {
                var inner = e.Exception.GetBaseException().GetInnermostException();
                log.Log(LogType.Error, inner);
            }
        }
    }
}
