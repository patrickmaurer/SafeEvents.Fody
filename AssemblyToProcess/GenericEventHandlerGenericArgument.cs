using System;

namespace AssemblyToProcess
{
	public class EventArgs<T> : EventArgs
	{
		public EventArgs(T data)
		{
			Data = data;
		}

		public T Data { get; private set; }
	}

	public class GenericEventHandlerGenericArgument
	{
		public event EventHandler<EventArgs<string>> MyEvent;

		public void RaiseMyEvent()
		{
			MyEvent(this, new EventArgs<string>("Hello"));
		}
	}
}