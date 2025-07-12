using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListUtility
{
    public static void RandomList<T>(this List<T> list)
    {
        list.Sort((a, b) => Random.Range(-1, 1));
    }
}
