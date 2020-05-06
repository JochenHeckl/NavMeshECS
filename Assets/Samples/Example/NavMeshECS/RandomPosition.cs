using System;
using Unity.Mathematics;
using UnityEngine;

namespace de.JochenHeckl.NavMeshECS.Example
{
    public static class RandomPosition
    {
        public static float3 MakeRandomPosition( float minSpawnRadius, float maxSpawnRadius )
        {
            var position = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range( minSpawnRadius, maxSpawnRadius );

            return new float3(
                position.x,
                0f,
                position.y );
        }
    }
}
