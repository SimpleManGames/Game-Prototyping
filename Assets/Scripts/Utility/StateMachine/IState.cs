/// <summary>
/// IState
/// Acts as slot for StateMachine
/// Inorder to use this just inherit from it
/// Then set the a StateMachine's CurrentState to the inherited class
/// </summary>
public interface IState
{
    StateMachine State { get; }

    /// <summary>
    /// Excutes when this is set to a StateMachine's CurrentState
    /// </summary>
    void Start();
    
    /// <summary>
    /// This must be called inside of a MonoBehaviour's Update
    /// </summary>
    void Update();

    /// <summary>
    /// Runs once this IState is no longer the CurrentState of a StateMachine
    /// </summary>
    void Exit();
}