using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;

namespace de.JochenHeckl.NavMeshECS.Editor.Tests
{
    class EditorTest
    {

        [Test]
        public void TestSSFASolverSimpleSquare()
        {
            var start = new float3( 0f, 0f, 0f );
            var destination = new float3( 1f, 0f, 1f );

            var portals = new (float3 right , float3 left)[]
            {
                ( new float3( 1f, 0f, 0f ), new float3( 0f, 0f, 1f ))
            };

            using (var pathResult = new NativeArray<float3>( 10, Allocator.Temp ))
            {
                var pathLength = SSFASolver.Solve( start, destination, portals, pathResult );

                Assert.AreEqual( pathResult.Take( pathLength ).ToArray(), new float3[] { start, destination } );
            }
        }

        [Test]
        public void TestSSFASolverLeftPortal()
        {
            var start = new float3( 0f, 0f, 0f );
            var destination = new float3( 2f, 0f, 0f );

            var portals = new (float3 right, float3 left)[]
            {
                ( new float3( 1f, 0f, 1f ), new float3( 1f, 0f, 2f ))
            };

            using (var pathResult = new NativeArray<float3>( 10, Allocator.Temp ))
            {
                var pathLength = SSFASolver.Solve( start, destination, portals, pathResult );

                Assert.AreEqual( pathResult.Take(pathLength).ToArray(), new float3[] { start, portals[0].right, destination } );
            }           
        }
    }
}