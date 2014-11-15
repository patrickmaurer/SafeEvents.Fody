using System;

namespace SampleAssembly
{
	public delegate string IsThisPossible(object sender, EventArgs e);

	public class NonVoid
	{
		public event IsThisPossible MyEvent = delegate { return "Hello"; };

		public void RaiseEvent()
		{
			var result = MyEvent(this, EventArgs.Empty);
		}
	}
}