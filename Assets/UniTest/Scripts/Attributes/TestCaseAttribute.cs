using System;

namespace UniTest 
{
	public abstract class TestCaseAttribute 
		: Attribute 
	{
		public int Order 
		{
			get; 
			private set;
		}

		public abstract string Summary 
		{
			get; 
		}

		protected TestCaseAttribute(int Order) 
		{
			this.Order = Order;
		}
	}
}
