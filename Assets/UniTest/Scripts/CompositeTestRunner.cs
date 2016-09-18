using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UniTest 
{
	public class CompositeTestRunner 
		: ITestRunner 
	{

		public CompositeTestRunner(string description) 
		{
			Tester = TestNode.Factory.CreateCompositionNode(description);	
		}

		public TestNode Tester 
		{
			get; 
			private set;
		}

		List<ITestRunner> _testers = new List<ITestRunner>();

		public void Run(Action<bool> on_determined,Action on_complete) 
		{
			run(on_determined,on_complete).Run();
		}

		IEnumerator run(Action<bool> on_determined,Action on_complete) 
		{
			on_determined = on_determined ?? delegate(bool obj) {};

			bool isSomethingFailed = false;
			foreach(var tester in _testers) 
			{
				if(tester != null) 
				{
					bool result = false;
					bool isDone = false;

					tester.Run(i_result=>
					{
						result = i_result;
					},()=>{
						isDone = true;
					});

					while(isDone == false) 
					{
						yield return null;
					}

					if(result == false) 
					{
						on_determined(false);
						isSomethingFailed = true;
					}
				}
			}

			if(isSomethingFailed == false)
			{
				on_determined(true);
			}

			on_complete();
		}

		public void Add(ITestRunner runner) 
		{
			_testers.Add(runner);
			Tester.Add(runner.Tester as TestElement);
		}

		public void AddRange(IEnumerable<ITestRunner> runners) 
		{
			if(runners.Any()) 
			{
				_testers.AddRange(runners);
				Tester.AddRange(runners.Select(runner=>runner.Tester as TestElement));
			}
		}
	}
}