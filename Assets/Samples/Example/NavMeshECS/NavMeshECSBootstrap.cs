using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace de.JochenHeckl.NavMeshECS
{

    public class NavMeshECSBootstrap : MonoBehaviour
    {
        public GameObject agentPrefab;
        public int numAgentsToSpawn = 1;

        public float minSpawnRadius = 10f;
        public float maxSpawnRadius = 40f;

        private Entity agentPrefabEntity;

        private Entity activeAgent;

        void Start()
        {
            UnityEngine.Random.InitState( 0 );

            SpawnAgents( numAgentsToSpawn, minSpawnRadius, maxSpawnRadius );
        }

        private void SpawnAgents( int numAgentsToSpawn, float minSpawnRadius, float maxSpawnRadius )
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            var settings = GameObjectConversionSettings.FromWorld( world, null );
            agentPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy( agentPrefab, settings );

            for (var agentIndex = 0; agentIndex < numAgentsToSpawn; agentIndex++)
            {
                activeAgent = entityManager.Instantiate( agentPrefabEntity );

                var agentPosition = MakeRandomPosition( minSpawnRadius, maxSpawnRadius );
                var firstDestination = MakeRandomPosition( minSpawnRadius, maxSpawnRadius );

                Debug.DrawLine(
                    new Vector3( agentPosition.x, agentPosition.y, agentPosition.z ) + Vector3.up,
                    new Vector3( firstDestination.x, firstDestination.y, firstDestination.z ) + Vector3.up,
                    Color.red,
                    5f );

                entityManager.SetComponentData( activeAgent, new Translation() { Value = agentPosition } );

                entityManager.AddComponentData( activeAgent, new QueryPathRequestData()
                {
                    agentTypeId = 0,
                    areaMask = -1,
                    startPosition = agentPosition,
                    destinationPosition = firstDestination,
                    mapPositionExtents = new float3( 1f, 1f, 1f )
                } );


                entityManager.AddComponentData( activeAgent, new PathLocomotionData()
                {
                    maxVelocity = 1f
                } );

            }
        }

        private float3 MakeRandomPosition( float minSpawnRadius, float maxSpawnRadius )
        {
            var position = UnityEngine.Random.insideUnitCircle * minSpawnRadius * UnityEngine.Random.Range( minSpawnRadius, maxSpawnRadius );
            return new float3( position.x, 0f, position.y );
        }
    }
}