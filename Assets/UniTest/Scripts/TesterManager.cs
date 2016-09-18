using UnityEngine;
using System.Collections;
using System.Linq;

namespace UniTest 
{
	public class TesterManager
	{
		private static TesterManager _instance = null;
		public static TesterManager Instance 
		{
			get 
			{
				return _instance ?? (_instance = new TesterManager());
			}
		}

		public ITestRunner Tester 
		{
			get; 
			private set; 
		}

		public TesterManager() 
		{
			CompositeTestRunner tester = new CompositeTestRunner("All Tests");
			tester.AddRange(TestRunner.GetRootStories().Select(type=>new TestRunner(type) as ITestRunner));

			// NOTE(ruel): attach instantiated tester
			Tester = tester;
		}
	}
}