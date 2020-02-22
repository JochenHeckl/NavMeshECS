using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace de.JochenHeckl.NavMeshECS
{
    [InternalBufferCapacity( 8 )]
    public struct QueryPathResult : IBufferElementData
    {
        public static implicit operator float3( QueryPathResult e ) { return e.Value; }
        public static implicit operator QueryPathResult( float3 e ) { return new QueryPathResult { Value = e }; }

        public float3 Value;
    }
}
