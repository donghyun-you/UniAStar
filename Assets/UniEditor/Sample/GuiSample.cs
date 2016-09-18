using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniTest 
{
	public class GuiSample
	{
		public enum TestType 
		{
			Test1,
			Test2,
			Test3,
			Test4,
			Test5,
			Test6,
			Test7,
			Test8,
			Test9,
			Test10,
			Test11,
			Test12,
			Test13,
			Test14,
			Test15,
			Test16,
			Test17,
			Test18,
			Test19,
			Test20,
			Test21,
			Test22
		}

		private GuiFolder _folder = null;
		private GuiEnumDropdown<TestType> _enumDropdown = null;
		private GuiDictionaryDropdown<int,string> _dicDropdown = null;

		private readonly Dictionary<int,string> _dropdownTable = new Dictionary<int,string>() 
		{
			{ 1, "test1" },
			{ 2, "test2" },
			{ 3, "test3" },
			{ 4, "test4" },
			{ 5, "test5" },
			{ 6, "test6" },
			{ 7, "test7" },
			{ 8, "test8" },
			{ 9, "test9" },
			{ 10, "test10" },
			{ 12, "test12" },
			{ 14, "test14" },
			{ 20, "test20" },	
		};

		public GuiSample() 
		{
			_folder = new GuiFolder();
			_enumDropdown = new GuiEnumDropdown<TestType>() 
			{
				SelectorOption 			= new GUILayoutOption[] { GUILayout.Width(100f) },
				CandidateListOption 	= new GUILayoutOption[] { GUILayout.Width(130f) },
				CandidateButtonOption 	= new GUILayoutOption[] { GUILayout.Width(100f) },
			};

			_dicDropdown = new GuiDictionaryDropdown<int, string>(10)
			{
				Candidates 				= _dropdownTable,
				SelectorOption 			= new GUILayoutOption[] { GUILayout.Width(100f) },
				CandidateListOption 	= new GUILayoutOption[] { GUILayout.Width(130f) },
				CandidateButtonOption 	= new GUILayoutOption[] { GUILayout.Width(100f) },
			};
		}

		public void OnGUI() 
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("test enum");
			_enumDropdown.Draw();
			GUILayout.EndHorizontal();

			if(_folder.Fold("test","test")) 
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				GUILayout.Label("testing");
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("test dic");
			_dicDropdown.Draw();
			GUILayout.EndHorizontal();

		}
	}
}
