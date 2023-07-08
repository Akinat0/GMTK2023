using System.Linq;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    public static GameScene Instance { get; private set; }

    [SerializeField] GridController grid;
    [SerializeField] Character characterPrefab;
    [SerializeField] LevelConfig levelConfig;

    GridItem startRoom;
    GridItem finalRoom;

    Character character;

    Dungeon dungeon;
    
    public static GridController Grid => Instance != null ? Instance.grid : null;

    //UI DISPLAY
    [SerializeField] TextMeshProUGUI displayHp, displayLvl;
    [SerializeField] TextMeshProUGUI multiplierField;
    [SerializeField] Slider sliderHp, sliderExp;
    bool started;

    void Start()
    {
        Instance = this;
        Grid.Build(levelConfig.FieldSize.x, levelConfig.FieldSize.y);
        
        PlaceItem(startRoom = Instantiate(levelConfig.StartRoom));
        
        foreach (var tilePrefab in levelConfig.AllowedTiles)
            PlaceItem(Instantiate(tilePrefab));

        PlaceItem(finalRoom = Instantiate(levelConfig.FinalRoom));
    }

    private void Update()
    {
        if (started)
        {
            displayHp.text = "HP:" + character.runtimeParamsContainer.Hp;
            sliderHp.value = character.runtimeParamsContainer.Hp/character.paramsContainer.Hp;

            displayLvl.text = "LVL:" + character.runtimeParamsContainer.Lvl;
            sliderExp.value = (float)character.runtimeParamsContainer.Exp / 10;

            multiplierField.text = $"X{dungeon.Multiplier}";
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

        dungeon = new Dungeon();
        
        character = character ? character : Instantiate(characterPrefab);
        character.transform.DOComplete();
        character.transform.DOKill();
        character.transform.position = startRoom.transform.position;
        character.transform.localScale = Vector3.zero;
        character.transform.DOScale(Vector3.one, 0.2f);
        character.StartRun(dungeon, startRoom, finalRoom, Win, Lose);
        started = true;
    }

    void Win()
    {
        character.transform.DOScale(Vector3.one * 1.2f, 0.4f).SetLoops(-1, LoopType.Yoyo);
        Debug.Log("WIN!");
    }

    void Lose()
    {
        character.transform.DOScale(Vector3.zero, 0.2f);
        Debug.Log("Lose :(");
    }
}