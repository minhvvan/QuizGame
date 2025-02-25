using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Switch : MonoBehaviour
{
    [SerializeField] private RectTransform _handle;
    [SerializeField] private Image _background;
    [SerializeField] List<Color> _colors = new List<Color>();
    
    private RectTransform _rectTransform;
    private Button _button;
    private bool _bActive;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClickSwitch);

        SetOn();
    }

    void OnClickSwitch()
    {
        if (_bActive) SetOff();
        else SetOn();
    }

    void SetOn()
    {
        _background.color = _colors[1];
        _handle.DOAnchorPosX(_rectTransform.sizeDelta.x - _handle.sizeDelta.x, .2f).SetEase(Ease.OutQuad);
        _bActive = true;
    }

    void SetOff()
    {
        _background.color = _colors[0];
        _handle.DOAnchorPosX(0, 0.2f).SetEase(Ease.OutQuad);
        _bActive = false;
    }
}
