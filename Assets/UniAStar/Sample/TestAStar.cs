using UniTest;
using UnityEngine;
using System;
using System.Collections;

namespace UniAStar.UnitTest
{
	[TestScenario(1, Summary: "Test AStar Scenarios")]
	public class TestAStar : TestFlow
	{
		private static AStarMap allDirectionWallMap = null;
		private static AStarMap NESEDirectionWallMap = null;
		private static AStarMap allDirectionFieldMap = null;
		private static AStarMap NESWDirectionFieldMap = null;

		[TestScenario(1, Summary: "Initialize")]
		public void Initialize()
		{
			allDirectionWallMap = createTestMap(AStarCardinalDirection.Factory.CreateAll(),AStarPath.FindingType.kDiagonal);
			AssertIf("Map(all,wall)",allDirectionWallMap).Should.Not.Be.Null(allDirectionWallMap.ToString());

			NESEDirectionWallMap = createTestMap(AStarCardinalDirection.Factory.CreateNESW(),AStarPath.FindingType.kDiagonal);
			AssertIf("Map(NESW,wall)",NESEDirectionWallMap).Should.Not.Be.Null(NESEDirectionWallMap.ToString());

			allDirectionFieldMap = createTestMap(AStarCardinalDirection.Factory.CreateAll(),AStarPath.FindingType.kDiagonalFree);
			AssertIf("Map(all,field)",allDirectionFieldMap).Should.Not.Be.Null(allDirectionFieldMap.ToString());

			NESWDirectionFieldMap = createTestMap(AStarCardinalDirection.Factory.CreateNESW(),AStarPath.FindingType.kDiagonalFree);
			AssertIf("Map(NESW,field)",NESWDirectionFieldMap).Should.Not.Be.Null(NESWDirectionFieldMap.ToString());
		}

		/// <summary>
		/// test map. isAvailable = true
		/// </summary>
		private static readonly bool[,] testMap = new bool[,] 
		{
			{ true , true , true , true , true , true , true , true , false, true , false, false, false, false, false, false, false, false, false, false, },
			{ true , true , true , true , true , true , true , true , false, false, false, false, false, false, false, false, false, false, false, false, },
			{ true , true , true , false, true , true , true , true , true , true , false, false, false, false, false, false, false, false, false, false, },
			{ true , true , true , false, true , true , true , true , true , true , false, false, false, false, false, false, false, false, false, false, },
			{ true , false, true , false, true , true , true , true , true , true , false, false, false, false, false, false, false, false, false, false, },
			{ false, true , false, true , true , true , true , true , true , true , false, false, false, false, false, false, false, false, false, false, },
			{ false, true , false, true , true , true , true , true , true , true , false, false, false, false, false, false, false, false, false, false, },
			{ true , false, false, false, false, false, true , true , true , true , false, false, false, false, false, false, false, false, false, false, },
			{ false, true , false, true , true , true , true , true , true , false, false, false, false, false, false, false, false, false, false, false, },
			{ false, true , false, false, true , true , true , true , false, true , false, false, false, false, false, false, false, false, false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , true , true , true , true , false, false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , true , false, true , false, false, false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , false, false, true , true , false, false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , false, true , true , true , true , false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , false, true , true , true , true , false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , false, true , true , true , true , false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , false, false, true , true , true , false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , false, false, true , true , true , false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , true , false, true , false, false, false, false, },
			{ false, false, false, false, false, false, false, false, false, false, true , true , true , true , true , true , false, false, false, false, },
		};

		public class StartAndGoalPair 
		{
			public AStarPosition start;
			public AStarPosition goal;

			public override string ToString ()
			{
				return string.Format ("[StartAndGoalPair] {0} -> {1}",start.ToString(),goal.ToString());
			}
		}

		private static AStarMap createTestMap(AStarCardinalDirection allow_direction,AStarPath.FindingType finding_type) 
		{
			var aStarMap = new AStarMap(testMap.GetLength(0),testMap.GetLength(1),allow_direction,finding_type);

			for(int y=0,dY=testMap.GetLength(1);y<dY;y++) 
			{
				for(int x=0,dX=testMap.GetLength(0);x<dX;x++)
				{
					aStarMap.Map[x,y].IsAvailable = testMap[y,x];
				}
			}

			return aStarMap;
		}

		[TestScenario(2,Summary: "Test servral paths in sync")]
		public class TestInSync : TestFlow
		{
			private delegate void OnSGPairCompleteEvent(float duration,AStarPath path);
			private void testStartAndGoalPairs(bool isSyncTest,AStarMap map,StartAndGoalPair scenario,OnSGPairCompleteEvent on_complete) 
			{
				var startTime = Time.realtimeSinceStartup;

				if(isSyncTest) 
				{
					on_complete(Time.realtimeSinceStartup - startTime,map.FindPath(scenario.start,scenario.goal));
				}
				else 
				{
					map.FindPathAsync(scenario.start,scenario.goal,result=>
					{
						on_complete(Time.realtimeSinceStartup - startTime,result);
					});
				}
			}

			[TestScenario(1, Summary:"Test for all direction expecting reachable on the wall based map")]
			public IEnumerator ReachablePathsForAllDirectionAllowedForWallMap() 
			{
				var scenarios = new StartAndGoalPair[] 
				{
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(1,1) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(5,5) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(8,8) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(3,8) },
					new StartAndGoalPair { start = new AStarPosition(17,17), goal = new AStarPosition(11,11) },
				};

				for(int iSyncPhase=0;iSyncPhase<2;iSyncPhase++) 
				{
					bool isSyncTest = iSyncPhase == 0;
					int iPhase = 0;

					foreach(var scenario in scenarios)
					{
						iPhase++;

						// NOTE(donghyun-you): AStarPath instance is IDisposable. it do not expecting being reused. if invalidated, retry FindPath on AStarMap.
						AStarPath resultPath = null;
						float time = 0f;

						this.testStartAndGoalPairs(isSyncTest,allDirectionWallMap,scenario,(float duration, AStarPath path) => {
							time = duration;
							resultPath = path;
						});

						while(resultPath == null) yield return null;

						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] Reachable path",resultPath).Should.Not.Be.Null("and it's timed to calculate: "+time+"ms, and the path is "+resultPath.ToString());
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] invalidated node("+(resultPath.InvalidatedNode == null ? "?":resultPath.InvalidatedNode.ToString())+")",resultPath.InvalidatedNode).Should.Be.Null();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result valid",resultPath.IsValid).Should.Be.True();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result",resultPath.IsReachable).Should.Be.True();

						resultPath.Dispose();
					}
				}

				// NOTE(donghyun-you): if Debug.Break and monitor GC alloc here,it will be take some resources. AStarPath is pool based implementation it has.
				yield break;
			}

			[TestScenario(2, Summary:"Test for all direction expecting unreachable on the wall based map")]
			public IEnumerator UnreachablePathsForAllDirectionAllowedForWallMap() 
			{
				var scenarios = new StartAndGoalPair[] 
				{
					new StartAndGoalPair { start = new AStarPosition(9,9), goal = new AStarPosition(0,0) },
					new StartAndGoalPair { start = new AStarPosition(9,9), goal = new AStarPosition(1,1) },
					new StartAndGoalPair { start = new AStarPosition(9,9), goal = new AStarPosition(5,5) },
				};

				for(int iSyncPhase=0;iSyncPhase<2;iSyncPhase++) 
				{
					bool isSyncTest = iSyncPhase == 0;
					int iPhase = 0;

					foreach(var scenario in scenarios)
					{
						iPhase++;

						// NOTE(donghyun-you): AStarPath instance is IDisposable. it do not expecting being reused. if invalidated, retry FindPath on AStarMap.
						AStarPath resultPath = null;
						float time = 0f;

						this.testStartAndGoalPairs(isSyncTest,allDirectionWallMap,scenario,(float duration, AStarPath path) => {
							time = duration;
							resultPath = path;
						});

						while(resultPath == null) yield return null;

						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] Reachable path",resultPath).Should.Not.Be.Null("and it's timed to calculate: "+time+"ms, and the path is "+resultPath.ToString());
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] invalidated node("+(resultPath.InvalidatedNode == null ? "?":resultPath.InvalidatedNode.ToString())+")",resultPath.InvalidatedNode).Should.Be.Null();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result valid",resultPath.IsValid).Should.Be.True();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result",resultPath.IsReachable).Should.Be.False();

						resultPath.Dispose();
					}
				}

				// NOTE(donghyun-you): let unity getting the caputre profile to see pause on belows.
				yield return null;

				// NOTE(donghyun-you): 	here it is appropriate point to checking out the GC Alloc on the Unity's profiler. if you want to see how many garbages produced each running.
				//						to see GC Alloc on path finding, do phases on belows.
				//
				//						* Unity3D do not provide any scriptable interface to profiling. it cannot be automated. (T-T)
				//
				//						1. uncomment Debug.Break(); on belows.
				//						2. open TestRunner unity scene
				// 						3. enable {Deep Profile} (Windows->Profiler->Deep Profile) and enable {Record}, disable {Profile Editor}. // all curly braced name is like tab radio button like things on Profiler view
				//						4. play on the unity editor
				//						5. run unit test
				//						6. waiting for Unity3d paused. (its almost immediate)
				//						7. disable Record(recording for profiling) from Unity's profiler window
				//						8. input the search keyword to "AStarMap.FindPath" on Profiler window.
				//						9. select "AStarMap.FindPath" and delete search keyword that you wrote.
				//						10. open folding lists on profile window
				//
				// 						GC Alloc was about 0.8 to 1.0 Kbyte ((about, not sure but delta from hierarchy tell its 0.2x) 0.2kb for instance of Path, +@ for adding event (it cloning delegate internally.) for each AStarPath. 
				//						GC Alloc expected = 104 bytes (delegation cloning costs) * {number of not disposed AStarPath} + 0.25kb
				//Debug.Break();

				yield break;
			}

			[TestScenario(3, Summary:"Test for NESW(North-East-South-West) direction expecting reachable on the wall based map")]
			public IEnumerator ReachablePathsForNESWDirectionAllowedForWallMap() 
			{
				var scenarios = new StartAndGoalPair[] 
				{
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(1,1) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(5,5) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(5,5) },
				};

				for(int iSyncPhase=0;iSyncPhase<2;iSyncPhase++) 
				{
					bool isSyncTest = iSyncPhase == 0;
					int iPhase = 0;

					foreach(var scenario in scenarios)
					{
						iPhase++;

						// NOTE(donghyun-you): AStarPath instance is IDisposable. it do not expecting being reused. if invalidated, retry FindPath on AStarMap.
						AStarPath resultPath = null;
						float time = 0f;

						this.testStartAndGoalPairs(isSyncTest,NESEDirectionWallMap,scenario,(float duration, AStarPath path) => {
							time = duration;
							resultPath = path;
						});

						while(resultPath == null) yield return null;

						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] Reachable path",resultPath).Should.Not.Be.Null("and it's timed to calculate: "+time+"ms, and the path is "+resultPath.ToString());
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] invalidated node("+(resultPath.InvalidatedNode == null ? "?":resultPath.InvalidatedNode.ToString())+")",resultPath.InvalidatedNode).Should.Be.Null();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result valid",resultPath.IsValid).Should.Be.True();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result",resultPath.IsReachable).Should.Be.True();

						resultPath.Dispose();
					}
				}

				yield break;
			}

			[TestScenario(4, Summary:"Test for NESW(North-East-South-West) direction expecting unreachable on the wall based map")]
			public IEnumerator UnreachablePathsForNESWDirectionAllowedForWallMap() 
			{
				var scenarios = new StartAndGoalPair[] 
				{
					new StartAndGoalPair { start = new AStarPosition(9,9), goal = new AStarPosition(0,0) },
					new StartAndGoalPair { start = new AStarPosition(9,9), goal = new AStarPosition(1,1) },
					new StartAndGoalPair { start = new AStarPosition(9,9), goal = new AStarPosition(5,5) },
				};

				for(int iSyncPhase=0;iSyncPhase<2;iSyncPhase++) 
				{
					bool isSyncTest = iSyncPhase == 0;
					int iPhase = 0;

					foreach(var scenario in scenarios)
					{
						iPhase++;

						// NOTE(donghyun-you): AStarPath instance is IDisposable. it do not expecting being reused. if invalidated, retry FindPath on AStarMap.
						AStarPath resultPath = null;
						float time = 0f;

						this.testStartAndGoalPairs(isSyncTest,NESEDirectionWallMap,scenario,(float duration, AStarPath path) => {
							time = duration;
							resultPath = path;
						});

						while(resultPath == null) yield return null;

						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] Reachable path",resultPath).Should.Not.Be.Null("and it's timed to calculate: "+time+"ms, and the path is "+resultPath.ToString());
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] invalidated node("+(resultPath.InvalidatedNode == null ? "?":resultPath.InvalidatedNode.ToString())+")",resultPath.InvalidatedNode).Should.Be.Null();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result valid",resultPath.IsValid).Should.Be.True();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result",resultPath.IsReachable).Should.Be.False();

						resultPath.Dispose();
					}
				}

				yield break;
			}

			[TestScenario(5, Summary:"Test for all direction expecting reachable on the wall based map on the field based map")]
			public IEnumerator ReachablePathsForAllDirectionAllowedForFieldMap() 
			{
				var scenarios = new StartAndGoalPair[] 
				{
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(1,1) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(5,5) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(8,8) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(3,8) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(1,6) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(1,8) },
				};

				for(int iSyncPhase=0;iSyncPhase<2;iSyncPhase++) 
				{
					bool isSyncTest = iSyncPhase == 0;
					var iPhase = 0;

					foreach(var scenario in scenarios)
					{
						iPhase++;

						// NOTE(donghyun-you): AStarPath instance is IDisposable. it do not expecting being reused. if invalidated, retry FindPath on AStarMap.
						AStarPath resultPath = null;
						float time = 0f;
						this.testStartAndGoalPairs(isSyncTest,allDirectionFieldMap,scenario,(float duration, AStarPath path) => {
							time = duration;
							resultPath = path;
						});

						while(resultPath == null) yield return null;

						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] Reachable path",resultPath).Should.Not.Be.Null("and it's timed to calculate: "+time+"ms, and the path is "+resultPath.ToString());
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] invalidated node("+(resultPath.InvalidatedNode == null ? "?":resultPath.InvalidatedNode.ToString())+")",resultPath.InvalidatedNode).Should.Be.Null();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result valid",resultPath.IsValid).Should.Be.True();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result",resultPath.IsReachable).Should.Be.True();

						resultPath.Dispose();
					}
				}

				yield break;
			}

			[TestScenario(6, Summary:"Test for all direction expecting unreachable on the field based map")]
			public IEnumerator UnreachablePathsForAllDirectionAllowedForFieldMap() 
			{
				var scenarios = new StartAndGoalPair[] 
				{
					new StartAndGoalPair { start = new AStarPosition(9,0), goal = new AStarPosition(0,0) },
					new StartAndGoalPair { start = new AStarPosition(9,0), goal = new AStarPosition(1,1) },
					new StartAndGoalPair { start = new AStarPosition(9,0), goal = new AStarPosition(5,5) },
				};

				for(int iSyncPhase=0;iSyncPhase<2;iSyncPhase++) 
				{
					bool isSyncTest = iSyncPhase == 0;
					var iPhase = 0;

					foreach(var scenario in scenarios)
					{
						iPhase++;

						// NOTE(donghyun-you): AStarPath instance is IDisposable. it do not expecting being reused. if invalidated, retry FindPath on AStarMap.
						AStarPath resultPath = null;
						float time = 0f;
						this.testStartAndGoalPairs(isSyncTest,allDirectionFieldMap,scenario,(float duration, AStarPath path) => {
							time = duration;
							resultPath = path;
						});

						while(resultPath == null) yield return null;

						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] Reachable path",resultPath).Should.Not.Be.Null("and it's timed to calculate: "+time+"ms, and the path is "+resultPath.ToString());
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] invalidated node("+(resultPath.InvalidatedNode == null ? "?":resultPath.InvalidatedNode.ToString())+")",resultPath.InvalidatedNode).Should.Be.Null();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result valid",resultPath.IsValid).Should.Be.True();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result",resultPath.IsReachable).Should.Be.False();

						resultPath.Dispose();
					}
				}

				yield break;
			}


			[TestScenario(7, Summary:"Test for NESW direction expecting reachable on the wall based map on the field based map")]
			public IEnumerator ReachablePathsForNESWDirectionAllowedForFieldMap() 
			{
				var scenarios = new StartAndGoalPair[] 
				{
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(1,1) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(5,5) },
					new StartAndGoalPair { start = new AStarPosition(0,0), goal = new AStarPosition(8,8) },
				};

				for(int iSyncPhase=0;iSyncPhase<2;iSyncPhase++) 
				{
					bool isSyncTest = iSyncPhase == 0;
					var iPhase = 0;

					foreach(var scenario in scenarios)
					{
						iPhase++;

						// NOTE(donghyun-you): AStarPath instance is IDisposable. it do not expecting being reused. if invalidated, retry FindPath on AStarMap.
						AStarPath resultPath = null;
						float time = 0f;
						this.testStartAndGoalPairs(isSyncTest,NESWDirectionFieldMap,scenario,(float duration, AStarPath path) => {
							time = duration;
							resultPath = path;
						});

						while(resultPath == null) yield return null;

						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] Reachable path",resultPath).Should.Not.Be.Null("and it's timed to calculate: "+time+"ms, and the path is "+resultPath.ToString());
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] invalidated node("+(resultPath.InvalidatedNode == null ? "?":resultPath.InvalidatedNode.ToString())+")",resultPath.InvalidatedNode).Should.Be.Null();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result valid",resultPath.IsValid).Should.Be.True();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result",resultPath.IsReachable).Should.Be.True();

						resultPath.Dispose();
					}
				}

				yield break;
			}

			[TestScenario(8, Summary:"Test for NESW direction expecting unreachable on the field based map")]
			public IEnumerator UnreachablePathsForNESWDirectionAllowedForFieldMap() 
			{
				var scenarios = new StartAndGoalPair[] 
				{
					new StartAndGoalPair { start = new AStarPosition(9,0), goal = new AStarPosition(0,0) },
					new StartAndGoalPair { start = new AStarPosition(9,0), goal = new AStarPosition(1,1) },
					new StartAndGoalPair { start = new AStarPosition(9,0), goal = new AStarPosition(5,5) },
				};

				for(int iSyncPhase=0;iSyncPhase<2;iSyncPhase++) 
				{
					bool isSyncTest = iSyncPhase == 0;
					var iPhase = 0;

					foreach(var scenario in scenarios)
					{
						iPhase++;

						// NOTE(donghyun-you): AStarPath instance is IDisposable. it do not expecting being reused. if invalidated, retry FindPath on AStarMap.
						AStarPath resultPath = null;
						float time = 0f;
						this.testStartAndGoalPairs(isSyncTest,NESWDirectionFieldMap,scenario,(float duration, AStarPath path) => {
							time = duration;
							resultPath = path;
						});

						while(resultPath == null) yield return null;

						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] Reachable path",resultPath).Should.Not.Be.Null("and it's timed to calculate: "+time+"ms, and the path is "+resultPath.ToString());
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] invalidated node("+(resultPath.InvalidatedNode == null ? "?":resultPath.InvalidatedNode.ToString())+")",resultPath.InvalidatedNode).Should.Be.Null();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result valid",resultPath.IsValid).Should.Be.True();
						AssertIf("["+iPhase+"/"+scenario+"/"+(isSyncTest?"Sync":"Async")+"] the path test in all direction allowed result",resultPath.IsReachable).Should.Be.False();

						resultPath.Dispose();
					}
				}

				yield break;
			}

			[TestScenario(9, Summary:"Test invalidation")]
			public IEnumerator InvalidationTest() 
			{
				var map = createTestMap(AStarCardinalDirection.Factory.CreateNESW(),AStarPath.FindingType.kDiagonal);

				// NOTE(donghyun-you): test it will be validated
				AStarPath path = null;
				map.FindPathAsync(new AStarPosition(0,0),new AStarPosition(0,2),result=>
				{
						path = result;
				});
				
				while(path == null) yield return null;

				AssertIf("path",path).Should.Not.Be.Null(path.ToString());
				AssertIf("path validation",path.IsValid).Should.Be.True();

				map.Map[0,1].IsAvailable = false;

				AssertIf("path validation after 0/1 turn to unavailable",path.IsValid).Should.Be.False();

				path.Dispose();
				path = null;
				map.Map[0,1].IsAvailable = true;

				map.FindPathAsync(new AStarPosition(0,0),new AStarPosition(0,2),result=>
				{
					path = result;
				});

				yield return null;

				map.Map[0,1].IsAvailable = false;

				while(path == null) yield return null;

				AssertIf("path",path).Should.Not.Be.Null(path.ToString());
				AssertIf("path validation after 0/1 turn to unavailable(change availability before path published) at one frame after",path.IsValid).Should.Be.False();

				path.Dispose();
				path = null;
				map.Map[0,1].IsAvailable = true;

				yield return null;

				map.FindPathAsync(new AStarPosition(0,0),new AStarPosition(0,2),result=>
				{
					path = result;
				});
				
				map.Map[0,1].IsAvailable = false;

				while(path == null) yield return null;

				AssertIf("path",path).Should.Not.Be.Null(path.ToString());
				// NOTE(donghyun-you): 	in this case. framework do not ensure it will be still validated(expecting IsValid=true). 
				//						sometimes Pathfinder pass the route that avoid the blocked AStarPath.Spot, sometimes not but its invalidated(IsValid=false).
				// 						so it just warn.
				WarnIf("path validation after 0/1 turn to unavailable(change availability before path published) at same frame find path invoked",path.IsValid).Should.Be.True();

				yield break;
			}
		}
	}
}