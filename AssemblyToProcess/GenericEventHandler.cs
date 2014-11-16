using System;
using System.ComponentModel;

namespace AssemblyToProcess
{
	public class GenericEventHandler
	{
		public event EventHandler<CancelEventArgs> MyEvent;

		public void RaiseMyEvent()
		{
			MyEvent(this, new CancelEventArgs(false));
		}
	}
}