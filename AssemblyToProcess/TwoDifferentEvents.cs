using System;
using System.Data;

namespace AssemblyToProcess
{
	public delegate void UnusualEventHandler(object one, string two, DataTable three);

	public class TwoDifferentEvents
	{
		public event EventHandler NormalEvent;

		public event UnusualEventHandler UnusualEvent;

		public void RaiseEvent()
		{
			NormalEvent(this, EventArgs.Empty);
			UnusualEvent(1, "two", new DataTable("T_THREE"));
		}
	}
}