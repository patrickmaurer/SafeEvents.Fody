using System;

namespace AssemblyToProcess
{
	public delegate void MyEventHandler<in T>(object sender, T e);

	public class ValueTypeGenericEventHandler
	{
		public event MyEventHandler<int> MyIntegerEvent;
		public event MyEventHandler<double> MyDoubleEvent;
		public event MyEventHandler<DateTime> MyDateTimeEvent;

		public void RaiseEvent()
		{
			MyIntegerEvent(this, 1);
			MyDoubleEvent(this, 2.0);
			MyDateTimeEvent(this, DateTime.Now);
		}
	}
}