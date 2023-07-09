
using UnityEngine;

public class UIStartButton : MonoBehaviour
{
    public void StartGame()
    {
        if(!GameScene.Instance.started)
            GameScene.Instance.StartGame();
    }
    
    
}
