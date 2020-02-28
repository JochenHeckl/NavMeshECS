using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace de.JochenHeckl.NavMeshECS
{
    public struct UpdateNavMeshQueryJob : IJob
    {
        public struct UpdateResult
        {
            public PathQueryStatus queryStatus;
            public int pathResultLength;
        }

        public NavMeshQuery navMeshQuery;
        public Entity queryingEntity;
        public QueryPathRequestData queryPathRequest;
        public NativeArray<float> areaCosts;

        public NativeArray<UpdateResult> updateResult;
        public NativeArray<float3> pathResult;

        public int numIterationsPerUpdate;
        public bool isFirstUpdate;

        public void Execute()
        {
            var queryStatus = isFirstUpdate ? InitializeQuery() : updateResult[0].queryStatus;

            if (queryStatus == PathQueryStatus.InProgress)
            {
                queryStatus = navMeshQuery.UpdateFindPath( numIterationsPerUpdate, out var _ );
            }

            if (queryStatus == PathQueryStatus.Success)
            {
                queryStatus = navMeshQuery.EndFindPath( out var foundPathSize );

                if ((queryStatus == PathQueryStatus.Success) || (queryStatus == PathQueryStatus.PartialResult))
                {
                    var pathPolygons = new NativeArray<PolygonId>( foundPathSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory );
                    var numNodes = navMeshQuery.GetPathResult( pathPolygons );

                    if (numNodes == 1)
                    {
                        pathResult[0] = queryPathRequest.startPosition;
                        pathResult[1] = queryPathRequest.destinationPosition;

                        updateResult[0] = new UpdateResult()
                        {
                            queryStatus = queryStatus,
                            pathResultLength = 2
                        };
                    }
                    else
                    {
                        var portals = new (Vector3 left, Vector3 right)[numNodes - 1];

                        for (var polygonIdx = 0; polygonIdx < numNodes - 1; ++polygonIdx)
                        {
                            navMeshQuery.GetPortalPoints(
                                pathPolygons[polygonIdx],
                                pathPolygons[polygonIdx + 1],
                                out portals[polygonIdx].left,
                                out portals[polygonIdx].right );
                        }

                        updateResult[0] = new UpdateResult()
                        {
                            queryStatus = queryStatus,
                            pathResultLength = SSFASolver.SolveV2(
                                queryPathRequest.startPosition,
                                queryPathRequest.destinationPosition,
                                portals.Select( x => ((float3) x.left, (float3) x.right) ).ToArray(),
                                pathResult )
                        };
                    }
                }
            }
            else
            {
                updateResult[0] = new UpdateResult()
                {
                    queryStatus = queryStatus,
                    pathResultLength = 0
                };
            }
        }

        private PathQueryStatus InitializeQuery()
        {
            var startLocation = navMeshQuery.MapLocation(
                    queryPathRequest.startPosition,
                    queryPathRequest.mapPositionExtents,
                    queryPathRequest.agentTypeId,
                    queryPathRequest.areaMask );

            var destinationLocation = navMeshQuery.MapLocation(
                queryPathRequest.destinationPosition,
                queryPathRequest.mapPositionExtents,
                queryPathRequest.agentTypeId,
                queryPathRequest.areaMask );

            return navMeshQuery.BeginFindPath( startLocation, destinationLocation, queryPathRequest.areaMask );
        }
    }
}
