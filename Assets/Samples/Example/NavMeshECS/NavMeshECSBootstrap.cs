using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace de.JochenHeckl.NavMeshECS.Example
{

    public class NavMeshECSBootstrap : MonoBehaviour
    {
        public GameObject agentPrefab;
        public int numAgentsToSpawn;

        public int agentMinVelocity;
        public int agentMaxVelocity;

        public float minSpawnRadius;
        public float maxSpawnRadius;

        private Entity agentPrefabEntity;

        private Entity activeAgent;

        void Start()
        {
            UnityEngine.Random.InitState( 0 );

            var repathSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RepathToRandomPositionSystem>();
            repathSystem.MinSpawnRadius = minSpawnRadius;
            repathSystem.MaxSpawnRadius = maxSpawnRadius;

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

                var agentPosition = RandomPosition.MakeRandomPosition( minSpawnRadius, maxSpawnRadius );
                
                entityManager.SetComponentData( activeAgent, new Translation() { Value = agentPosition } );

                entityManager.AddComponentData( activeAgent, new PathLocomotionData()
                {
                    destinationReached = true,
                    maxVelocity = Random.Range( agentMinVelocity, agentMaxVelocity )
                } ); ;

            }
        }

        
    }
}