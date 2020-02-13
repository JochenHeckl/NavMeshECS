using Unity.Entities;
using UnityEngine.Experimental.AI;

namespace de.JochenHeckl.NavMeshECS
{
    public struct PathQueryProcessing : IComponentData
    {
        public int processingQuerySlotIdx;
        public PathQueryStatus pathQueryStatus;
    }
}
