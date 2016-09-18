using UnityEngine;
using System.Collections;

namespace UniTest 
{
	public class TestScenarioAttribute 
		: TestCaseAttribute
	{
		private string _summary;
		public override string Summary 
		{
			get 
			{
				return _summary;	
			}
		}

		public TestScenarioAttribute(int Order,string Summary=null) : base(Order)
		{
			this._summary = Summary;
		}
	}
}