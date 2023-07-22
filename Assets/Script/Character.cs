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

    Action OnSuccess { get; set; }
    Action OnFail { get; set; }
    
    readonly HashSet<GridItem> visitedGrids = new ();
    readonly Stack<GridItem> history = new ();


    public GridItem ActiveRoom { get; set; }

    Dungeon Dungeon { get; set; }
    GridItem StartRoom { get; set; }

    [SerializeField] TextMeshPro damage;
    [SerializeField] Color red;
    [SerializeField] Color green;

    void Start()
    {
        runtimeParamsContainer = new ParamsContainer(paramsContainer);
    }

    public void StartRun(Dungeon dungeon, Action onSuccess, Action onFail)
    {
        history.Clear();
        visitedGrids.Clear();
        
        OnSuccess = onSuccess;
        OnFail = onFail;
        
        Dungeon = dungeon;

        if (GameScene.Grid.TryGetCellCoordinates(transform.position, out int x, out int y))
            StartRoom = GameScene.Grid.GetCellItem(x, y);
        

        BeginTheRoom(StartRoom, null);
    }

    void BeginTheRoom(GridItem room, GridItem prevRoom)
    {
        void NextRoom()
        {
            visitedGrids.Add(room);
            room.DisableItemContent();
            
            room.DungeonOperation.Apply(Dungeon);
            runtimeParamsContainer.Apply(room.Params, Dungeon);

            if (room.Params.Hp != 0)
            {
                string toDisplay = room.Params.Hp > 0 ? "+" + room.Params.Hp * Dungeon.Multiplier : room.Params.Hp * Dungeon.Multiplier + "";
                Color toDisplayColor = room.Params.Hp > 0 ? green : red; // change to palette colors
                damage.text = toDisplay;
                damage.color = toDisplayColor;
                damage.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 0); //idk if this one working or not
            }
            else damage.text = null;

            if (runtimeParamsContainer.Hp <= 0)
            {
                OnFail?.Invoke();
                return;
            }
            
            if (room.IsFireplace && room != StartRoom)
            {
                OnSuccess?.Invoke();
                return;
            }
            
            List<GridItem> exits = new();

            foreach (var direction in GridItem.Neighbours)
            {
                var neighbour = room.GetNeighbour(direction);
                
                if(neighbour != null && neighbour != prevRoom && !visitedGrids.Contains(neighbour))
                    exits.Add(neighbour);
            }
            
            if (exits.Count == 0)
            {
                GridItem backRoom;

            
                backRoom = history.Peek();

                foreach (var direction in GridItem.Neighbours)
                {
                    exits.Clear();

                    var neighbour = backRoom.GetNeighbour(direction);

                    if (neighbour != null && !visitedGrids.Contains(neighbour))
                        exits.Add(neighbour);
                }

                if (exits.Count == 0)
                    exits.Add(history.Pop());
            }
            else
            {
                history.Push(room);
            }

            var nextRoom = exits[Random.Range(0, exits.Count)];
                
            BeginTheRoom(nextRoom, room);
        }
        
        ActiveRoom = room;

        Dungeon.RoomsCount++;
        
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(transform.DOMove(room.transform.position, 0.5f))
            .Join(transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 0))
            .OnComplete(NextRoom);

        SoundManager.PlaySound("walk");
       
    }
    
}
