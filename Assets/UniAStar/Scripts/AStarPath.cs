//#define CREATE_LIST_ON_UNREACAHBLE
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UniAStar 
{
	/// <summary>
	/// Create AStarPath from AStarMap.Spot Availability
	/// </summary>
	public class AStarPath
		: Bases.DisposableBase, IEnumerable<AStarPath.Node>
	{
		/// <summary>
		/// AStarPath must constructed from AStarPath.Factory.Create
		/// </summary>
		protected AStarPath(AStarMap map)
		{
			Map = map;
			Map.OnUpdateAvailability += OnAvailabilityChanged;
		}

		protected override void onDisposed ()
		{
			Map.OnUpdateAvailability -= OnAvailabilityChanged;
		}

		protected void OnAvailabilityChanged(AStarPosition position, bool is_available) 
		{
			if(this.IsValid) 
			{
				var foundNode = this.Find(node=>node.x == position.x && node.y == position.y);
				if(foundNode != null)
				{
					this.InvalidatedNode = foundNode;
				}
			}
		}

		public bool ContainsNode(Func<Node,bool> determiner) 
		{
			return Find(determiner) != null;
		}

		public Node Find(Func<Node,bool> determiner) 
		{
			if(this.Begin == null) 
			{
				return null;
			}

			var current = this.Begin;

			do
			{
				if(determiner(current)) 
				{
					return current;	
				}
			} while( (current = current.next) != null);

			return null;
		}

		public bool IsReachable 
		{
			get 
			{
				return End != null && End.x == ExpectedEnd.x && End.y == ExpectedEnd.y;
			}
		}

		public bool IsValid 
		{
			get { return InvalidatedNode == null; }
		}

		public Node InvalidatedNode 
		{
			get; 
			protected set;
		}

		public class Node 
		{
			public int x;
			public int y;

			public Node next;

			public override string ToString ()
			{
				return string.Format("[AStarPath.Node:({0},{1})]",this.x,this.y);
			}
		}

		public enum FindingType 
		{
			kDiagonal,
			kDiagonalFree,
			kEuclidean,
			kEuclideanFree,
			kManhattan,
		}

		public AStarMap Map 
		{
			get;
			private set;
		}

		public Node Begin 
		{
			get;
			private set;
		}

		public Node End
		{
			get;
			private set;
		}

		public AStarPosition ExpectedBegin
		{
			get;
			private set;
		}

		public AStarPosition ExpectedEnd
		{
			get;
			private set;
		}

		private IEnumerable<Node> nodeIteration() 
		{
			Node focus = Begin;

			while(focus != null) 
			{
				yield return focus;
				focus = focus.next;
			}
		}

		public IEnumerator<Node> GetEnumerator()
		{
			return nodeIteration().GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString ()
		{
			return string.Format ("[AStarPath: IsReachable={0}, IsValid={1}, Path={2}, StartAt={3}, DestinationAt={4}]", IsReachable, IsValid, string.Join("->",this.Select(spot=>string.Format("[{0}x{1}:{2}]",spot.x,spot.y,(this.Map.Spots[spot.x,spot.y].IsAvailable?"O":"X"))).ToArray()), Begin, End);
		}

		public class Factory 
		{
			public void CreateAsync(AStarMap map,AStarPosition start_at,AStarPosition destination_at,Action<AStarPath> on_complete,FindingType type) 
			{
				var mainThreadDispatcher = Threading.AStarMainThreadDispatcher.Instance;
				Threading.ThreadPool.Run(()=>
				{
					var path = this.Create(map,start_at,destination_at,type);
					mainThreadDispatcher.Run(()=>
					{
						on_complete(path);	
					},Debug.LogException);

				},Debug.LogException);
			}

			public AStarPath Create(AStarMap map,AStarPosition start_at,AStarPosition destination_at,FindingType type) 
			{
				return find(map,start_at,destination_at,type);
			}

			#region resource pool
			private Utils.Pool<List<int>> 				_listPool 			= new UniAStar.Utils.Pool<List<int>>();
			private Utils.Pool<HashSet<int>>			_hashSetPool		= new UniAStar.Utils.Pool<HashSet<int>>();
			private Utils.Pool<List<InnerNode>> 		_nodeListPool 		= new UniAStar.Utils.Pool<List<InnerNode>>();
			#endregion

			// NOTE(donghyun-you): AStarPath generates each path calculations. avoding cloning a lot of ILs, make it in the factory.

			#region magnitude
			private double diagonal(InnerNode start, InnerNode end) 
			{
				return Math.Max(Math.Abs(start.x - end.x), Math.Abs(start.y - end.y));
			}

			private double euclidean(InnerNode start, InnerNode end) 
			{
				int x = start.x - end.x;
				int y = start.y - end.y;

				return Math.Sqrt(x * x + y * y);
			}

			private double manhattan(InnerNode start, InnerNode end) 
			{
				return Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
			}
			#endregion

			#region calculation

			protected struct InnerNode 
			{
				public int x;
				public int y;

				public double f;
				public double g;
				public int v;

				public int previous;

				public InnerNode(int x,int y) 
				{
					this.x = x;
					this.y = y;
					this.f = 0;
					this.g = 0;
					this.v = 0;
					this.previous = -1;
				}

				public InnerNode(int x,int y,double f,double g,int v,int previous) 
				{
					this.x = x;
					this.y = y;
					this.f = f;
					this.g = g;
					this.v = v;
					this.previous = previous;
				}

				public InnerNode SetF(double f) 
				{
					return new InnerNode(this.x,this.y,f,this.g,this.v,this.previous);
				}

				public InnerNode SetG(double g) 
				{
					return new InnerNode(this.x,this.y,this.f,g,this.v,this.previous);
				}

				public InnerNode SetV(int v) 
				{
					return new InnerNode(this.x,this.y,this.f,this.g,v,this.previous);
				}

				public InnerNode SetPrevious(int previous) 
				{
					return new InnerNode(this.x,this.y,this.f,this.g,this.v,previous);
				}
			}

			private AStarPath find(AStarMap map,AStarPosition start_at,AStarPosition destination_at,FindingType type=FindingType.kDiagonal) 
			{
				int cols 	= map.Width;
				int rows 	= map.Height;
				int limit 	= cols * rows;
				int length 	= 1;

				List<InnerNode> 		opensetBucket 		= _nodeListPool.Alloc() ?? new List<InnerNode>();
				List<int>				opensetEnteries 	= _listPool.Alloc() ?? new List<int>();
				HashSet<int>			used				= _hashSetPool.Alloc() ?? new HashSet<int>();

				opensetBucket.Add(new InnerNode(start_at.x,start_at.y).SetV(start_at.x+start_at.y*cols));
				opensetEnteries.Add(0);

				double 	max;
				int 	min;

				InnerNode current;
				InnerNode end = new InnerNode(destination_at.x,destination_at.y).SetV(destination_at.x+destination_at.y*cols);

				Node result_begin = null;
				Node result_end = null;

				do 
				{
					max = limit;
					min = 0;

					for (int i = 0; i < length; i++) 
					{
						var entryIndex = opensetEnteries[i];
						var f = opensetBucket[entryIndex].f;
						if (f < max) 
						{
							max = f;
							min = i;
						}
					}

					int current_handle = opensetEnteries[min];
					current = opensetBucket[current_handle];
					opensetEnteries.RemoveAt(min);

					if (current.v != end.v) 
					{
						--length;
						List<InnerNode> next = successors(current.x, current.y, map, rows, cols, type);
						double 	distanceS;
						double 	distanceE;

						#if VERBOSE_LOG
						List<InnerNode> adjusts = new List<InnerNode>();
						Logger.Verbose(this,"finding next from "+current.x+"/"+current.y+" with "+map.FindingType.ToString());
						#endif

						for (int i=0,j=next.Count; i<j;++i)
						{
							InnerNode adjust = next[i].SetF(0).SetG(0).SetV(next[i].x + next[i].y * cols).SetPrevious(current_handle);

							if (!used.Contains(adjust.v)) 
							{
								if (type == FindingType.kDiagonalFree || type == FindingType.kDiagonal) 
								{
									distanceS = diagonal(adjust, current);
									distanceE = diagonal(adjust, end);
								}
								else if (type == FindingType.kEuclideanFree || type == FindingType.kEuclidean) 
								{
									distanceS = euclidean(adjust, current);
									distanceE = euclidean(adjust, end);
								}
								else 
								{
									distanceS = manhattan(adjust, current);
									distanceE = manhattan(adjust, end);
								}

								adjust = adjust.SetG(current.g + distanceS);
								adjust = adjust.SetF(adjust.g + distanceE);

								opensetBucket.Add(adjust);
								opensetEnteries.Add(opensetBucket.Count-1);
								used.Add(adjust.v);
								length++;
							}

							#if VERBOSE_LOG
							adjusts.Add(adjust);
							#endif

						}

						#if VERBOSE_LOG
						Logger.Verbose(this,string.Join("->",adjusts.Select(entry=>string.Format("[x:{0},y:{1},f:{2},g:{3}]",entry.x,entry.y,entry.f,entry.g)).ToArray()));
						#endif

						if(next != null) 
						{
							next.Clear();
							_nodeListPool.Free(next);
						}

					}
					else 
					{
						break;
					}

				} while (length != 0);

				#if CREATE_LIST_ON_UNREACAHBLE
				if (current.v == end.v) 
				#endif
				createNodeLinkedList(current,opensetBucket,out result_begin,out result_end);

				var path = new AStarPath(map) 
				{
					ExpectedBegin 	= start_at,
					ExpectedEnd 	= destination_at,
					Begin 			= result_begin,
					End 			= result_end,
				};

				// NOTE(donghyun-you): invalidate if something changed resulted route
				path.InvalidatedNode = path.Find(spot=>map.Spots[spot.x,spot.y].IsAvailable == false);

				// NOTE(donghyun-you): free node list
				opensetBucket.Clear();
				opensetEnteries.Clear();
				used.Clear();

				_nodeListPool.Free(opensetBucket);
				_listPool.Free(opensetEnteries);
				_hashSetPool.Free(used);

				Logger.Verbose(this,"bucket length: "+opensetBucket.Count);
				Logger.Verbose(this,"enteries length: "+opensetEnteries.Count);
				Logger.Verbose(this,"node list released count: "+_nodeListPool.ReleasedCount);
				Logger.Verbose(this,"int list released count: "+_listPool.ReleasedCount);

				return path;
			}

			private void createNodeLinkedList(InnerNode last,List<InnerNode> openset_bucket,out Node result_begin,out Node result_end) 
			{
				Node result_prev = null;
				result_begin = null;
				result_end = null;

				do 
				{
					Node result_current = new Node() { x = last.x, y = last.y };
					result_current.next = null;

					if(result_begin == null) 
					{
						result_begin = result_current;	
					}

					if(result_prev != null) 
					{
						result_prev.next = result_current;
					}

					result_prev = result_current;

					if(last.previous >= 0) 
					{
						last = openset_bucket[last.previous];
					}
					else 
					{
						break;
					}
				}
				while (true);

				result_end = result_begin;
				result_begin = result_begin.Reverse();
			}

			private List<InnerNode> successors(int x, int y, AStarMap map, int rows, int cols, FindingType type) 
			{
				int north = y - 1;	
				int south = y + 1;
				int east = x + 1;
				int west = x - 1;

				bool isNorthAvailable 	= north > -1 	&& map.Spots[x,north].IsAvailable && map.AllowDirection.north;
				bool isSouthAvailable 	= south < rows 	&& map.Spots[x,south].IsAvailable && map.AllowDirection.south;
				bool isEastAvailable 	= east < cols 	&& map.Spots[east,y].IsAvailable  && map.AllowDirection.east;
				bool isWestAvailable 	= west > -1 	&& map.Spots[west,y].IsAvailable  && map.AllowDirection.west;

				List<InnerNode> result = this._nodeListPool.Alloc() ?? new List<InnerNode>();

				if (isNorthAvailable) 	result.Add(new InnerNode(x, north));
				if (isEastAvailable) 	result.Add(new InnerNode(east, y));
				if (isSouthAvailable) 	result.Add(new InnerNode(x, south));
				if (isWestAvailable) 	result.Add(new InnerNode(west,y));

				if(type == FindingType.kDiagonal || type == FindingType.kEuclidean) 
				{
					return diagonalSuccessors(isNorthAvailable, isSouthAvailable, isEastAvailable, isWestAvailable, north, south, east, west, map, rows, cols, result);
				}
				else if(type == FindingType.kDiagonalFree || type == FindingType.kEuclideanFree)
				{
					return diagonalSuccessorsFree(north, south, east, west, map, rows, cols, result);
				}
				else 
				{
					return result;
				}
			}

			private List<InnerNode> diagonalSuccessors (bool is_north_available, bool is_south_available, bool is_east_available, bool is_west_available, int north, int south, int east, int west, AStarMap map, int rows, int cols, List<InnerNode> result) 
			{
				if (is_north_available) 
				{
					if (is_east_available && map.Spots[east,north].IsAvailable && map.AllowDirection.northEast)
					{
						result.Add(new InnerNode(east, north));
					}
					if (is_west_available && map.Spots[west,north].IsAvailable && map.AllowDirection.northWest)
					{
						result.Add(new InnerNode(west, north));
					}
				}

				if (is_south_available) 
				{
					if (is_east_available && map.Spots[east,south].IsAvailable && map.AllowDirection.southEast) 
					{
						result.Add(new InnerNode(east, south));
					}
					if (is_west_available && map.Spots[west,south].IsAvailable && map.AllowDirection.southWest) 
					{
						result.Add(new InnerNode(west, south));
					}
				}

				return result;
			}

			private List<InnerNode> diagonalSuccessorsFree (int north, int south, int east, int west, AStarMap map, int rows, int cols, List<InnerNode> result) 
			{
				bool xN = north > -1;
				bool xS = south < rows;
				bool xE = east < cols;
				bool xW = west > -1;

				if (xE) 
				{
					if (xN && map.Spots[east,north].IsAvailable && map.AllowDirection.northEast)
					{
						result.Add(new InnerNode(east, north));
					}
					if (xS && map.Spots[east,south].IsAvailable && map.AllowDirection.southEast)
					{
						result.Add(new InnerNode(east, south));
					}
				}

				if (xW) 
				{
					if (xN && map.Spots[west,north].IsAvailable && map.AllowDirection.northWest)
					{
						result.Add(new InnerNode(west, north));
					}
					if (xS && map.Spots[west,south].IsAvailable && map.AllowDirection.southWest) 
					{
						result.Add(new InnerNode(west, south));
					}
				}

				return result;
			}
			#endregion
		}
	}

	// NOTE(donghyun-you): avoding number of ILs of each Node instances. implement the static extension
	public static class AStarPathExtension
	{
		public static AStarPath.Node Reverse(this AStarPath.Node begin) 
		{
			AStarPath.Node focus = begin;
			AStarPath.Node before = null;
			AStarPath.Node next = null;

			while(focus != null)
			{
				next = focus.next;
				focus.next = before;
				before = focus;
				focus = next;
			}

			return before;
		}
	}
}