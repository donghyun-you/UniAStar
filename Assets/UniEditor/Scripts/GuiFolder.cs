using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UniTest 
{
	public class GuiFolder 
	{
		private HashSet<string> _unfolds = new HashSet<string>();
		private GUIStyle		_style = null;
		private GUIStyle 		Style 
		{
			get 
			{
				return _style ?? (_style = new GUIStyle(GUI.skin.label));
			}
		}

		public bool IsFold(string key) 
		{
			return _unfolds.Contains(key) == false;
		}

		public void SetFold(string key) 
		{
			_unfolds.Remove(key);
		}

		public void UnsetFold(string key) 
		{
			_unfolds.Add(key);
		}

		public bool Fold(string key, string show) 
		{
			if(key == null) 
			{
				throw new ArgumentNullException("show");
			}

			if(show == null) 
			{
				throw new ArgumentNullException("show");
			}

			Color colorBefore = GUI.color;

			if(this.IsFold(key)) 
			{
				GUI.color = Color.white;
				if(GUILayout.Button("▼ "+show,Style)) 
				{
					this.UnsetFold(key);
				}
			}
			else 
			{
				GUI.color = Color.grey;
				if(GUILayout.Button("▶︎ "+show,Style)) 
				{
					this.SetFold(key);
				}
			}

			GUI.color = colorBefore;

			return this.IsFold(key);
		}
	}
}