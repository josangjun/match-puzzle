using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace System.Threading.Tasks
{
    public static partial class TaskEx
    {
        public const int UNIT_TIME = 20;

        private static bool IsWaitable(CancellationToken? cancel) => Application.isPlaying && (cancel == null || !cancel.Value.CanBeCanceled || !cancel.Value.IsCancellationRequested);

        async static public Task WaitForNextFrame(CancellationToken? cancel = null)
        {
            var frame = Time.frameCount;
            do
            {
                await Task.Delay(1);
            } while (IsWaitable(cancel) && frame == Time.frameCount);
        }

        async static public Task WaitUntil(Func<bool> p, CancellationToken? cancel = null)
        {
            do
            {
                await Task.Delay(UNIT_TIME);
            } while (IsWaitable(cancel) && !p());
        }
        async static public Task WaitWhile(Func<bool> p, CancellationToken? cancel = null)
        {
            do
            {
                await Task.Delay(UNIT_TIME);
            } while (IsWaitable(cancel) && p());
        }

        public static IEnumerator ToCoroutine(this Task t)
        {
            return new TaskEnumerator(t);
        }

        public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation op)
        {
            return WaitUntilDone(op).GetAwaiter();

            async Task WaitUntilDone(UnityWebRequestAsyncOperation o)
            {
                while (Application.isPlaying && !o.isDone)
                {
                    await Task.Delay(UNIT_TIME);
                }
            }
        }

        public static TaskAwaiter GetAwaiter(this Animation ani)
        {
            return WaitUntilDone(ani).GetAwaiter();

            async Task WaitUntilDone(Animation a)
            {
                while (Application.isPlaying && a.isPlaying && a.isActiveAndEnabled)
                {
                    await Task.Delay(UNIT_TIME);
                }
            }
        }

        public static TaskAwaiter GetAwaiter(this TimeSpan timeSpan)
        {
            return Task.Delay(timeSpan).GetAwaiter();
        }

        public static TaskAwaiter GetAwaiter(this DG.Tweening.Tweener tweener)
        {
            async Task WaitUntilDone()
            {
                while (tweener.active)
                {
                    await Task.Yield();
                }
            }
            return WaitUntilDone().GetAwaiter();
        }

        [Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ConfirmPlaying()
        {
            if (!Application.isPlaying)
            {
                throw new Exception("Quit Playing");
            }
        }
    }

    public struct TaskEnumerator : IEnumerator
    {
        private readonly Task task;

        public TaskEnumerator(Task t)
        {
            this.task = t;
        }
        public object Current => 0;

        public bool MoveNext()
        {

#if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                throw new Exception("Quit Playing");
            }
#endif
            if (task != null)
            {
                switch (task.Status)
                {
                    case TaskStatus.Running:
                    case TaskStatus.WaitingForActivation:
                    case TaskStatus.WaitingForChildrenToComplete:
                    case TaskStatus.WaitingToRun:
                        return true;
                    case TaskStatus.Created:
                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled:
                    case TaskStatus.RanToCompletion:
                        return false;
                    default:
                        return true;

                }
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
        }
    }
}

