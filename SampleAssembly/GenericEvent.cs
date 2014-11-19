using System;
using System.ComponentModel;

namespace SampleAssembly
{
	class GenericEvent
	{
		public event EventHandler<CancelEventArgs> MyEvent = Foo;

		private static void Foo(object a1, object a2) { }

		public void RaiseEvent()
		{
			MyEvent(this, new CancelEventArgs());
		}
	}
}