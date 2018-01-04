using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualConsole
{
    class StackPool<T>
    {
        private T[] values;
        private int size = 0;
        private int lastValueIndex = 0; // Points to empty
        private int capacity { get { return values.Length; } }

        public int Count { get { return size; } }

        public StackPool(int capacity)
        {
            values = new T[capacity];
        }

        public void Push(T elem)
        {
            values[lastValueIndex] = elem;
            lastValueIndex = (lastValueIndex + 1) % capacity;

            size++;
            if (size > capacity)
                size = capacity;
        }

        public T Peek()
        {
            if (size == 0)
                throw new InvalidOperationException("Stack is empty");

            int index = mod(lastValueIndex - 1, capacity);
            return values[index];
        }

        public T Pop()
        {
            if (size == 0)
                throw new InvalidOperationException("Stack is empty");

            lastValueIndex = mod(lastValueIndex - 1, capacity);
            size--;

            return values[lastValueIndex];
        }

        public IEnumerable<T> LastToFirst()
        {
            for (int i = 0; i < size; i++)
            {
                int index = lastValueIndex - 1 - i;
                index = mod(index, capacity);
                yield return values[index];
            }
        }

        public IEnumerable<T> FirstToLast()
        {
            int firstIndex = mod(lastValueIndex - size, capacity);

            for (int i = 0; i < size; i++)
            {
                int index = (firstIndex + i) % capacity;
                yield return values[index];
            }
        }

        int mod(int val, int m)
        {
            return (val % m + m) % m;
        }
    }

}
