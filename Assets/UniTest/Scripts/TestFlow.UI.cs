using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace UniTest 
{
	public partial class TestFlow
	{
		#region subject 
		public TestFlow Selectable<TSelectable>(string selectableName) 
			where TSelectable : Selectable
		{
			var selectableGameObject = UnityEngine.GameObject.Find(selectableName);
			if(selectableGameObject == null) throw new InvalidOperationException("Unable to find \""+(selectableName ?? "")+"\"");

			var selectable = selectableGameObject.GetComponent<TSelectable>();
			if(selectableGameObject == null) throw new InvalidOperationException("Unable to find \""+(selectableName ?? "")+"\"."+typeof(TSelectable).Name+"(Component)");

			return new TestFlow(this,getMethodName(),toStringOrNull(selectable),TestReportType.kPass,selectable);
		}
		#endregion

		#region conclude
		public TestFlow Click(string conclusion=null) 
		{
			if(this.Subject is Button) 
			{
				var button = this.Subject as Button;
				button.onClick.Invoke();
			}
			else 
			{
				throw new InvalidOperationException("[Click] must chained with Button");
			}

			return conclude(conclusion,null);
		}

		public TestFlow SetText(string text,string conclusion=null) 
		{
			if(this.Subject is InputField) 
			{
				var inputfield = this.Subject as InputField;
				inputfield.text = text;
			}
			else 
			{
				throw new InvalidOperationException("[SetText] must chained with InputField");
			}

			return conclude(conclusion,null);
		}
		#endregion
	}
}