using UnityEngine;
using System.Collections;

namespace UniAStar 
{
	public struct AStarPosition
	{
		public int x;
		public int y;

		public AStarPosition(int x,int y) 
		{
			this.x = x;
			this.y = y;
		}

		public override string ToString ()
		{
			return string.Format("[AStarPosition:({0},{1})]",this.x,this.y);
		}
	}
}