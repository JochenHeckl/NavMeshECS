using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

namespace de.JochenHeckl.NavMeshECS.Example
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
                    pathLocomotionData.nextPathVertexIndex = 0;

                    EntityManager.RemoveComponent<PathVertexResultData>( entity );
                    
                    SetNewDestination( entity, translation );
                }
            } );
        }

        private void SetNewDestination( Entity entity, Translation translation )
        {
            var newDestination = RandomPosition.MakeRandomPosition(MinSpawnRadius, MaxSpawnRadius);
            
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
    }
}