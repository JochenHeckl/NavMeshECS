using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace de.JochenHeckl.NavMeshECS
{
    public struct PathLocomotionData : IComponentData
    {
        public bool destinationReached;
        public int nextPathVertexIndex;
        public float maxVelocity;
    }
}
