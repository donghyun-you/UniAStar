using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UniTest 
{
	public class GuiDictionaryDropdown<TKey,TValue>
		where TKey : struct
	{
		private GUIStyle		_style = null;
		private GUIStyle 		Style 
		{
			get 
			{
				return _style ?? (_style = new GUIStyle(GUI.skin.label));
			}
		}

		public GuiDictionaryDropdown() 
		{
			if(Candidates != null) 
			{
				SelectionKey = Candidates.First().Key;
			}
		}

		public GuiDictionaryDropdown(TKey initial_value) 
		{
			SelectionKey = initial_value;
		}

		public TKey SelectionKey 
		{
			get;
			set;
		}

		public TValue SelectionValue
		{
			get 
			{
				return this.Candidates[this.SelectionKey];
			}
		}

		public Dictionary<TKey,TValue> Candidates
		{
			get;
			set;
		}

		private bool 	_isSelcting = false;
		private Vector2 _scrollViewPosition = new Vector2();

		public GUILayoutOption[] SelectorOption
		{
			get; 
			set;
		}

		public GUILayoutOption[] CandidateListOption
		{
			get; 
			set;
		}

		public GUILayoutOption[] CandidateButtonOption
		{
			get; 
			set;
		}

		public TKey Draw(params GUILayoutOption[] options) 
		{
			GUILayout.BeginVertical();

			if(GUILayout.Button(SelectionValue.ToString(),GUI.skin.box,SelectorOption))
			{
				if(!_isSelcting)
				{
					_isSelcting = true;
				}
				else
				{
					_isSelcting = false;
				}
			}

			if(_isSelcting)
			{
				_scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition,GUI.skin.box,CandidateListOption);

				foreach(var candidate in this.Candidates)
				{
					if(GUILayout.Button(candidate.Value.ToString(),GUI.skin.label,CandidateButtonOption))
					{
						_isSelcting = false;
						this.SelectionKey = candidate.Key;
					}
				}

				GUILayout.EndScrollView();
			}

			GUILayout.EndVertical();

			return SelectionKey;
		}
	}
}