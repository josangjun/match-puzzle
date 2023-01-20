namespace UnityEngine
{
    public static class AnimationEx
    {
        public static void Sample(this Animation a, string clipName)
        {
            a.Play(clipName);
            a.Sample();
            a.Stop();
        }
    }
}

