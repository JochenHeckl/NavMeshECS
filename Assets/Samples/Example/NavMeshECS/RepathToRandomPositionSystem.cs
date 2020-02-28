using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

namespace de.JochenHeckl.NavMeshECS
{
    [UpdateBefore( typeof( QueryPathSystem ) )]
    public class RepathToRandomPositionSystem : ComponentSystem
    {
        private readonly int walkableAreaMask = 1 << NavMesh.GetAreaFromName( "Walkable" );

        public float MinSpawnRadius { get; set; }
        public float MaxSpawnRadius { get; set; }

        protected override void OnUpdate()
        {
            Entities.ForEach( ( Entity entity, ref Translation translation, ref PathLocomotionData pathLocomotionData ) =>
            {
                if (pathLocomotionData.destinationReached)
                {
                    pathLocomotionData.destinationReached = false;
                    EntityManager.RemoveComponent<PathVertexResultData>( entity );
                    
                    SetNewDestination( entity, translation );
                }
            } );
        }

        private void SetNewDestination( Entity entity, Translation translation )
        {
            var newDestination = MakeRandomPosition();
            var upOffset = new float3( 0f, 0.5f, 0f );

            Debug.DrawLine(
                translation.Value + upOffset,
                newDestination + upOffset,
                Color.red,
                5f );

            var newDestinationQuery = new QueryPathRequestData()
            {
                agentTypeId = 0,
                areaCostIndex = 0,
                areaMask = walkableAreaMask,
                startPosition = translation.Value,
                destinationPosition = newDestination,
                mapPositionExtents = new float3( 1f, 1f, 1f )
            };

            if (EntityManager.HasComponent<QueryPathRequestData>( entity ))
            {
                EntityManager.SetComponentData( entity, newDestinationQuery );
            }
            else
            {
                EntityManager.AddComponentData( entity, newDestinationQuery );
            }
        }

        private float3 MakeRandomPosition()
        {
            var position = UnityEngine.Random.insideUnitCircle * MinSpawnRadius * UnityEngine.Random.Range( MinSpawnRadius, MaxSpawnRadius );
            return new float3( position.x, 0f, position.y );
        }
    }
}