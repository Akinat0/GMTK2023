using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class UIButtons : MonoBehaviour
{
    [SerializeField] GameObject FaqScreen;

    [SerializeField] Image image;
    [SerializeField] Sprite sprite1;
    [SerializeField] Sprite sprite2;

    [SerializeField] AudioListener listener;
    
    Tween tween;
    Tween punchTween;

    public void ButtonBop()
    {
        tween.Pause();
        transform.localScale = Vector3.one;

        punchTween?.Complete();
        punchTween = transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 0).OnComplete(() => tween.Play());
    }


    public void Start()
    {
        tween = transform.DOScale(Vector3.one * 1.03f, 2f).SetLoops(-1, LoopType.Yoyo);
    }

    public void Faq()
    {
        FaqScreen.SetActive(!FaqScreen.activeSelf);
    }

    public void ToggleSprite()
    {
        image.sprite = image.sprite == sprite1 ? sprite2 : sprite1;
        listener.enabled = !listener.enabled;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
