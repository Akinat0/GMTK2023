using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] Character characterPrefab;
    [SerializeField] GridItem gridItemPrefab;
    [SerializeField] LevelConfig levelConfig;
    [SerializeField] MainCamera mainCamera;

    Character character;

    Dungeon dungeon;

    public static GridController Grid => Instance != null ? Instance.grid : null;
    public static Character Character => Instance != null ? Instance.character : null;
    public static MainCamera Camera => Instance != null ? Instance.mainCamera : null;

    public static event Action OnStartRun;
    public static event Action OnEndRun;

    //UI DISPLAY
    [SerializeField] TextMeshProUGUI displayHp;
    [SerializeField] TextMeshProUGUI multiplierField;
    [SerializeField] TextMeshProUGUI[] roomsCountField;
    [SerializeField] Slider sliderHp;
    [SerializeField] GameObject win;
    [SerializeField] Button startButton;

    bool started;
    int levelIndex;

    void Awake()
    {
        Instance = this;
        Grid.Build(50, 50);
        
        AddTiles(levelConfig.StartTileSet, 2, 2, SpawnCharacter);

        multiplierField.transform.DOScale(Vector3.one * 1.07f, 2f).SetLoops(-1, LoopType.Yoyo);
        
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
                
                item.IsFireplace = entrance.IsFireplace;
                item.DungeonOperation = entrance.Operation; 
                item.Params = entrance.ParamsContainer;
                item.IsRed = entrance.IsRed;

                if (grid.TryPlaceItem(item, j, i, onComplete))
                {
                    item.PortalsCount = entrance.PortalsCount;
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
        
        Grid.SetLockedOnGridAll(true);

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
        
        TileSet config = levelConfig.RandomTileSets.ElementAt(Random.Range(0, levelConfig.RandomTileSets.Count));

        Vector2Int tilesOffset = GetTilesSpawnOffset();
        AddTiles(config, tilesOffset.x, tilesOffset.y);
        
        OnEndRun?.Invoke();
    }

    //Player won 
    void Lose()
    {
        character.transform.DOScale(Vector3.zero, 0.2f);
        win.SetActive(true);
        started = false;
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

        CheckIsValidItem(character.ActiveRoom, checkedItems);

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
}