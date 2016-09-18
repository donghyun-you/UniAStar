using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UniTest 
{
	[InitializeOnLoadAttribute]
	public static class EditorUpdateWorker
	{
		static EditorUpdateWorker() 
		{
			EditorApplication.update += TestCoroutineRunner.OnEditorUpdate;
		}
	}
}