using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

namespace UniTest 
{
	public class EditorTesterWindow 
		: EditorWindow
	{
		[MenuItem ("Window/UniTest Runner")]
		private static void OpenWindow () 
		{
			EditorTesterWindow window = (EditorTesterWindow)EditorWindow.GetWindow (typeof (EditorTesterWindow));
			window.Show();
		}

		private TesterView _view = null;

		public void OnEnable() 
		{
			_view = new TesterView(TesterManager.Instance.Tester);
			EditorApplication.update += this.Repaint;
		}

		void OnDisable() 
		{
			EditorApplication.update -= this.Repaint;
			_view = null;
		}

		public void OnGUI() 
		{
			_view.OnGUI();
		}
	}
}