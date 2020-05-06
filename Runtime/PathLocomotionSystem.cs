using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace de.JochenHeckl.NavMeshECS
{

    [UpdateAfter( typeof( QueryPathSystem ) )]
    public class PathLocomotionSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var deltaTime = Time.DeltaTime;

            return Entities
                .ForEach( ( ref Translation translation, ref PathLocomotionData pathLocomotionData, ref DynamicBuffer<PathVertexResultData> path ) =>
            {
                if( pathLocomotionData.destinationReached )
                {
                    return;
                }

                float travelDistance = deltaTime * pathLocomotionData.maxVelocity;

                pathLocomotionData.destinationReached =
                    ( path.Length <= pathLocomotionData.nextPathVertexIndex)
                    || TestInRange( translation.Value, path[path.Length - 1].Value, travelDistance );

                if (pathLocomotionData.destinationReached)
                {
                    translation.Value = path[path.Length - 1];
                    return;
                }

                var nextdestination = path[pathLocomotionData.nextPathVertexIndex];

                if (TestInRange( translation.Value, nextdestination.Value, travelDistance ))
                {
                    travelDistance -= math.distance(nextdestination.Value, translation.Value);

                    pathLocomotionData.nextPathVertexIndex++;
                    nextdestination = path[pathLocomotionData.nextPathVertexIndex];
                }

                if (!TestInRange( translation.Value, nextdestination.Value, travelDistance ))
                {
                    var towardsNextDestination = nextdestination.Value - translation.Value;
                    var offset = math.normalize( towardsNextDestination ) * travelDistance;

                    translation.Value += offset;
                }
            } )
            .Schedule( inputDeps );
        }

        private static bool TestInRange( float3 one, float3 other, float distance )
        {
            return math.distancesq( one, other ) < (distance * distance);
        }
    }
}