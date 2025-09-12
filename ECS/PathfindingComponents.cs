using Unity.Entities;
using Unity.Mathematics;


namespace pathfinding
{
    public struct PathfindingParamsComponent : QG_IEnableComponent
    {
        public int2 startingPosition;
        public int2 endingPosition;
    }

    public struct PathIndexComponent : QG_IEnableComponent
    {
        public int pathIndex;
    }

    public struct PathPosition : IBufferElementData
    {
        public int2 position;
    }
}

