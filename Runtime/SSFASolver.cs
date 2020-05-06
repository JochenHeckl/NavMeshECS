using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace de.JochenHeckl.NavMeshECS
{
    public static class SSFASolver
    {
        private static readonly float epsilonEqual = math.sqrt( float.Epsilon ) * 10f;

        public static int Solve( float3 startPosition, float3 destination, (float3 right, float3 left)[] portals, NativeArray<float3> pathVerticesContainer )
        {
            var currentPathVertexIndex = 0;

            PushPathVertex( ref pathVerticesContainer, ref currentPathVertexIndex, startPosition );

            if (portals.Length > 0)
            {
                (var apex, var funnel) = (startPosition, portals[0]);

                for (var portalIdx = 1; portalIdx < portals.Length; ++portalIdx)
                {
                    (apex, funnel) = SolvePortal( apex, funnel, portals[portalIdx], ref pathVerticesContainer, ref currentPathVertexIndex );
                }

                var funnelUp = CalculateFunnelUp( apex, funnel );

                if (!TestFunnelLeft( apex, destination, funnel.left, funnelUp ))
                {
                    PushPathVertex( ref pathVerticesContainer, ref currentPathVertexIndex, funnel.left );
                }
                else if (!TestFunnelRight( apex, funnel.right, destination, funnelUp ))
                {
                    PushPathVertex( ref pathVerticesContainer, ref currentPathVertexIndex, funnel.right );
                }
            }

            PushPathVertex( ref pathVerticesContainer, ref currentPathVertexIndex, destination );

            return currentPathVertexIndex;
        }

        private static (float3 apex, (float3 right, float3 left) funnel) SolvePortal(
            float3 apex,
            (float3 right, float3 left) funnel,
            (float3 right, float3 left) portal,
            ref NativeArray<float3> pathVerticesContainer,
            ref int currentPathVertexIndex )
        {
            var funnelUp = CalculateFunnelUp( apex, funnel );

            var portalLeftWithinFunnel = TestFunnelLeft( apex, portal.left, funnel.left, funnelUp );
            var portalRightWithinFunnel = TestFunnelRight( apex, funnel.right, portal.right, funnelUp );

            if (portalLeftWithinFunnel && portalRightWithinFunnel)
            {
                return (apex, portal);
            }

            if ((!portalLeftWithinFunnel) && (!portalRightWithinFunnel))
            {
                return (apex, funnel);
            }

            if (!portalLeftWithinFunnel)
            {
                var portalIsAllLeft = !TestFunnelLeft( apex, portal.right, funnel.left, funnelUp );

                if (portalIsAllLeft)
                {
                    PushPathVertex( ref pathVerticesContainer, ref currentPathVertexIndex, funnel.left );
                    return (funnel.left, portal);
                }
                else
                {
                    return (apex, (portal.right, funnel.left));
                }
            }
            else if (!portalRightWithinFunnel)
            {
                var portalIsAllRight = !TestFunnelRight( apex, funnel.right, portal.left, funnelUp );

                if (portalIsAllRight)
                {
                    PushPathVertex( ref pathVerticesContainer, ref currentPathVertexIndex, funnel.right );
                    return (funnel.right, portal);
                }
                else
                {
                    return (apex, (funnel.right, portal.left));
                }
            }

            return (apex, portal);
        }

        private static bool TestFunnel( float3 apex, float3 funnelRight, float3 funnelLeft, float3 funnelUp )
        {
            return math.dot( CalculateFunnelUp( apex, funnelRight, funnelLeft ), funnelUp ) > 0f;
        }

        private static bool TestFunnelLeft( float3 apex, float3 nextLeft, float3 funnelLeft, float3 funnelUp )
        {
            return AreEqual( apex, funnelLeft )
                || AreEqual( nextLeft, funnelLeft )
                || math.dot( CalculateFunnelUp( apex, nextLeft, funnelLeft ), funnelUp ) > 0f;
        }

        private static bool TestFunnelRight( float3 apex, float3 funnelRight, float3 nextRight, float3 funnelUp )
        {
            return AreEqual( apex, funnelRight )
                || AreEqual( funnelRight, nextRight )
                || math.dot( CalculateFunnelUp( apex, funnelRight, nextRight ), funnelUp ) > 0f;
        }

        private static void Swap( ref float3 funnelLeft, ref float3 funnelRight )
        {
            var temp = funnelLeft;
            funnelLeft = funnelRight;
            funnelRight = temp;
        }

        private static bool AreEqual( float3 one, float3 other )
        {
            return math.distancesq( one, other ) < epsilonEqual;
        }

        private static float3 CalculateFunnelUp( float3 apex, float3 funnelRight, float3 funnelLeft )
        {
            return math.cross( funnelLeft - apex, funnelRight - apex );
        }

        private static float3 CalculateFunnelUp( float3 apex, (float3 right, float3 left) funnel )
        {
            return math.cross( funnel.left - apex, funnel.right - apex );
        }

        private static void PushPathVertex( ref NativeArray<float3> pathVerticesContainer, ref int currentPathVertexIndex, float3 pathVertex )
        {
            if (currentPathVertexIndex < pathVerticesContainer.Length)
            {
                pathVerticesContainer[currentPathVertexIndex] = pathVertex;
                currentPathVertexIndex++;
            }
        }

        private static bool TestDestination( float3 apex, float3 destination, IEnumerable<(float3 right, float3 left)> enumerable )
        {
            throw new NotImplementedException();
        }
    }
}
