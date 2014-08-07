using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Assisticant
{
    static class WeakArray
    {
        public static void Add(ref WeakReference[] array, object item)
        {
            Debug.Assert(item != null);
            if (array == null)
                array = new WeakReference[] { new WeakReference(item) };
            else
            {
                int count = GetCount(ref array);
                if (count < array.Length)
                    array[count] = new WeakReference(item);
                else
                {
                    var enlarged = new WeakReference[2 * array.Length];
                    Array.Copy(array, enlarged, array.Length);
                    enlarged[array.Length] = new WeakReference(item);
                    array = enlarged;
                }
            }
        }

        public static void Remove(ref WeakReference[] array, object item)
        {
            if (array != null)
            {
                int count = 0;
                for (int i = 0; i < array.Length; ++i)
                {
                    if (array[i] == null)
                        break;
                    var target = array[i].Target;
                    if (target != null && target != item)
                    {
                        if (i > count)
                        {
                            array[count] = array[i];
                            array[i] = null;
                        }
                        ++count;
                    }
                    else
                        array[i] = null;
                }
                if (count == 0)
                    array = null;
            }
        }

        public static bool Contains(ref WeakReference[] array, object item)
        {
            Debug.Assert(item != null);
            if (array == null)
                return false;
            else
            {
                foreach (var reference in array)
                {
                    if (reference == null)
                        break;
                    if (reference.Target == item)
                        return true;
                }
                return false;
            }
        }

        public static int GetCount(ref WeakReference[] array)
        {
            if (array == null)
                return 0;
            int count = 0;
            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i] == null)
                    break;
                if (array[i].IsAlive)
                {
                    if (i > count)
                    {
                        array[count] = array[i];
                        array[i] = null;
                    }
                    ++count;
                }
                else
                    array[i] = null;
            }
            if (count == 0)
                array = null;
            return count;
        }

        public static IEnumerable<T> Enumerate<T>(WeakReference[] array)
            where T : class
        {
            if (array == null)
                yield break;
            foreach (var reference in array)
            {
                if (reference == null)
                    break;
                var target = reference.Target as T;
                if (target != null)
                    yield return target;
            }
        }
    }
}
