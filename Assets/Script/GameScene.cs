using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Abu.Tools;
using DG.Tweening;
using Script;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameScene : MonoBehaviour
{
    static GameScene Instance { get; set; }

    [SerializeField] GridController grid;
    [SerializeField] Character roguePrefab;
    [SerializeField] Character minotaurPrefab;
    [SerializeField] GridItem gridItemPrefab;
    [SerializeField] LevelConfig levelConfig;
    [SerializeField] MainCamera mainCamera;

    Character rogueCharacter;
    Character minotaurCharacter;

    Dungeon dungeon;

    public static GridController Grid => Instance != null ? Instance.grid : null;
    public static Character Character => Instance != null ? Instance.rogueCharacter : null;
    public static MainCamera Camera => Instance != null ? Instance.mainCamera : null;
    public static GridPathCost PathCost => Instance != null ? Instance.pathCost : null;
    public static int UnlockableCount { get; set; }

    public static event Action OnStartRun;
    public static event Action OnEndRun;

    //UI DISPLAY
    [SerializeField] TextMeshProUGUI displayHp;
    [SerializeField] TextMeshProUGUI minotaurDisplayHp;
    [SerializeField] TextMeshProUGUI multiplierField;
    [SerializeField] TextMeshProUGUI unlockablesCountField;
    [SerializeField] TextMeshProUGUI[] roomsCountField;
    [SerializeField] Slider sliderHp;
    [SerializeField] Slider minotaurSliderHp;
    [SerializeField] GameObject win;
    [SerializeField] Button startButton;

    GridPathCost pathCost;

    bool started;

    void Awake()
    {
        Instance = this;
        Grid.Build(50, 50);
        pathCost = new GridPathCost(Grid);

        dungeon = new Dungeon();
        
        AddTiles(levelConfig.StartTileSet, 2, 2, SpawnCharacters);

        multiplierField.transform.DOScale(Vector3.one * 1.07f, 2f).SetLoops(-1, LoopType.Yoyo);
        
        startButton.onClick.AddListener(StartGame);
    }

    void SpawnCharacters()
    {
        GridItem[] startRooms = Grid.Items.Where(item => item != null && item.IsFireplace).Take(2).ToArray();

        rogueCharacter = SpawnCharacter(startRooms[0], roguePrefab);
        minotaurCharacter = SpawnCharacter(startRooms[1], minotaurPrefab);
    }
    
    Character SpawnCharacter(GridItem startRoom, Character characterPrefab)
    {
        Character character = Instantiate(characterPrefab, startRoom.transform.position, Quaternion.identity);
        character.transform.localScale = Vector3.zero;
        character.transform.DOScale(Vector3.one, 0.2f);
        character.Initialize(dungeon, startRoom);
        
        Grid.SetLockedOnGrid(startRoom.X, startRoom.Y, true);

        return character;
    }

    void ChangeFireplace()
    {
        PathCost.Evaluate(rogueCharacter.ActiveRoom);
        
        foreach (var item in Grid.Items.Where(item => item != null))
            item.PathCost = item.PathCost * item.PathCost * item.EmptyNeighboursCount;

        GridItem fireplaceItem = Grid.Items.First(item => item != null && item != rogueCharacter.ActiveRoom && item.IsFireplace);
        
        var items = Grid.Items.Where(item => item != null && !item.IsFireplace && item.EmptyNeighboursCount > 0).ToArray();
        var weights = items.Select(item => item.PathCost).ToArray();

        GridItem selectedPoint = items[Utility.GetRandomWeightedIndex(weights)];

        if(selectedPoint.TryGetEmptyNeighbourCoord(out int x, out int y))
            Grid.TryPlaceItem(fireplaceItem, x, y);
    }
    
    void AddTiles(TileSet tileSet, int xOffset = 0, int yOffset = 0, Action onComplete = null)
    {
        int count = tileSet.AllowedTiles.Count;

        void OnComplete()
        {
            count--;

            if (count == 0)
                onComplete?.Invoke();
        }
        
        foreach (TileSet.Entrance tileEntrance in tileSet.AllowedTiles)
            PlaceItem(tileEntrance, xOffset, yOffset, OnComplete);
    }

    int roomsCount = 0;
    float multiplier = 1;
    
    void Update()
    {
        startButton.interactable = !started;
        unlockablesCountField.text = $"Unlock:{UnlockableCount}";

        if (!started)
            return;
        
        // displayHp.text = $"HP:{rogueCharacter.runtimeParamsContainer.Hp:#.#}";
        displayHp.text = $"HP:{Mathf.Ceil(rogueCharacter.runtimeParamsContainer.Hp)}";
        sliderHp.value = rogueCharacter.runtimeParamsContainer.Hp/rogueCharacter.paramsContainer.Hp;
        
        minotaurDisplayHp.text = $"HP:{Mathf.Ceil(minotaurCharacter.runtimeParamsContainer.Hp)}";
        minotaurSliderHp.value = minotaurCharacter.runtimeParamsContainer.Hp/minotaurCharacter.paramsContainer.Hp;

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

    void PlaceItem(TileSet.Entrance entrance, int xOffset = 0, int yOffset = 0, Action onComplete = null)
    {
        for (int i = yOffset; i < grid.Height; i++)
        {
            for (int j = xOffset; j < grid.Width; j++)
            {
                if(grid.IsBusyCell(j, i))
                    continue;

                GridItem item = Instantiate(gridItemPrefab);

                if (grid.TryPlaceItem(item, j, i, onComplete))
                {
                    item.Initialize(entrance.ParamsContainer, entrance.Operation, entrance.IsRed, entrance.IsFireplace, entrance.PortalsCount);
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
        if(rogueCharacter == null)
            return;

        if(!CheckIsValid())
            return;
        
        Grid.SetLockedOnGridAll(true);

        OnStartRun?.Invoke();

        StartCoroutine(PlayRoutine());
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

        CheckIsValidItem(rogueCharacter.ActiveRoom, checkedItems);

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

    void CheckIsValidItem(GridItem item, HashSet<GridItem> checkedItems)
    {
        if (checkedItems.Contains(item))
            return; 
        
        checkedItems.Add(item);
        
        var neighbours = GridItem.Neighbours.Select(item.GetNeighbour)
            .Where(neighbour => neighbour != null && !checkedItems.Contains(neighbour));

        foreach (var neighbour in neighbours)
            CheckIsValidItem(neighbour, checkedItems);
    }

    Vector2Int GetTilesSpawnOffset()
    {
        Vector2 leftBottomCorner = Camera.CameraRect.min;

        if (!Grid.TryGetCellCoordinates(leftBottomCorner, out int x, out int y))
            return Vector2Int.zero;

        return new Vector2Int(x + 1, y + 1);
    }

    IEnumerator PlayRoutine()
    {
        started = true;
        rogueCharacter.Reset();
        minotaurCharacter.Reset();
        
        while (true)
        {
            bool canContinue = false;

            DelegateGroup finished = new (2, () => canContinue = true);
            
            rogueCharacter.NextRoom(finished.Invoke);
            minotaurCharacter.NextRoom(finished.Invoke);
            dungeon.RoomsCount++;
            yield return new WaitUntil(() => canContinue);

            if (rogueCharacter.runtimeParamsContainer.Hp <= 0)
            {
                OpenWinScreen();
                break;
            }

            if (rogueCharacter.ActiveRoom.IsFireplace && rogueCharacter.ActiveRoom != rogueCharacter.StartRoom)
            {
                Continue();
                break;
            }
        }

        started = false;
    }
    
    //Player is still playing 
    void Continue()
    {
        foreach (var item in Grid.Items.Where(item => item != null))
            item.ResetItem();
        
        Grid.SetMovableAll(true);
        Grid.SetLockedOnGridAll(true);

        rogueCharacter.transform.DOScale(Vector3.one * 1.2f, 0.4f).SetLoops(-1, LoopType.Yoyo);

        started = false;
        
        TileSet config = levelConfig.RandomTileSets.ElementAt(Random.Range(0, levelConfig.RandomTileSets.Count));

        Vector2Int tilesOffset = GetTilesSpawnOffset();
        
        ChangeFireplace();
        UnlockableCount += 2 + Mathf.CeilToInt((float)Grid.Items.Count(item => item != null) / 10);
        
        AddTiles(config, tilesOffset.x, tilesOffset.y);

        OnEndRun?.Invoke();
    }

    //Player won 
    void OpenWinScreen()
    {
        rogueCharacter.transform.DOScale(Vector3.zero, 0.2f);
        win.SetActive(true);
        started = false;
    }
}