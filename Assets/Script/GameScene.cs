using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Common;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameScene : MonoBehaviour
{
    static GameScene Instance { get; set; }

    [SerializeField] GridController grid;
    [SerializeField] Character characterPrefab;
    [SerializeField] GridItem gridItemPrefab;
    [SerializeField] LevelConfig[] levelConfig;
    [SerializeField] LeanConstrainToCollider cameraConstraint;

    Character character;

    Dungeon dungeon;

    public static GridController Grid => Instance != null ? Instance.grid : null;
    public static Character Character => Instance != null ? Instance.character : null;

    public static event Action OnStartRun;
    public static event Action OnEndRun;

    //UI DISPLAY
    [SerializeField] TextMeshProUGUI displayHp, displayLvl;
    [SerializeField] TextMeshProUGUI multiplierField;
    [SerializeField] TextMeshProUGUI[] roomsCountField;
    [SerializeField] Slider sliderHp, sliderExp;
    [SerializeField] GameObject win;
    [SerializeField] Button startButton;

    bool started;
    int levelIndex;

    void Start()
    {
        Instance = this;
        Grid.Build(levelConfig[0].FieldSize.x, levelConfig[0].FieldSize.y);

        cameraConstraint.Collider = Grid.CameraBounds;
        AddTiles(2, 2, SpawnCharacter);

        multiplierField.transform.DOScale(Vector3.one * 1.1f, 2f).SetLoops(-1, LoopType.Yoyo);
        
        startButton.onClick.AddListener(StartGame);
    }

    void SpawnCharacter()
    {
        GridItem[] startRooms = Grid.Items.Where(item => item != null && item.NeighboursCount == 1).ToArray();
        GridItem startRoom = startRooms[Random.Range(0, startRooms.Length)];
            
        character = Instantiate(characterPrefab, startRoom.transform.position, Quaternion.identity);
        character.transform.localScale = Vector3.zero;
        character.transform.DOScale(Vector3.one, 0.2f);
        character.ActiveRoom = startRoom;
    }
    
    void AddTiles(int xOffset = 0, int yOffset = 0, Action onComplete = null)
    {
        LevelConfig config = levelIndex < levelConfig.Length 
            ? levelConfig[levelIndex] 
            : levelConfig[Random.Range(0, levelConfig.Length)];

        int count = config.AllowedTiles.Count;

        void OnComplete()
        {
            count--;

            if (count == 0)
                onComplete?.Invoke();
        }
        
        foreach (LevelConfig.Entrance tileEntrance in config.AllowedTiles)
            PlaceItem(tileEntrance, xOffset, yOffset, OnComplete);
        
        levelIndex++;
    }

    int roomsCount = 0;
    float multiplier = 1;
    
    void Update()
    {
        startButton.interactable = !started;

        if (!started)
            return;
        
        // displayHp.text = $"HP:{character.runtimeParamsContainer.Hp:#.#}";
        displayHp.text = $"HP:{Mathf.Ceil(character.runtimeParamsContainer.Hp)}";
        sliderHp.value = character.runtimeParamsContainer.Hp/character.paramsContainer.Hp;

        displayLvl.text = "LVL:" + character.runtimeParamsContainer.Lvl;
        sliderExp.value = (float)character.runtimeParamsContainer.Exp / 10;

        if (dungeon != null)
        {
            multiplierField.text = $"X{dungeon.Multiplier:#.##}";
                
            if (!Mathf.Approximately(multiplier, dungeon.Multiplier))
            {
                multiplier = dungeon.Multiplier;
                multiplierField.text = $"X{dungeon.Multiplier:#.#}";
                multiplierField.transform.DOPunchScale(Vector3.one * 0.3f, 0.15f, 1);
            }
                
            if (roomsCount != dungeon.RoomsCount)
            {
                roomsCount = dungeon.RoomsCount;

                foreach (var rooms in roomsCountField)
                {
                    rooms.text = $"Rooms:{dungeon.RoomsCount}";
                    rooms.transform.DOPunchScale(Vector3.one * 0.1f, 0.15f, 1);
                }
            }
        }
    }

    void PlaceItem(LevelConfig.Entrance entrance, int xOffset = 0, int yOffset = 0, Action onComplete = null)
    {
        for (int i = yOffset; i < grid.Height; i++)
        {
            for (int j = xOffset; j < grid.Width; j++)
            {
                if(grid.IsBusyCell(j, i))
                    continue;

                GridItem item = Instantiate(gridItemPrefab);
                
                item.DungeonOperation = entrance.operation; 
                item.Params = entrance.paramsContainer;
                item.IsRed = entrance.isRed;

                if (grid.TryPlaceItem(item, j, i, onComplete))
                {
                    item.PortalsCount = entrance.portalsCount;
                    return;
                }
            }
        }
    
    }
    
    void OnDestroy()
    {
        Instance = null;
    }

    public void StartGame()
    {
        if(character == null)
            return;

        if(!CheckIsValid())
            return;

        foreach (var item in Grid.Items.Where(item => item != null))
            item.ResetItem();

        dungeon ??= new Dungeon();
        
        StartRun();
    

        void StartRun()
        {
            OnStartRun?.Invoke();
            
            character.StartRun(dungeon, Win, Lose);
            started = true;
        }
    }

    //Player is still playing 
    void Win()
    {
        foreach (var item in Grid.Items.Where(item => item != null))
            item.ResetItem();
        
        Grid.SetMovableAll(true);
        character.ActiveRoom.IsMovable = false;
        
        character.transform.DOScale(Vector3.one * 1.2f, 0.4f).SetLoops(-1, LoopType.Yoyo);

        started = false;
        AddTiles();
        
        OnEndRun?.Invoke();
    }

    //Player won 
    void Lose()
    {
        character.transform.DOScale(Vector3.zero, 0.2f);
        win.SetActive(true);
        started = false;
        
        AddTiles();
    }

    bool CheckIsValid()
    {
        bool hasNotValid = false;
        
        foreach (var item in Grid.Items.Where(item => item != null))
        {
            if (!item.IsValid)
            {
                item.MarkAsNotValid();
                hasNotValid = true;
            }
        }

        HashSet<GridItem> checkedItems = new HashSet<GridItem>();

        CheckItem(character.ActiveRoom, checkedItems);

        foreach (GridItem separatedItem in grid.Items.Where(item => item != null && !checkedItems.Contains(item)))
        {
            hasNotValid = true;
            separatedItem.MarkAsNotValid();
        }

        if (hasNotValid)
        {
            SoundManager.PlaySound("wrong");
            return false;
        }

        return true;
    }

    void CheckItem(GridItem item, HashSet<GridItem> checkedItems)
    {
        if (checkedItems.Contains(item))
            return; 
        
        checkedItems.Add(item);
        
        var neighbours = GridItem.Neighbours.Select(item.GetNeighbour)
            .Where(neighbour => neighbour != null && !checkedItems.Contains(neighbour));

        foreach (var neighbour in neighbours)
            CheckItem(neighbour, checkedItems);
    }
}