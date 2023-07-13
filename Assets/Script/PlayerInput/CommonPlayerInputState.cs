using System.Linq;
using UnityEngine;

public class CommonPlayerInputState : PlayerInputState
{
    public CommonPlayerInputState(PlayerInput playerInput) : base(playerInput)
    {
    }

    public override void Enable()
    {
        PlayerInput.SelectByFinger.DeselectAll();
        
        PlayerInput.SelectionBox.enabled = false;
        PlayerInput.DragCamera.enabled = true;
        PlayerInput.Grid.SetMovableAll(true);
        PlayerInput.SelectByFinger.DeselectWithFingers = true;
        PlayerInput.DragCamera.enabled = true;
        PlayerInput.FingerDown.enabled = true;


        PlayerInput.Grid.OnSelectFingerDown += OnSelectedFingerHandler;
        PlayerInput.Grid.OnSelectFingerUp += OnSelectedFingerUpHandler;
        
        GameScene.OnStartRun += HandleStartRun;
    
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            SetState<BoxSelectionPlayerInputState>();
    }
    
    public override void Disable()
    {
        PlayerInput.Grid.OnSelectFingerDown -= OnSelectedFingerHandler;
        PlayerInput.Grid.OnSelectFingerUp -= OnSelectedFingerUpHandler;
        
        GameScene.OnStartRun -= HandleStartRun;
    }

    void HandleStartRun()
    {
        SetState<SpectatePlayerInputState>();
    }
    
    void OnSelectedFingerHandler(GridItem item)
    {
        PlayerInput.DragCamera.enabled = false;
        
        if(!item.IsMovable)
            return;

        PlayerInput.Grid.DetachItem(item.X, item.Y); //detach this
    }

    void OnSelectedFingerUpHandler(GridItem item)
    {
        PlayerInput.DragCamera.enabled = true;
        
        if(!item.IsMovable)
            return;
        
        if (!PlayerInput.Grid.TryGetCellCoordinates(item.transform.position, out int x, out int y)
            || !PlayerInput.Grid.TryPlaceItem(item, x, y))
        {
            PlayerInput.Grid.TryPlaceItem(item, item.X, item.Y);
        }
    }
}