using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private GameObject _startButtonEffectImage;
    [SerializeField] private HeartPanelController _heartPanelController;

    private void Start()
    {
        _stageText.text = $"Stage {UserInformations.LastStageIndex + 1}";
        _startButtonEffectImage.transform.localScale = Vector3.one;
        
        var sequence = DOTween.Sequence();
        sequence.SetLink(gameObject);
        sequence.Join(_startButtonEffectImage.transform.DOScale(1.2f, 1f));
        sequence.Join(_startButtonEffectImage.GetComponent<Image>().DOFade(0f, 1f));
        sequence.SetLoops(-1);
    }

    public async void OnClickPlayButton()
    {
        await _heartPanelController.RemoveHeart();
        GameManager.Instance.StartGame();
    }

    #region Main Menu Button Listener

    public void OnClickShopButton()
    {
        
    }
    
    public void OnClickStageButton()
    {
        
    }
    
    public void OnClickLeaderboardButton()
    {
        
    }
    
    public void OnClickSettingsButton()
    {
        
    }

    #endregion
}
