using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PopupPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject container;
    
    private Image _bgImage;

    protected void Awake()
    {
        _bgImage = GetComponent<Image>();

        var color = _bgImage.color;
        color.a = 0;
        _bgImage.color = color;
        
        container.GetComponent<CanvasGroup>().alpha = 0;
    }

    protected virtual void Start()
    {
        ShowContentPanel();
    }

    public void OnClickCloseButton()
    {
        HideContentPanel();
    }
    
    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    private void ShowContentPanel()
    {
        _bgImage.DOFade(0, 0);
        container.GetComponent<CanvasGroup>().DOFade(0, 0);
        container.GetComponent<RectTransform>().DOAnchorPosY(-500f, 0);
        
        _bgImage.DOFade(1, 0.2f);
        container.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
        container.GetComponent<RectTransform>().DOAnchorPosY(0, 0.2f);
    }

    private void HideContentPanel()
    {
        _bgImage.DOFade(1, 0);
        container.GetComponent<CanvasGroup>().DOFade(1, 0);
        container.GetComponent<RectTransform>().DOAnchorPosY(0, 0);
        
        _bgImage.DOFade(0, 0.2f);
        container.GetComponent<CanvasGroup>().DOFade(0, 0.2f);
        container.GetComponent<RectTransform>().DOAnchorPosY(-500f, 0.2f).OnComplete(() => { Destroy(gameObject); });
    }
}
