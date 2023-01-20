namespace Studio.Framework.Pool
{
    public interface ISealedCreator<T>
    {
        U New<U>() where U : T;

        //U Copy<U>(U source) where U : T;

        void Reset(T target);
    }
}