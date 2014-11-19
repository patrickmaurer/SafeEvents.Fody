using System;

namespace AssemblyToProcess
{
	public class TwoEvents
	{
		public event EventHandler MyEvent1;

		public event EventHandler MyEvent2;

		public void RaiseEvent()
		{
			MyEvent1(this, EventArgs.Empty);
			MyEvent2(this, EventArgs.Empty);
		}
	}
}