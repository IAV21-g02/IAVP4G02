using System;
using System.Collections.Generic;

namespace es.ucm.fdi.iav.rts.g02
{
    /// <summary>
    /// A binary heap, useful for sorting data and priority queues.
    /// </summary>
    /// <typeparam name="T"><![CDATA[IComparable<T> type of item in the heap]]>.</typeparam>
    public class Priority_Queue<T> : ICollection<T> where T : IComparable<T>
    {
        // Constantes
        private const int DEFAULT_SIZE = 4;

        //No constantes
        private T[] list = new T[DEFAULT_SIZE];
        private int it = 0;
        private int capacity = DEFAULT_SIZE;
        private bool ordenado;

        /// <summary>
        /// Devuelve el tamaño de la lista
        /// </summary>
        public int Count
        {
            get { return it; }
        }

        /// <summary>
        /// Devuelve o asigna la capacidad de la pila
        /// </summary>
        public int SetCapacity
        {
            get { return capacity; }
            set
            {
                int previousCapacity = capacity;
                capacity = Math.Max(value, it);
                if (capacity != previousCapacity)
                {
                    T[] temp = new T[capacity];
                    Array.Copy(list, temp, it);
                    list = temp;
                }
            }
        }

        /// <summary>
        /// Crea una cola vacía
        /// </summary>
        public Priority_Queue()
        {
        }

        /// <summary>
        /// Crea una cola a partir de un array
        /// </summary>
        private Priority_Queue(T[] array, int count)
        {
            SetCapacity = count;
            it = count;
            Array.Copy(array, list, count);
        }

        /// <summary>
        /// Devuelve el valor más prioritario
        /// </summary>
        public T Top()
        {
            return list[0];
        }

        /// <summary>
        /// Vacía la cola
        /// </summary>
        public void Clear()
        {
            this.it = 0;
            list = new T[capacity];
        }

        /// <summary>
        /// Adds a key and value to the heap.
        /// </summary>
        /// <param name="e">The item to add to the heap.</param>
        public void Add(T e)
        {
            if (it == capacity) SetCapacity *= 2;

            list[it] = e;
            Flotar();
            it++;
        }

        /// <summary>
        /// Elmina y devuelve el elemento más prioritario
        /// </summary>
        public T Remove()
        {
            if (this.it == 0) throw new Exception("La pila está vacía. Imposible eliminar elemento.");

            T v = list[0];
            it--;
            list[0] = list[it];
            list[it] = default(T);
            Hundir();
            return v;
        }

        //Aumenta la prioridad del nuevo elemento
        //si es necesario
        private void Flotar()
        {
            ordenado = false;
            int p = it;
            T item = list[p];
            int par = Parent(p);
            while (par > -1 && item.CompareTo(list[par]) < 0)
            {
                list[p] = list[par]; //Swap nodes
                p = par;
                par = Parent(p);
            }
            list[p] = item;
        }

        //Reduce la prioridad del nuevo elemento
        //si es necesario
        private void Hundir()
        {
            ordenado = false;
            int n;
            int p = 0;
            T item = list[p];
            while (true)
            {
                int ch1 = Child1(p);
                if (ch1 >= it) break;
                int ch2 = Child2(p);
                if (ch2 >= it)
                {
                    n = ch1;
                }
                else
                {
                    n = list[ch1].CompareTo(list[ch2]) < 0 ? ch1 : ch2;
                }
                if (item.CompareTo(list[n]) > 0)
                {
                    list[p] = list[n]; //Swap nodes
                    p = n;
                }
                else
                {
                    break;
                }
            }
            list[p] = item;
        }
        private void EnsureSort()
        {
            if (ordenado) return;
            Array.Sort(list, 0, it);
            ordenado = true;
        }

        private static int Parent(int index)
        {
            return (index - 1) >> 1;
        }

        private static int Child1(int index)
        {
            return (index << 1) + 1;
        }

        private static int Child2(int index)
        {
            return (index << 1) + 2;
        }

        /// <summary>
        /// Constructor por copia
        /// </summary>
        public Priority_Queue<T> Copy()
        {
            return new Priority_Queue<T>(list, it);
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnsureSort();
            for (int i = 0; i < it; i++)
            {
                yield return list[i];
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Comrpueba si existe el elemento en la pila
        /// </summary>
        public bool Contains(T e)
        {
            foreach (T elem in list)
            {
                if (ReferenceEquals(elem, e))
                    return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            EnsureSort();
            Array.Copy(list, array, it);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T e)
        {
            EnsureSort();
            int i = Array.BinarySearch<T>(list, 0, it, e);
            if (i < 0) return false;
            Array.Copy(list, i + 1, list, i, it - i);
            list[it] = default(T);
            it--;
            return true;
        }

        public bool Empty()
        {
            return it <= 0;
        }
    }
}