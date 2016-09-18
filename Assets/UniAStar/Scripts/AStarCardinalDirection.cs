using System.Text;

namespace UniAStar 
{
	public struct AStarCardinalDirection 
	{
		public bool north;
		public bool south;
		public bool east;
		public bool west;

		public bool northEast;
		public bool northWest;
		public bool southEast;
		public bool southWest;

		public override string ToString ()
		{
			var builder = new StringBuilder();

			builder.Append("[AStarDirection:");
			if(this.north) 		builder.Append("+N");
			if(this.south) 		builder.Append("+S");
			if(this.east) 		builder.Append("+E");
			if(this.west) 		builder.Append("+W");
			if(this.northEast) 	builder.Append("+NE");
			if(this.northWest) 	builder.Append("+NW");
			if(this.southEast) 	builder.Append("+SE");
			if(this.southWest) 	builder.Append("+SW");
			builder.Append("]");

			return builder.ToString();
		}

		public static class Factory
		{
			public static AStarCardinalDirection CreateAll() 
			{
				return new AStarCardinalDirection 
				{
					north = true,
					south = true,
					east = true,
					west = true,
					northEast = true,
					northWest = true,
					southEast = true,
					southWest = true,
				};
			}

			public static AStarCardinalDirection CreateNESW() 
			{
				return new AStarCardinalDirection 
				{
					north = true,
					south = true,
					east = true,
					west = true,
					northEast = false,
					northWest = false,
					southEast = false,
					southWest = false,
				};
			}
		}
	}

}