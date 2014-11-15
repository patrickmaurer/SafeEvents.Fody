using System;

namespace SampleAssembly
{
	class LambdaHandler
	{
		public event EventHandler MyEvent = (sender, e) => { };
	}
}