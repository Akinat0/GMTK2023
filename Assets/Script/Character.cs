using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


public class Character : MonoBehaviour
{
    [SerializeField] ParamsContainer paramsContainer;
    
    public ParamsContainer runtimeParamsContainer;

    Action OnSuccess { get; set; }
    Action OnFail { get; set; }

    GridController Grid { get; set; }

    readonly HashSet<GridItem> visitedGrids = new HashSet<GridItem>();
    readonly Stack<GridItem> history = new Stack<GridItem>();

    GridItem LastRoom { get; set; }

    public void StartRun(GridController grid, GridItem startRoom, GridItem lastRoom, Action onSuccess, Action onFail)
    {
        history.Clear();
        visitedGrids.Clear();
        
        OnSuccess = onSuccess;
        OnFail = onFail;

        LastRoom = lastRoom;

        runtimeParamsContainer = new ParamsContainer(paramsContainer); 
        
        StartRoom(startRoom, null);
    }

    void StartRoom(GridItem room, GridItem prevRoom)
    {
        void NextRoom()
        {
            visitedGrids.Add(room);

            runtimeParamsContainer.Apply(room.Params);

            if (runtimeParamsContainer.Hp <= 0)
            {
                OnFail?.Invoke();
                return;
            }
            
            if (room == LastRoom)
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
                
            StartRoom(nextRoom, room);
        }

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(room.transform.position, 0.5f)).OnComplete(NextRoom);
    }
    
}
