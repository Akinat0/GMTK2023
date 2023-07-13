using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [field: SerializeField] public SelectionBox SelectionBox { get; private set; }
    [field: SerializeField] public LeanDragCamera DragCamera { get; private set;}
    [field: SerializeField] public GridController Grid { get; private set; }
    [field: SerializeField] public LeanSelectByFinger SelectByFinger { get; private set; }
    [field: SerializeField] public LeanFingerDown FingerDown { get; private set; }
    

    readonly Dictionary<Type, PlayerInputState> states = new Dictionary<Type, PlayerInputState>();
    PlayerInputState currentState;

    IEnumerator Start()
    {
        yield return null;
        
        states.Add(typeof(CommonPlayerInputState), new CommonPlayerInputState(this));
        states.Add(typeof(BoxSelectionPlayerInputState), new BoxSelectionPlayerInputState(this));
        states.Add(typeof(MultiselectPlayerInputState), new MultiselectPlayerInputState(this));
        states.Add(typeof(SpectatePlayerInputState), new SpectatePlayerInputState(this));
        SetState<CommonPlayerInputState>();
    }
    
    void LateUpdate()
    {
        currentState?.Update();
    }


    public void SetState<TState>() where TState : PlayerInputState
    {
        currentState?.Disable();

        Debug.Log($"[Input] Active state is {typeof(TState).Name}");
        
        currentState = states[typeof(TState)];
        
        currentState.Enable();
    }
}