using UnityEngine;

public interface ITransition
{
    Animator animator { get; set; }

    void SetTransitionAnimation(string trigger);
}