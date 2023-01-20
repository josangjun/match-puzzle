using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEngine
{
    public static class AnimatorEx
    {
        public static bool IsLocalEnd(this Animator a)
        {
            return a.IsLocalEnd(1f, 0);
        }

        public static bool IsLocalEnd(this Animator a, float normalizedTime, int stateHash)
        {
            if (a == null || a.isActiveAndEnabled == false)
                return true;
            var state = a.GetCurrentAnimatorStateInfo(0);
            if (stateHash != 0 && stateHash != state.shortNameHash)
                return true;
            return !a.IsInTransition(0) && (state.loop || state.normalizedTime >= normalizedTime);
        }

        public static bool IsTag(this Animator a, int tagHash)
        {
            var state = a.GetCurrentAnimatorStateInfo(0);
            return state.tagHash == tagHash;
        }

        public static TaskAwaiter GetAwaiter(this Animator ani)
        {
            return WaitUntilDone(ani).GetAwaiter();

            async Task WaitUntilDone(Animator a)
            {
                if (a == null || a.isActiveAndEnabled == false)
                {
                    return;
                }
                a.Update(0.01f);
                await TaskEx.WaitUntil(a.IsLocalEnd);

                //bool IsOver()
                //{
                //    var state = a.GetCurrentAnimatorStateInfo(0);
                //    return state.loop || (state.normalizedTime >= 1 && !a.IsInTransition(0));
                //}
            }
        }
    }
}

