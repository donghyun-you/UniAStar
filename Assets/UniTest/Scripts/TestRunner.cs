using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace UniTest 
{
	public class TestRunner
		: ITestRunner
	{
		public TestNode Tester 
		{
			get; 
			private set; 
		}

		public Type RootType 
		{
			get 
			{
				return Tester.NodeType;
			}
		}

		public TestRunner(Type type) 
		{
			Tester = TestNode.Factory.Create(type,null);
		}

		public void Run(Action<bool> on_determined,Action on_complete)
		{
			Tester.Execute(result=>
			{
				TestLogger.Info(this,"test done");
				if(on_determined != null) on_determined(result);
			},()=>{
				if(on_complete != null) on_complete();
			});
		}

		public static IEnumerable<Type> GetRootStories() 
		{
			return Assembly.GetExecutingAssembly().GetTypes().Where(type=>type.IsNested == false && type.GetCustomAttributes(typeof(TestCaseAttribute),true).Any());
		}
	}
}