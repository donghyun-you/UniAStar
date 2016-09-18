using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UniTest 
{
	public class GuiEnumDropdown<TEnum>
		where TEnum : struct, IConvertible
	{
		private GUIStyle		_style = null;
		private GUIStyle 		Style 
		{
			get 
			{
				return _style ?? (_style = new GUIStyle(GUI.skin.label));
			}
		}

		public GuiEnumDropdown() 
		{
			Selection = Candidates.First();
		}

		public GuiEnumDropdown(TEnum initial_value) 
		{
			Selection = initial_value;
		}

		public TEnum Selection 
		{
			get;
			set;
		}

		private TEnum[] _candidates = null;
		public TEnum[] Candidates
		{
			get 
			{
				return _candidates ?? (_candidates = (System.Enum.GetNames(typeof(TEnum))).Select(key=>(TEnum)Enum.Parse(typeof(TEnum),key)).ToArray());
			}
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


		public TEnum Draw(params GUILayoutOption[] options) 
		{
			GUILayout.BeginVertical();

			if(GUILayout.Button(Selection.ToString(),GUI.skin.box,SelectorOption))
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

				for(int i = 0; i < Candidates.Length; i++)
				{
					if(GUILayout.Button(this.Candidates[i].ToString(),GUI.skin.label,CandidateButtonOption))
					{
						_isSelcting = false;
						this.Selection = this.Candidates[i];
					}
				}

				GUILayout.EndScrollView();
			}

			GUILayout.EndVertical();

			return Selection;
		}
	}
}