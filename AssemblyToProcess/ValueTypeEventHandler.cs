using System;

namespace AssemblyToProcess
{
	public delegate void MyEventHandler(object sender, int data);

	public class ValueTypeEventHandler
	{
		public event EventHandler MyEvent;
		public event MyEventHandler MyValueEvent;

		public void RaiseEvent()
		{
			MyEvent(this, EventArgs.Empty);
			MyValueEvent(this, 1);
		}
	}
}