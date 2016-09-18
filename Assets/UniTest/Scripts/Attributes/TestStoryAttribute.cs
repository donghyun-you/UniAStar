using UnityEngine;
using System;
using System.Collections;

namespace UniTest 
{
	public class TestStoryAttribute 
		: TestCaseAttribute 
	{
		public string AsA 
		{
			get;
			private set;
		}

		public string IWant
		{
			get;
			private set;
		}

		public string SoThat
		{
			get;
			private set;
		}

		public override string Summary 
		{
			get 
			{
				string result = "";

				if (string.IsNullOrEmpty(this.IWant) == false)
				{
					result += "I want "+this.IWant;
				}

				if (string.IsNullOrEmpty(this.SoThat) == false) 
				{
					if(string.IsNullOrEmpty(result)) 
					{
						result += "It should be "+this.SoThat;
					}
					else
					{
						result += " so that "+this.SoThat;
					}
				}

				if (string.IsNullOrEmpty(this.AsA) == false) 
				{
					if(string.IsNullOrEmpty(result)) 
					{
						result += "Something undefined tried as a "+this.AsA;
					}
					else
					{
						result += " as a "+this.AsA;
					}
				}

				return result;
			}
		}

		public TestStoryAttribute(int Order,string AsA=null,string IWant=null,string SoThat=null) : base(Order)
		{
			this.AsA 		= AsA;
			this.IWant		= IWant;
			this.SoThat		= SoThat;
		}
	}
}