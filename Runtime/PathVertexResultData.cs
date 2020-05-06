using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace de.JochenHeckl.NavMeshECS
{
    [InternalBufferCapacity( 8 )]
    public struct PathVertexResultData : IBufferElementData
    {
        public static implicit operator float3( PathVertexResultData e ) { return e.Value; }
        public static implicit operator PathVertexResultData( float3 e ) { return new PathVertexResultData { Value = e }; }

        public float3 Value;
    }
}
