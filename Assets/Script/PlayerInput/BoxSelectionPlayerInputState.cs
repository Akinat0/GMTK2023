using UnityEngine;

public class BoxSelectionPlayerInputState : PlayerInputState
{
    public BoxSelectionPlayerInputState(PlayerInput playerInput) : base(playerInput)
    {
    }

    public override void Enable()
    {
        PlayerInput.SelectionBox.enabled = true;
        PlayerInput.DragCamera.enabled = false;
        PlayerInput.Grid.SetMovableAll(false);
        PlayerInput.SelectByFinger.DeselectWithFingers = false;
        
        GameScene.OnStartRun += HandleStartRun;
    }

    public override void Update()
    {
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(0))
        {
            if(PlayerInput.SelectByFinger.Selectables.Count == 0)
                SetState<CommonPlayerInputState>();
            else
                SetState<MultiselectPlayerInputState>();
        }
    }

    public override void Disable()
    {
        PlayerInput.SelectionBox.enabled = false;
        
        GameScene.OnStartRun -= HandleStartRun;
    }

    void HandleStartRun()
    {
        SetState<SpectatePlayerInputState>();
    }
}