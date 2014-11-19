using System;

namespace SampleAssembly
{
	class LambdaHandler
	{
		public event EventHandler MyEvent = (sender, e) => { };

		public void RaiseEvent()
		{
			MyEvent(this, EventArgs.Empty);
		}
	}
}