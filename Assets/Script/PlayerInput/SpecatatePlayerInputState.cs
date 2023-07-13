using UnityEngine;

public class SpectatePlayerInputState : PlayerInputState
{
    public SpectatePlayerInputState(PlayerInput playerInput) : base(playerInput)
    {
    }

    public override void Enable()
    {
        PlayerInput.SelectByFinger.DeselectAll();
        
        PlayerInput.DragCamera.enabled = true;
        PlayerInput.FingerDown.enabled = false;
        
        GameScene.OnEndRun += HandleEndRun;
    }

    public override void Update()
    {
    }
    
    public override void Disable()
    {
        GameScene.OnEndRun -= HandleEndRun;
    }
    
    void HandleEndRun()
    {
        SetState<CommonPlayerInputState>();
    }
}