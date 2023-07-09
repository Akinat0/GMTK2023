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
    
    public static GridController Grid => Instance != null ? Instance.grid : null;

    //UI DISPLAY
    [SerializeField] TextMeshProUGUI displayHp, displayLvl;
    [SerializeField] TextMeshProUGUI multiplierField;
    [SerializeField] Slider sliderHp, sliderExp;
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
        foreach (var tileEntrance in levelConfig[levelIndex].AllowedTiles)
        {
            GridItem item = Instantiate(tileEntrance.item);
            item.DungeonOperation = tileEntrance.operation; 
            item.Params = tileEntrance.paramsContainer;
            PlaceItem(item);
            item.PortalsCount = tileEntrance.portalsCount;
        }
        
        levelIndex++;
    }

    private void Update()
    {
        if (started)
        {
            displayHp.text = $"HP:{character.runtimeParamsContainer.Hp:#.#}";
            sliderHp.value = character.runtimeParamsContainer.Hp/character.paramsContainer.Hp;

            displayLvl.text = "LVL:" + character.runtimeParamsContainer.Lvl;
            sliderExp.value = (float)character.runtimeParamsContainer.Exp / 10;

            if(dungeon != null)
                multiplierField.text = $"X{dungeon.Multiplier:#.##}";
        }
    }

    void PlaceItem(GridItem item)
    {
        for (int i = 0; i < grid.Width; i++)
        {
            for (int j = 0; j < grid.Height; j++)
            {
                if (grid.TryPlaceItem(item, i, j))
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

        dungeon = new Dungeon();

        var startRooms = Grid.Items.Where(item => item != null && item.PortalsCount == 1).ToArray();

        GridItem startRoom = startRooms[Random.Range(0, startRooms.Length)]; 
        
        character = character ? character : Instantiate(characterPrefab);
        character.transform.DOComplete();
        
        character.transform.position = startRoom.transform.position;
        character.transform.localScale = Vector3.zero;
        character.transform.DOScale(Vector3.one, 0.2f).OnComplete(StartRun);

        void StartRun()
        {
            character.StartRun(dungeon, startRoom, Win, Lose);
            started = true;
        }
    }

    void Win()
    {
        character.transform.DOScale(Vector3.one * 1.2f, 0.4f).SetLoops(-1, LoopType.Yoyo);

        started = false;
        
        AddTiles();
    }

    void Lose()
    {
        character.transform.DOScale(Vector3.zero, 0.2f);
        Debug.Log("Lose :(");
        
        started = false;
    }
}