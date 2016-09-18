using UnityEngine;
using System;
using System.Collections;

namespace UniTest 
{
	public class TestSceneStoryAttribute 
		: TestCaseAttribute 
	{
		public string SceneAt
		{
			get;
			private set;
		}

		public override string Summary 
		{
			get 
			{
				string result = "";

				if (string.IsNullOrEmpty(this.SceneAt) == false)
				{
					result += "Tested at "+(this.SceneAt ?? "?");
				}

				return result;
			}
		}

		public TestSceneStoryAttribute(int Order,string SceneAt=null) : base(Order)
		{
			this.SceneAt = SceneAt;
		}
	}
}