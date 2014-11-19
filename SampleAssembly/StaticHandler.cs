using System;

namespace SampleAssembly
{
	class StaticHandler
	{
		public event EventHandler MyEvent = Foo;

		private static void Foo(object sender, EventArgs e) { }

		public void RaiseEvent()
		{
			MyEvent(this, EventArgs.Empty);
		}
	}
}