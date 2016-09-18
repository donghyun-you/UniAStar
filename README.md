#UniAStar

**Simple A* implementation** for Unity3D

A* implementation based on ported version of http://devpro.it/code/137.html (MIT Style Licensed)

## Features

- Managed A* map (AStarMap)
- Direction limitation configurable on initiate AStarMap (max 8 cardinal direction (NESW,and between of these ranges))
- Factory for avoiding cloning ILs for generated AStarPath
- Path Validation from managed spot availability (managed spot = AStarMap.Spot). get path and if the route got any effected, AStarPath instance will be invalidated.
- Optimized for C# (pooling(Pool<T>), struct based operations for reduce garbages)
- Async operation throught thread pool
- Unit test codes has included with UniTest (remove UniTest,UniEditor folder on release your product)
- currently GC Allocating 0.2x kb + 104byte*number of not disposed AStarPath(s) on AStarMap for every path finds

## Usage

```cs
// create aStarMap (managed a* map, 10x10 size, is_allow_diagonal = true)
AStarMap aStarMap = new AStarMap(10,10,AStarCardinalDirection.Factory.CreateAll(),AStarPath.FindingType.kDiagonal);

// Update availability
for(int x=0,dX=10; x<dX;x++) 
{
	for(int y=0,dY=10;y<dY;y++) 
	{
		aStarMap.Map[x,y].IsAvailable = true/* or false from your buffer*/;
	}
}

// Find the path synchronously
var aStarPath = aStarMap.FindPath(new AStarPosition(0,0),new AStarPosition(5,5));

// Find the path asynchronously
aStarMap.FindPathAsync(new AStarPosition(0,0),new AStarPosition(5,5),(AStarPath)path=>
{
// done. it works on thread pool
});
		
// if aStarPath contains 3,4 for route,
aStarMap.Map[3,4].IsAvailable = false;
// path will be invalidated(IsValid == false)
Debug.Assert(aStarPath.IsValid);

// aStarPath must be disposed for stopping receiving event from AStarMap
aStarPath.Dispose();

```

see more usages on [TestAStar.cs](Assets/UniAStar/Sample/TestAStar.cs)


## License (MIT)

Copyright (c) 2016 Donghyun You(ruel.o.wisp@gmail.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.