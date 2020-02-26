using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Experimental.AI;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace de.JochenHeckl.NavMeshECS
{
    public class QueryPathSystem : ComponentSystem
    {
        private const int PathNodePoolSizeDefault = 100;
        private const int MaxIterationsPerUpdateDefault = 100;
        private const int MaxNumParallelQueriesDefault = 4;
        private const int MaxPathLength = 100;

        private UpdateNavMeshQueryJob[] activeJobs = new UpdateNavMeshQueryJob[] { };
        private Queue<NavMeshQuery> idleQueries = new Queue<NavMeshQuery>();

        private bool reconfigurationPending;
        private int maxNumParallelQueries = MaxNumParallelQueriesDefault;

        private float[][] areaCostsMapping = new float[][] { };

        protected override void OnCreate()
        {
            base.OnCreate();

            reconfigurationPending = true;
        }

        protected override void OnDestroy()
        {
            foreach (var query in activeJobs.Select( x => x.navMeshQuery ).Concat( idleQueries ))
            {
                query.Dispose();
            }

            foreach (var job in activeJobs)
            {
                if (job.areaCosts.IsCreated)
                {
                    job.areaCosts.Dispose();
                }

                if (job.updateResult.IsCreated)
                {
                    job.updateResult.Dispose();
                }

                if (job.pathResult.IsCreated)
                {
                    job.pathResult.Dispose();
                }
            }

            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var allQueriesIdle = activeJobs.Length == 0;

            if (reconfigurationPending && allQueriesIdle)
            {
                Reconfigure();
                reconfigurationPending = false;
            }

            var newActiveJobs = new List<UpdateNavMeshQueryJob>();

            Entities.ForEach( ( Entity entity, ref QueryPathRequestData request ) =>
            {
                if (idleQueries.Any())
                {
                    newActiveJobs.Add( new UpdateNavMeshQueryJob()
                    {
                        navMeshQuery = idleQueries.Dequeue(),
                        queryPathRequest = request,
                        queryingEntity = entity,
                        isFirstUpdate = true,
                        numIterationsPerUpdate = MaxIterationsPerUpdateDefault,
                        areaCosts = MakeAreaCosts( request.areaCostIndex ),
                        updateResult = new NativeArray<UpdateNavMeshQueryJob.UpdateResult>( 1, Allocator.Persistent ),
                        pathResult = new NativeArray<float3>( MaxPathLength, Allocator.Persistent )
                    } );
                }

                PostUpdateCommands.RemoveComponent<QueryPathRequestData>( entity );
            } );

            activeJobs = activeJobs.Concat( newActiveJobs ).ToArray();
            var activeJobHandles = activeJobs.Select( x => x.Schedule() ).ToArray();

            // wait for the jobs to finish
            using (var handles = new NativeArray<JobHandle>( activeJobHandles, Allocator.Temp ))
            {
                var combinedJobs = JobHandle.CombineDependencies( handles );
                NavMeshWorld.GetDefaultWorld().AddDependency( combinedJobs );
                combinedJobs.Complete();
            }

            var jobsToContinue = new List<int>();

            for (var activeJobIdx = 0; activeJobIdx < activeJobs.Length; ++activeJobIdx)
            {
                var updateResult = activeJobs[activeJobIdx].updateResult[0];

                if (updateResult.queryStatus != PathQueryStatus.InProgress)
                {
                    if ((updateResult.queryStatus == PathQueryStatus.Success) || (updateResult.queryStatus == PathQueryStatus.PartialResult))
                    {
                        var buffer = PostUpdateCommands.AddBuffer<QueryPathResultData>( activeJobs[activeJobIdx].queryingEntity );

                        buffer.ResizeUninitialized( updateResult.pathResultLength );

                        for (int resultVertexIndex = 0; resultVertexIndex < updateResult.pathResultLength; resultVertexIndex++)
                        {
                            buffer[resultVertexIndex] = activeJobs[activeJobIdx].pathResult[resultVertexIndex];
                        }
                    }

                    if (activeJobs[activeJobIdx].areaCosts.IsCreated)
                    {
                        activeJobs[activeJobIdx].areaCosts.Dispose();
                    }

                    if (activeJobs[activeJobIdx].pathResult.IsCreated)
                    {
                        activeJobs[activeJobIdx].pathResult.Dispose();
                    }

                    if (activeJobs[activeJobIdx].updateResult.IsCreated)
                    {
                        activeJobs[activeJobIdx].updateResult.Dispose();
                    }

                    idleQueries.Enqueue( activeJobs[activeJobIdx].navMeshQuery );
                }
                else
                {
                    activeJobs[activeJobIdx].isFirstUpdate = false;
                    jobsToContinue.Add( activeJobIdx );
                }
            }

            activeJobs = jobsToContinue.Select( x => activeJobs[x] ).ToArray();
        }

        private NativeArray<float> MakeAreaCosts( int areaCostIndex )
        {
            if (areaCostsMapping.Length > areaCostIndex)
            {
                return new NativeArray<float>( areaCostsMapping[areaCostIndex], Allocator.TempJob );
            }

            return new NativeArray<float>( Enumerable.Repeat( 1f, 32 ).ToArray(), Allocator.TempJob );
        }

        private void Reconfigure()
        {
            foreach (var query in idleQueries)
            {
                query.Dispose();
            }

            idleQueries = new Queue<NavMeshQuery>(
                Enumerable.Range( 0, maxNumParallelQueries )
                .Select( x => new NavMeshQuery( NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, PathNodePoolSizeDefault ) ) );
        }
    }
}
