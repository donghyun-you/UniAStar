using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UniAStar 
{
	public class AStarMap
	{
		public AStarMap(int width,int height) 
		{
			initialize(width,height,AStarCardinalDirection.Factory.CreateAll(),AStarPath.FindingType.kDiagonal);
		}

		public AStarMap(int width,int height, AStarPath.FindingType finding_type) 
		{
			initialize(width,height,AStarCardinalDirection.Factory.CreateAll(),finding_type);
		}

		public AStarMap(int width,int height, AStarCardinalDirection allow_direction) 
		{
			initialize(width,height,allow_direction,AStarPath.FindingType.kDiagonal);
		}

		public AStarMap(int width,int height, AStarCardinalDirection allow_direction, AStarPath.FindingType finding_type) 
		{
			initialize(width,height,allow_direction,finding_type);
		}

		protected void initialize(int width,int height, AStarCardinalDirection allow_direction, AStarPath.FindingType finding_type) 
		{
			if(width <= 0) 
			{
				throw new ArgumentException("width <= 0");
			}

			if(height <= 0) 
			{
				throw new ArgumentException("height <= 0");
			}

			this.Map 				= new Spot[width,height];
			this.Width 				= width;
			this.Height 			= height;
			this.AllowDirection 	= allow_direction;
			this.Locker				= new object();
			this.FindingType		= finding_type;

			for(int x=0;x<width;x++) 
			{
				for(int y=0;y<height;y++) 
				{
					Map[x,y] = new Spot(this,new AStarPosition() { x = x, y = y });
				}
			}
		}

		public class Spot
		{
			public Spot(AStarMap map,AStarPosition position) 
			{
				this.Map = map;
				this.Position = position;
			}

			private bool _isAvailable = false;
			public bool IsAvailable 
			{
				get 
				{
					return _isAvailable; 
				}
				set 
				{
					if(_isAvailable != value) 
					{
						_isAvailable = value; 
						Map.updateAvailability(this.Position,value);
						if(this.Position.x == 1 && this.Position.y == 4) 
						{
							Debug.Log("changed to "+value);
						}
					}
				}
			}

			public AStarPosition Position 
			{
				get;
				private set;
			}

			public AStarMap Map 
			{
				get;
				private set;
			}

			public override string ToString ()
			{
				return string.Format ("[Spot: IsAvailable={0}, Position={1}]", IsAvailable, Position.ToString());
			}
		}

		public object Locker 
		{
			get;
			private set;
		}

		public Spot[,] Map
		{
			get;
			private set;
		}

		public int Width 
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public AStarCardinalDirection AllowDirection 
		{
			get;
			private set;
		}

		public AStarPath.FindingType FindingType 
		{
			get;
			private set;
		}

		protected void updateAvailability(AStarPosition position,bool is_available) 
		{
			if(this.OnUpdateAvailability != null) 
			{
				this.OnUpdateAvailability.Invoke(position,is_available);
			}
		}

		public delegate void UpdateAvailabilityEvent(AStarPosition position,bool is_available);
		public event UpdateAvailabilityEvent OnUpdateAvailability;

		private AStarPath.Factory pathFactory = new AStarPath.Factory();

		public AStarPath FindPath(AStarPosition start_at,AStarPosition destinate_at) 
		{
			return pathFactory.Create(this,start_at,destinate_at,this.FindingType);
		}

		public void FindPathAsync(AStarPosition start_at,AStarPosition destinate_at,Action<AStarPath> on_complete) 
		{
			pathFactory.CreateAsync(this,start_at,destinate_at,on_complete,this.FindingType);
		}

		public override string ToString ()
		{
			var builder = new System.Text.StringBuilder();

			for(int y=0;y<Height;y++) 
			{ 
				for(int x=0;x<Width;x++) 
				{
					builder.Append(this.Map[x,y].IsAvailable ? "O":"X");
					//builder.Append(string.Format("{0}/{1}={2},",x,y,this.Map[x,y].Position.ToString()));
				}
				builder.AppendLine("");
			}

			return string.Format ("[AStarMap: Locker={0}, Map={1}, Width={2}, Height={3}, AllowDirection={4}, FindingType={5}]\n{6}", Locker, Map, Width, Height, AllowDirection, FindingType,builder.ToString());
		}
	}
}