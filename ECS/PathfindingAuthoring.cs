using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PathfindingAuthoring : MonoBehaviour
{
    public int2 startingPosition;
    public int2 endingPosition;
    public int pathIndex;

    [SerializeField] private int2 position;

    public class Baker : Baker<PathfindingAuthoring>
    {
        public override void Bake(PathfindingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.WorldSpace);

            AddComponent(entity, new pathfinding.PathfindingParamsComponent
            {
                startingPosition = authoring.startingPosition,
                endingPosition = authoring.endingPosition,
            });

            AddComponent(entity, new pathfinding.PathIndexComponent
            {
                pathIndex = authoring.pathIndex
            });


            AddBuffer<pathfinding.PathPosition>(entity);

        }
    }
}
