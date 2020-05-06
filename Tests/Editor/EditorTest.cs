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

            var portals = new (float3 right, float3 left)[]
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
        {            var start = new float3( 0f, 0f, 0f );
            var destination = new float3( 2f, 0f, 0f );

            var portals = new (float3 right, float3 left)[]
            {
                ( new float3( 1f, 0f, 1f ), new float3( 1f, 0f, 2f ))
            };

            using (var pathResult = new NativeArray<float3>( 10, Allocator.Temp ))
            {
                var pathLength = SSFASolver.Solve( start, destination, portals, pathResult );

                Assert.AreEqual( pathResult.Take( pathLength ).ToArray(), new float3[] { start, portals[0].right, destination } );
            }
        }

        [Test]
        public void TestIRLBug0001Fixed()
        {
            var start = new float3( -18.46583f, 0f, 10.78635f );
            var destination = new float3( -6.755621f, 0f, -18.75267f );

            var portals = new (float3 right, float3 left)[]
            {
                (new float3(-4.333332f, 0.08333349f, 3.666667f), new float3(-42.66667f, 0.08333349f, 0f)),
                (new float3( -5.333332f, 0.08333349f, 2f ), new float3( -42.66667f, 0.08333349f, 0f )),
                (new float3( -5.5f, 0.08333349f, 0f ), new float3( -42.66667f, 0.08333349f, 0f )),
                (new float3( -5.333332f, 0.08333349f, -2f ), new float3( -42.66667f, 0.08333349f, 0f )),
                (new float3( -4.333332f, 0.08333349f, -3.666668f ), new float3( -42.66667f, 0.08333349f, 0f ))
            };

            using (var pathResult = new NativeArray<float3>( 10, Allocator.Temp ))
            {
                var pathLength = SSFASolver.Solve( start, destination, portals, pathResult );

                Assert.AreEqual(
                    pathResult.Take( pathLength ).ToArray(),
                    new float3[] { start, destination } );
            }
        }

        [Test]
        public void TestIRLBug0002Fixed()
        {
            var start = new float3( -5f, 0f, 10f );
            var destination = new float3( -5f, 0f, -10f );

            var portals = new (float3 right, float3 left)[]
            {
                (new float3(-2f, 0f, 6f), new float3(-10f, 0f, 0f)),
                (new float3( -4f, 0f, 4f ), new float3(-10f, 0f, 0f)),
                (new float3( -6f, 0f, 0f ), new float3(-10f, 0f, 0f)),
                (new float3( -4f, 0f, -4f ), new float3(-10f, 0f, 0f)),
                (new float3( -2f, 0f, -6f ), new float3(-10f, 0f, 0f))
            };

            using (var pathResult = new NativeArray<float3>( 10, Allocator.Temp ))
            {
                var pathLength = SSFASolver.Solve( start, destination, portals, pathResult );

                Assert.AreEqual(
                    pathResult.Take( pathLength ).ToArray(),
                    new float3[] { start, portals[2].right, destination } );
            }
        }
    }
}