using UnityEngine;

public interface IMovement
{
    Pathfollowing pathfollowing { get; set; }

    void CheckFacing(Vector2 velocity);

    void Flip();
}
