using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

namespace de.JochenHeckl.NavMeshECS
{
    public struct QueryPathRequestData : IComponentData
    {
        public int agentTypeId;
        public int areaMask;
        public int areaCostIndex;

        public float3 startPosition;
        public float3 destinationPosition;
        public float3 mapPositionExtents;
    }
}
