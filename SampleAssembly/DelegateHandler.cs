using System;

namespace SampleAssembly
{
	class DelegateHandler
	{
		public event EventHandler MyEvent = delegate { };
	}
}