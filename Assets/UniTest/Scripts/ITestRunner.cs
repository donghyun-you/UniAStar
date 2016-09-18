using System;

namespace UniTest 
{
	public interface ITestRunner
	{
		TestNode Tester 
		{
			get; 
		}
		void Run(Action<bool> on_determined,Action on_complete);
	}
}