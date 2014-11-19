using System;

namespace SampleAssembly
{
	class NoHandler
	{
		public event EventHandler MyEvent;

		public void RaiseEvent()
		{
			MyEvent(this, EventArgs.Empty);
		}
	}
}