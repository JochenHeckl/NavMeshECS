using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

namespace de.JochenHeckl.NavMeshECS
{
    public struct QueryPathRequest : IComponentData
    {
        public bool isDone;
        public int assignedSlot;
        
        public PathQueryStatus queryStatus;

        public int agentTypeId;
        public int areaMask;
        public int areaCostIndex;

        public float3 startPosition;
        public float3 destinationPosition;

        public float3 mapPositionExtents;
    }
}
