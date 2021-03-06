﻿using System;

namespace SampleAssembly
{
	class DelegateHandler
	{
		public event EventHandler MyEvent = delegate { };

		public void RaiseEvent()
		{
			MyEvent(this, EventArgs.Empty);
		}
	}
}