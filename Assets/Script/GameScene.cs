using System.Linq;
using DG.Tweening;
using Lean.Common;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    public static GameScene Instance { get; private set; }

    [SerializeField] GridController grid;
    [SerializeField] Character characterPrefab;
    [SerializeField] LevelConfig[] levelConfig;
    [SerializeField] LeanConstrainToCollider cameraConstraint;

    public Character character;

    Dungeon dungeon;

    GridItem startRoom;
    
    public static GridController Grid => Instance != null ? Instance.grid : null;

    //UI DISPLAY
    [SerializeField] TextMeshProUGUI displayHp, displayLvl;
    [SerializeField] TextMeshProUGUI multiplierField;
    [SerializeField] TextMeshProUGUI[] roomsCountField;
    [SerializeField] Slider sliderHp, sliderExp;
    [SerializeField] GameObject win;
    public bool started;
    int levelIndex;

    void Start()
    {
        Instance = this;
        Grid.Build(levelConfig[0].FieldSize.x, levelConfig[0].FieldSize.y);

        cameraConstraint.Collider = Grid.CameraBounds;
        AddTiles();
    }

    void AddTiles()
    {
        LevelConfig config = levelIndex < levelConfig.Length 
            ? levelConfig[levelIndex] 
            : levelConfig[Random.Range(0, levelConfig.Length)];
        
        foreach (var tileEntrance in config.AllowedTiles)
        {
            GridItem item = Instantiate(tileEntrance.item);
            item.DungeonOperation = tileEntrance.operation; 
            item.Params = tileEntrance.paramsContainer;
            item.IsRed = tileEntrance.isRed;
            PlaceItem(item);
            item.PortalsCount = tileEntrance.portalsCount;
        }
        
        levelIndex++;
    }

    int roomsCount = 0;
    float multiplier = 1;
    
    private void Update()
    {
        if (started)
        {
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
                    multiplierField.text = $"X{dungeon.Multiplier:#.##}";
                    multiplierField.transform.DOPunchScale(Vector3.one * 0.1f, 0.15f, 1);
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
    }

    void PlaceItem(GridItem item)
    {
        if (dungeon == null)
        {
            for (int i = 2; i < grid.Height; i++)
            {
                for (int j = 2; j < grid.Width; j++)
                {
                    if (grid.TryPlaceItem(item, j, i))
                        return;
                }
            }
            return;
        }
        
        for (int i = 0; i < grid.Height; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                if (grid.TryPlaceItem(item, j, i))
                    return;
            }
        }
    }
    
    void OnDestroy()
    {
        Instance = null;
    }

    public void StartGame()
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
        
        if(hasNotValid)
            return;

        foreach (var item in Grid.Items.Where(item => item != null))
            item.ResetItem();

        dungeon ??= new Dungeon();
        
        if (startRoom == null)
        {
            var startRooms = Grid.Items.Where(item => item != null && item.PortalsCount == 1).ToArray();
            startRoom = startRooms[Random.Range(0, startRooms.Length)];
            
            character = Instantiate(characterPrefab);
            character.transform.position = startRoom.transform.position;
            character.transform.localScale = Vector3.zero;
            started = true;
            character.transform.DOScale(Vector3.one, 0.2f).OnComplete(StartRun);
        }
        else
        {
            startRoom = character.ActiveRoom;
            StartRun();
        }
        

        void StartRun()
        {
            character.StartRun(dungeon, startRoom, Win, Lose);
            started = true;
        }
    }

    void Win()
    {
        character.transform.DOScale(Vector3.one * 1.2f, 0.4f).SetLoops(-1, LoopType.Yoyo);

        
        AddTiles();
    }

    void Lose()
    {
        character.transform.DOScale(Vector3.zero, 0.2f);
        win.SetActive(true);
        Debug.Log("Lose :(");
        
        started = false;
        
        AddTiles();
    }
}