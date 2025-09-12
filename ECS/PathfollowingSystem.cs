using pathfinding;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct PathfollowingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (
            pathBuffer,
            transform,
            pathIndexComponent)
            in SystemAPI.Query<
                DynamicBuffer<PathPosition>,
                RefRW<LocalTransform>,
                RefRW<PathIndexComponent>>())
        {
            if (pathIndexComponent.ValueRW.pathIndex >= 0)
            {
                int2 pathPosition = pathBuffer[pathIndexComponent.ValueRW.pathIndex].position;

                float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
                float3 moveDirection = math.normalizesafe(targetPosition - transform.ValueRW.Position);

                float moveSpeed = 3f;

                transform.ValueRW.Position += moveDirection * moveSpeed + Time.deltaTime;

                if (math.distance(transform.ValueRW.Position, targetPosition) < 0.1f)
                {
                    // Go to next index
                    pathIndexComponent.ValueRW.pathIndex--;
                }
            }
        }
    }
}
