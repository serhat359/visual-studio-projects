using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ConsoleApplication1
{
    class Stack<T> : IEnumerable<T>
    {
        Node<T> top = null;

        public void push(T item)
        {
            Node<T> node = new Node<T>(item);

            node.next = top;
            top = node;
        }

        public T pop()
        {
            if (top != null)
            {
                Node<T> returnNode = top;
                top = top.next;
                return returnNode.item;
            }
            else
                throw new Exception("Stack is empty");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (top != null)
            {
                yield return top.item;
                top = top.next;
            }
        }

        private class Node<E>
        {
            public Node<E> next;
            public E item;

            public Node(E item)
            {
                this.item = item;
            }
        }
    }
}
