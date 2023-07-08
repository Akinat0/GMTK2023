using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class UIButtons : MonoBehaviour
{
    public void ButtonBop()
    {
        transform.DOPunchScale(Vector3.one * 1.1f, 0.5f, 0);
    }

    public void ButtonIdle()
    {
        transform.DOScale(Vector3.one * 1.2f, 0.4f).SetLoops(-1, LoopType.Yoyo);
    }
}
