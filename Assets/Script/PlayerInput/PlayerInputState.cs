
public abstract class PlayerInputState
{
    public PlayerInput PlayerInput { get; }

    public PlayerInputState(PlayerInput playerInput)
    {
        PlayerInput = playerInput;
    }
    
    protected void SetState<TState>() where TState : PlayerInputState
    {
        PlayerInput.SetState<TState>();
    }
    
    public abstract void Enable();
    public abstract void Update();
    public abstract void Disable();
}