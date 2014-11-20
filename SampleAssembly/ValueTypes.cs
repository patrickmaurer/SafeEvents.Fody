namespace SampleAssembly
{
	public delegate void ValueTypeEventHandler(object sender, int data);

	class ValueTypes
	{
		public event ValueTypeEventHandler MyEvent = Foo;

		private static void Foo(object sender, int parameter) { }

		public void RaiseEvent()
		{
			MyEvent(this, 42);
		}
	}
}