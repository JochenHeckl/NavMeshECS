using System.Linq;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Experimental.AI;
using System;
using UnityEngine;
using Unity.Mathematics;

namespace de.JochenHeckl.NavMeshECS
{
    public class QueryPathSystem : JobComponentSystem
    {
        private const int UnassignedSlotIndex = -1;
        private const int PathNodePoolSizeDefault = 100;
        private const int MaxIterationsPerUpdateDefault = 100;
        private const int MaxNumParallelQueriesDefault = 4;

        public struct QueryPathSystemConfig
        {
            public int PathNodePoolSize;
            public int MaxIterationsPerUpdate;
            public int MaxNumParallelQueries;

            public NativeArray<float>[] AreaCosts { get; set; }
        }

        private NativeArray<NavMeshQuery> queries;
        private NativeQueue<int> idleQueries;

        private QueryPathSystemConfig pendingConfig = new QueryPathSystemConfig();
        private QueryPathSystemConfig activeConfig = new QueryPathSystemConfig();
        private bool reconfigurationPending = true;

        public void UpdateSystemConfiguration( QueryPathSystemConfig configIn )
        {
            pendingConfig = configIn;
            reconfigurationPending = true;
        }

        protected override void OnCreate()
        {
            UpdateSystemConfiguration( new QueryPathSystemConfig()
            {
                PathNodePoolSize = PathNodePoolSizeDefault,
                MaxIterationsPerUpdate = MaxIterationsPerUpdateDefault,
                MaxNumParallelQueries = MaxNumParallelQueriesDefault,
            } ) ;
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            if (reconfigurationPending && (queries.Count() == idleQueries.Count))
            {
                reconfigurationPending = false;
                activeConfig = pendingConfig;
                pendingConfig = default( QueryPathSystemConfig );

                Reconfigure();
            }
            
            return Entities.ForEach( ( ref QueryPathRequest request ) =>
            {
                if (request.isDone)
                {
                    return;
                }

                if (request.assignedSlot == UnassignedSlotIndex)
                {
                    if (!idleQueries.TryDequeue( out request.assignedSlot ))
                    {
                        // there is no free queue to process this request
                        return;
                    }

                    var startLocation = queries[request.assignedSlot].MapLocation( request.startPosition, request.mapPositionExtents, request.agentTypeId, request.areaMask );
                    var destinationLocation = queries[request.assignedSlot].MapLocation( request.destinationPosition, request.mapPositionExtents, request.agentTypeId, request.areaMask );

                    var areaCosts = activeConfig.AreaCosts.Length > request.areaCostIndex ? activeConfig.AreaCosts[request.areaCostIndex] : default( NativeArray<float> );
                    request.queryStatus = queries[request.assignedSlot].BeginFindPath( startLocation, destinationLocation, request.areaMask, areaCosts );
                }

                if (request.queryStatus == PathQueryStatus.InProgress)
                {
                    request.queryStatus = queries[request.assignedSlot].UpdateFindPath( activeConfig.MaxIterationsPerUpdate, out var _ );
                }

                if (request.queryStatus == PathQueryStatus.InProgress)
                {
                    return;
                }

                if (request.queryStatus == PathQueryStatus.Success)
                {
                    request.queryStatus = queries[request.assignedSlot].EndFindPath( out var foundPathSize );

                    if ((request.queryStatus == PathQueryStatus.Success) || (request.queryStatus == PathQueryStatus.PartialResult))
                    {
                        var pathPolygons = new NativeArray<PolygonId>( foundPathSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory );
                        var numNodes = queries[request.assignedSlot].GetPathResult( pathPolygons );

                        if (numNodes > 1)
                        {
                            var portals = new (Vector3 left, Vector3 right)[numNodes - 1];

                            for (var polygonIdx = 0; polygonIdx < numNodes - 1; ++polygonIdx)
                            {
                                queries[request.assignedSlot].GetPortalPoints(
                                    pathPolygons[polygonIdx],
                                    pathPolygons[polygonIdx + 1],
                                    out portals[polygonIdx].left,
                                    out portals[polygonIdx].right );

                                var path = SSFASolver.Solve(
                                    request.startPosition,
                                    request.destinationPosition,
                                    portals.Select( x => ((float3) x.left, (float3) x.right) ).ToArray() );
                            }
                        }

                    }
                }

                request.isDone = true;
                idleQueries.Enqueue( request.assignedSlot );

            } )
            .Schedule( inputDeps );
        }

        private void Reconfigure()
        {
            queries = new NativeArray<NavMeshQuery>( 
                Enumerable.Range( 0, activeConfig.MaxNumParallelQueries )
                .Select( x =>
                {
                    idleQueries.Enqueue( x );
                    return new NavMeshQuery( NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, activeConfig.PathNodePoolSize );
                } )
                .ToArray(),
                Allocator.Persistent );
        }
    }
}
