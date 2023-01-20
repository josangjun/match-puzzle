namespace UnityEngine
{
    public static partial class GameObjectEx
    {
        public static void SetActiveSafe(this GameObject o, bool visible)
        {
            if (o == null)
            {
                return;
            }
            o.SetActive(visible);
        }
    }
}

