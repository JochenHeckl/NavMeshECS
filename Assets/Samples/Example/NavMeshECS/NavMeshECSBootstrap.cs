using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace de.JochenHeckl.NavMeshECS
{

    public class NavMeshECSBootstrap : MonoBehaviour
    {
        public GameObject agentPrefab;
        private Entity agentPrefabEntity;

        private Entity activeAgent;

        void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;

            var queryPathSystem = world.GetOrCreateSystem<QueryPathSystem>();

            var settings = GameObjectConversionSettings.FromWorld( world, null );
            agentPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy( agentPrefab, settings );

            activeAgent = entityManager.Instantiate( agentPrefabEntity );

            var randomPosition = Random.insideUnitCircle;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}