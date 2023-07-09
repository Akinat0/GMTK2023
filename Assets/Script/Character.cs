using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


public class Character : MonoBehaviour
{
    public ParamsContainer paramsContainer;
    
    public ParamsContainer runtimeParamsContainer;

    Action OnSuccess { get; set; }
    Action OnFail { get; set; }
    
    readonly HashSet<GridItem> visitedGrids = new ();
    readonly Stack<GridItem> history = new ();

    Dungeon Dungeon { get; set; }
    GridItem StartRoom { get; set; }

    public void StartRun(Dungeon dungeon, GridItem startRoom, Action onSuccess, Action onFail)
    {
        history.Clear();
        visitedGrids.Clear();
        
        OnSuccess = onSuccess;
        OnFail = onFail;
        
        Dungeon = dungeon;
        StartRoom = startRoom;

        runtimeParamsContainer = new ParamsContainer(paramsContainer); 
        
        BeginTheRoom(startRoom, null);
    }

    void BeginTheRoom(GridItem room, GridItem prevRoom)
    {
        void NextRoom()
        {
            visitedGrids.Add(room);
            
            room.DungeonOperation.Apply(Dungeon);
            runtimeParamsContainer.Apply(room.Params, Dungeon);

            if (runtimeParamsContainer.Hp <= 0)
            {
                OnFail?.Invoke();
                return;
            }
            
            if (room.PortalsCount == 1 && room != StartRoom)
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

        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(transform.DOMove(room.transform.position, 0.5f))
            .Join(transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 0))
            .OnComplete(NextRoom);

        SoundManager.PlaySound("walk");
       
    }
    
}
