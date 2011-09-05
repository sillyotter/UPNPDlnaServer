using System;
using System.Collections.Generic;

namespace MediaServer.Utility
{
	static class EnumerableExtensions
	{
		//public static IEnumerable<TResult> ZipWith<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, 
		//                                                                     IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> func)
		//{
		//    if (first == null) throw new ArgumentNullException("first");
		//    if (second == null) throw new ArgumentNullException("second");

		//    var fe = first.GetEnumerator();
		//    var se = second.GetEnumerator();

		//    while (fe.MoveNext() && se.MoveNext())
		//    {
		//        yield return func(fe.Current, se.Current);
		//    }
		//}

		public static void Apply<T>(this IEnumerable<T> stuff, Action<T> activity)
		{
			foreach(var item in stuff)
			{
				activity(item);
			}
		}
	}
}