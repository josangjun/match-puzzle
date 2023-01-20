
namespace Studio.Framework
{
    public struct ScopeExit : System.IDisposable
    {
        System.Action action;
        
        public ScopeExit(System.Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            action?.Invoke();
        }
    }
}