using System.Collections;

namespace Studio.Game
{
    public interface ICommand
    {
        void Run();
        bool IsRunning() => false;
    }
}