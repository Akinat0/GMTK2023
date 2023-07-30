using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;


public class Character : MonoBehaviour
{
    public ParamsContainer paramsContainer;

    public ParamsContainer runtimeParamsContainer;

    readonly HashSet<GridItem> visitedGrids = new();
    readonly Stack<GridItem> history = new();
    readonly List<GridItem> exits = new();

    public GridItem ActiveRoom { get; private set; }
    public GridItem StartRoom { get; private set; }

    Dungeon Dungeon { get; set; }

    [SerializeField] TextMeshPro damage;
    [SerializeField] Color red;
    [SerializeField] Color green;

    void Start()
    {
        runtimeParamsContainer = new ParamsContainer(paramsContainer);
    }

    public void Initialize(Dungeon dungeon, GridItem startRoom)
    {
        Dungeon = dungeon;
        StartRoom = startRoom;
        ActiveRoom = startRoom;
    }
    
    public void Reset()
    {
        history.Clear();
        visitedGrids.Clear();
        
        StartRoom = ActiveRoom;
        
        history.Push(StartRoom);
        visitedGrids.Add(StartRoom);
        StartRoom.UseItem();
    }

    public void NextRoom(Action onFinished)
    {
        UpdateExitsList(ActiveRoom);

        var nextRoom = exits[Random.Range(0, exits.Count)];
        
        void Finished()
        {
            ProcessRoom(nextRoom);
            ActiveRoom = nextRoom;
            onFinished?.Invoke();
        }

        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(transform.DOMove(nextRoom.transform.position, 0.5f))
            .Join(transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 0))
            .OnComplete(Finished);

        SoundManager.PlaySound("walk");
    }

    void ProcessRoom(GridItem room)
    {
        damage.text = string.Empty; //clean text

        if (!room.IsUsed)
            ApplyRoom(room);

        visitedGrids.Add(room);
    }

    void ApplyRoom(GridItem room)
    {
        room.DungeonOperation.Apply(Dungeon);
        runtimeParamsContainer.Apply(room.Params, Dungeon);
        room.UseItem();

        if (room.Params.Hp != 0)
        {
            string toDisplay = room.Params.Hp > 0
                ? "+" + room.Params.Hp * Dungeon.Multiplier
                : room.Params.Hp * Dungeon.Multiplier + "";
            Color toDisplayColor = room.Params.Hp > 0 ? green : red; // change to palette colors
            damage.text = toDisplay;
            damage.color = toDisplayColor;
            damage.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 0); //idk if this one working or not
        }
        else damage.text = string.Empty;
    }

    void UpdateExitsList(GridItem room)
    {
        exits.Clear();

        foreach (var direction in GridItem.Neighbours)
        {
            var neighbour = room.GetNeighbour(direction);

            if (neighbour != null && !visitedGrids.Contains(neighbour))
                exits.Add(neighbour);
        }

        if (exits.Count == 0)
            exits.Add(history.Pop());
        else
            history.Push(room);
    }
}