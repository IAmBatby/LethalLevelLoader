using System;

namespace LethalLevelLoader.General;
internal static class ArrayExtensions
{
    public static void AddItem<T>(ref T[] array, T item)
    {
        if (array == null)
        {
            array = new T[1] { item };
            return;
        }

        Array.Resize(ref array, array.Length + 1);
        array[array.Length - 1] = item;
    }
}
