using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class Tools
{
	public static T PopFirst<T>(this List<T> list)
	{
		if (list == null || list.Count == 0) return default;
		var item = list[0];
		list.RemoveAt(0);
		return item;
	}

	public static T PopLast<T>(this List<T> list)
	{
		if (list == null || list.Count == 0) return default;
		var end = list.Count - 1;
		var item = list[end];
		list.RemoveAt(end);
		return item;
	}

	public static T RandomElement<T>(this List<T> src)
	{
		if (src == null || src.Count == 0) return default;
		var idx = Random.Range(0, src.Count);
		return src[idx];
	}

	public static void RemoveByValue<T>(this List<T> src, T Value)
	{
		var i = 0;
		while (i < src.Count)
			if (src[i].Equals(Value))
				src.RemoveAt(i);
			else
				i++;
	}

	public static void RemoveByValue<T, T1>(this Dictionary<T, T1> src, T1 Value)
	{
		foreach (var item in src.Where(kvp => kvp.Value.Equals(Value)).ToList())
			_ = src.Remove(item.Key);
	}

	public static void RemoveByValue<T, T1>(this ConcurrentDictionary<T, T1> src, T1 Value)
	{
		foreach (var item in src.Where(kvp => kvp.Value.Equals(Value)).ToList())
			_ = src.TryRemove(item.Key, out var _);
	}

	public static void Deconstruct<T>(this IList<T> list, out T first, out IList<T> rest)
	{
		first = list.Count > 0 ? list[0] : default;
		rest = list.Skip(1).ToList();
	}

	public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out IList<T> rest)
	{
		first = list.Count > 0 ? list[0] : default;
		second = list.Count > 1 ? list[1] : default;
		rest = list.Skip(2).ToList();
	}
}
