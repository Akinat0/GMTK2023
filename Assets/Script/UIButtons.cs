using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class UIButtons : MonoBehaviour
{
    [SerializeField] GameObject FaqScreen;
    Tween tween;

    public void ButtonBop()
    {
        tween.Pause();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 0).OnComplete(() => tween.Play());
    }


    public void Start()
    {
        tween = transform.DOScale(Vector3.one * 1.05f, 0.4f).SetLoops(-1, LoopType.Yoyo);
    }

    public void Faq(bool status)
    {
        FaqScreen.SetActive(status);
    }
}
