using System.Collections.Generic;

namespace Studio.Framework.Pool
{

    public class SealedPool<T>
    {

        private ISealedCreator<T> _creator;

        private IDictionary<System.Type, Stack<T>> _stacks = new SortedList<System.Type, Stack<T>>();

        public SealedPool(ISealedCreator<T> creator)
        {
            _creator = creator;
        }

        private Stack<T> GetStack(System.Type type)
        {
            Stack<T> stack;
            if (!_stacks.TryGetValue(type, out stack))
                stack = _stacks[type] = new Stack<T>();
            return stack;
        }

        public U Rent<U>() where U : T
        {
            var stack = GetStack(typeof(U));
            var item = (U)stack.Pop();
            if (item != null) return item;
            if (_creator != null) return _creator.New<U>();
            return default;
        }

        public void Return(T item)
        {
            _creator.Reset(item);
            var stack = GetStack(item.GetType());
            stack.Push(item);
        }

        public void Clear()
        {
            _stacks.Clear();
        }
    }
}