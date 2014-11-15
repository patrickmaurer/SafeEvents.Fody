using System;

namespace AssemblyToProcess
{
	public class SimpleEventHandler
	{
		public event EventHandler MyEvent;

		public void RaiseMyEvent()
		{
			MyEvent(this, EventArgs.Empty);
		}
	}
}