using UnityEngine;

/// <summary>
/// Inherit from this on a compnent that you wish
/// to have the player be able to lock on to
/// </summary>
/// <typeparam name="T">Pass in the type that you inherit to. Must be a Monobehaviour</typeparam>
public interface ITargetable
{
    float TargetOffset { get; }
    Vector3 TargetPosition();

    bool IsTargetable();
}