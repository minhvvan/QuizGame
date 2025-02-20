using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class HeartPanelController : MonoBehaviour
{
    [SerializeField] private GameObject _heartEffectImage;
    [SerializeField] private TMP_Text _heartCountText;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<AudioClip> _heartSFX;
    public enum SFXType { Remove, Add, Empty };
    
    private void Start()
    {
        _heartEffectImage.SetActive(false);
        InitHeartCount(UserInformations.HeartCount);
    }

    public void InitHeartCount(int heartCount)
    {
        _heartCountText.text = GameManager.Instance.HeartCount.ToString();
    }
    
    public void AddHeart(int heartCount)
    {
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < heartCount; i++)
        {
            sequence.AppendCallback(() =>
            {
                ChangeTextAnimation(true, ()=>{GameManager.Instance.HeartCount++;});
                if (UserInformations.IsPlaySFX)
                {
                    _audioSource.PlayOneShot(_heartSFX[(int)SFXType.Add]);
                }
            });
            sequence.AppendInterval(0.5f);
        }
    }

    public async Task RemoveHeart()
    {
        _heartEffectImage.SetActive(true);
        _heartEffectImage.transform.localScale = Vector3.zero;
        _heartEffectImage.GetComponent<Image>().color = Color.white;

        var sequence = DOTween.Sequence();
    
        // 첫 번째: 스케일과 페이드 애니메이션
        sequence.Join(_heartEffectImage.transform.DOScale(3f, 1f));
        sequence.Join(_heartEffectImage.GetComponent<Image>().DOFade(0f, 1f));
    
        // 1초 대기 후 텍스트 애니메이션
        sequence.AppendCallback(() => {
            ChangeTextAnimation(false, () => { GameManager.Instance.HeartCount--; });
            if (UserInformations.IsPlaySFX)
            {
                _audioSource.PlayOneShot(_heartSFX[(int)SFXType.Remove]);
            }
        });
        sequence.AppendInterval(1f);

        // 시퀀스 완료 대기
        await sequence.AsyncWaitForCompletion();
    }
    
    public void EmptyHeart()
    {
        GetComponent<RectTransform>().DOPunchPosition(new Vector3(20f, 0, 0), 1f, 7);
        if (UserInformations.IsPlaySFX)
        {
            _audioSource.PlayOneShot(_heartSFX[(int)SFXType.Empty]);
        }
    }

    private void ChangeTextAnimation(bool isAdd, Action callback = null)
    {
        float duration = 0.2f;
        float yPos = 40f;

        _heartCountText.rectTransform.DOAnchorPosY(-yPos, duration);
        _heartCountText.DOFade(0, duration).OnComplete(() =>
        {
            if (isAdd)
            {
                _heartCountText.text = (GameManager.Instance.HeartCount + 1).ToString();
            }
            else
            {
                _heartCountText.text = (GameManager.Instance.HeartCount - 1).ToString();
            }
            
            _heartCountText.rectTransform.DOAnchorPosY(yPos, 0);
            _heartCountText.rectTransform.DOAnchorPosY(0, duration);
            _heartCountText.DOFade(1f, duration);
            
            callback?.Invoke();
        });
    }
}