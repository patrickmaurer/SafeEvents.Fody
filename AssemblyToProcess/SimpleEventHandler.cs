using System;

namespace AssemblyToProcess
{
	public class SimpleEventHandler
	{
		public event EventHandler MyEvent;

		public void RaiseEvent()
		{
			MyEvent(this, EventArgs.Empty);
		}
	}
}