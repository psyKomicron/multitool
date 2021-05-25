namespace Multitool.Optimisation
{
    public class CircularBag<T>
    {
        private object _lock = new object();
        private T[] buffer;
        private int capacity;
        private int head;

        public CircularBag(int capacity)
        {
            buffer = new T[capacity];
            this.capacity = capacity;
        }

        public int Length => buffer.Length;

        public bool Full => Length == capacity;

        public T this[int index]
        {
            get => buffer[index];
        }

        public void Add(T value)
        {
            lock (_lock)
            {
                buffer[head] = value;
                head = (head + 1) % capacity;
            }
        }
    }
}
