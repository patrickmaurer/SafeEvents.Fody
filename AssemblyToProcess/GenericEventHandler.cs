using System;
using System.ComponentModel;

namespace AssemblyToProcess
{
	public class GenericEventHandler
	{
		public event EventHandler<CancelEventArgs> MyEvent;

		public void RaiseEvent()
		{
			MyEvent(this, new CancelEventArgs(false));
		}
	}
}