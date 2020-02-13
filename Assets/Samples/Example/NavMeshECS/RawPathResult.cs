using Unity.Collections;
using Unity.Entities;
using UnityEngine.Experimental.AI;

namespace de.JochenHeckl.NavMeshECS
{
    public struct RawPathResult : IComponentData
    {
        public int pathSize;
        //public NativeArray<PolygonId> pathPolygons;
        //public NativeArray<NavMeshLocation> resolvedPath;
        //public NativeArray<StraightPathFlags> resolvedPathFlags;
        //public NativeArray<float> vertexSide;
        public int straightPathCount;
    }
}
