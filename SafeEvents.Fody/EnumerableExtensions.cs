using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeEvents.Fody
{
	public static class EnumerableExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
		{
			@this.ToList().ForEach(action);
		}
	}
}