using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace de.JochenHeckl.NavMeshECS
{
    public static class SSFASolver
    {
        private static readonly float epsilonEqual = math.sqrt( float.Epsilon ) * 10f;
        private static bool TestFunnel( float3 apex, float3 funnelRight, float3 funnelLeft, float3 funnelUp )
        {
            return math.dot( math.cross( funnelRight - apex, funnelLeft - apex ), funnelUp ) > 0f;
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

        public static int Solve( float3 startPosition, float3 destsination, (float3 right, float3 left)[] portals, NativeArray<float3> pathVerticesContainer )
        {
            var apex = startPosition;
            var funnelLeft = portals[0].left;
            var funnelRight = portals[0].right;

            var currentPathVertexIndex = 0;

            PushPathVertex( pathVerticesContainer, ref currentPathVertexIndex, startPosition );

            var funnelUp = math.cross( funnelRight - apex, funnelLeft - apex );

            if (!TestFunnel( apex, funnelRight, funnelLeft, funnelUp ))
            {
                Swap( ref funnelRight, ref funnelLeft );
            }

            for (var portalIdx = 1; portalIdx < portals.Length; ++portalIdx)
            {
                if (!TestFunnel( apex, portals[portalIdx].right, portals[portalIdx].left, funnelUp ))
                {
                    Swap( ref portals[portalIdx].right, ref portals[portalIdx].left );
                }

                if (!AreEqual( apex, funnelLeft ) && !TestFunnel( apex, portals[portalIdx].left, funnelLeft, funnelUp ) )
                {
                    PushPathVertex( pathVerticesContainer, ref currentPathVertexIndex, funnelLeft );
                    
                    apex = funnelLeft;
                }

                funnelLeft = portals[portalIdx].left;
                funnelUp = math.cross( funnelRight - apex, funnelLeft - apex );

                if (!AreEqual( apex, funnelRight ) && !TestFunnel( apex, funnelRight, portals[portalIdx].right, funnelUp ))
                {
                    PushPathVertex( pathVerticesContainer, ref currentPathVertexIndex, funnelRight );

                    apex = funnelRight;
                }

                funnelRight = portals[portalIdx].right;
                funnelUp = math.cross( funnelRight - apex, funnelLeft - apex );
            }

            if (!AreEqual( funnelLeft, destsination ) && !TestFunnel( apex, destsination, funnelLeft, funnelUp ))
            {
                PushPathVertex( pathVerticesContainer, ref currentPathVertexIndex, funnelLeft );
            }

            if (!AreEqual( funnelRight, destsination ) && !TestFunnel( apex, funnelRight, destsination, funnelUp ))
            {
                PushPathVertex( pathVerticesContainer, ref currentPathVertexIndex, funnelRight );
            }

            PushPathVertex( pathVerticesContainer, ref currentPathVertexIndex, destsination );

            return currentPathVertexIndex;

            //// Add start point.
            //vcpy( &pts[npts * 2], portalApex );
            //npts++;

            //for (int i = 1; i < nportals && npts < maxPts; ++i)
            //{
            //    const float* left = &portals[i * 4 + 0];
            //    const float* right = &portals[i * 4 + 2];

            //    // Update right vertex.
            //    if (triarea2( portalApex, portalRight, right ) <= 0.0f)
            //    {
            //        if (vequal( portalApex, portalRight ) || triarea2( portalApex, portalLeft, right ) > 0.0f)
            //        {
            //            // Tighten the funnel.
            //            vcpy( portalRight, right );
            //            rightIndex = i;
            //        }
            //        else
            //        {
            //            // Right over left, insert left to path and restart scan from portal left point.
            //            vcpy( &pts[npts * 2], portalLeft );
            //            npts++;
            //            // Make current left the new apex.
            //            vcpy( portalApex, portalLeft );
            //            apexIndex = leftIndex;
            //            // Reset portal
            //            vcpy( portalLeft, portalApex );
            //            vcpy( portalRight, portalApex );
            //            leftIndex = apexIndex;
            //            rightIndex = apexIndex;
            //            // Restart scan
            //            i = apexIndex;
            //            continue;
            //        }
            //    }

            //    // Update left vertex.
            //    if (triarea2( portalApex, portalLeft, left ) >= 0.0f)
            //    {
            //        if (vequal( portalApex, portalLeft ) || triarea2( portalApex, portalRight, left ) < 0.0f)
            //        {
            //            // Tighten the funnel.
            //            vcpy( portalLeft, left );
            //            leftIndex = i;
            //        }
            //        else
            //        {
            //            // Left over right, insert right to path and restart scan from portal right point.
            //            vcpy( &pts[npts * 2], portalRight );
            //            npts++;
            //            // Make current right the new apex.
            //            vcpy( portalApex, portalRight );
            //            apexIndex = rightIndex;
            //            // Reset portal
            //            vcpy( portalLeft, portalApex );
            //            vcpy( portalRight, portalApex );
            //            leftIndex = apexIndex;
            //            rightIndex = apexIndex;
            //            // Restart scan
            //            i = apexIndex;
            //            continue;
            //        }
            //    }
            //}
            //// Append last point to path.
            //if (npts < maxPts)
            //{
            //    vcpy( &pts[npts * 2], &portals[(nportals - 1) * 4 + 0] );
            //    npts++;
            //}

            //return npts;
        }

        private static void PushPathVertex( NativeArray<float3> pathVerticesContainer, ref int currentPathVertexIndex, float3 startPosition )
        {
            if (currentPathVertexIndex < pathVerticesContainer.Length)
            {
                pathVerticesContainer[currentPathVertexIndex] = startPosition;
                currentPathVertexIndex++;
            }
        }
    }
}
