using UnityEngine;
using System.Collections;
using System.Linq;

namespace UniTest 
{
	public class RuntimeTesterView 
		: MonoBehaviour 
	{
		private TesterView _view = null;

		public void Awake() 
		{
			_view = new TesterView(TesterManager.Instance.Tester);
		}

		public void OnGUI() 
		{
			_view.OnGUI();
		}
	}
}