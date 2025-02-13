using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VFolders.Libs;

public class GamePanelController : MonoBehaviour
{
    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform quizCardParent;
    [SerializeField] private Button nextQuizButton;

    private List<QuizCardController> _cardControllers = new();
    private int _currentIdx;
    
    private void Start()
    {
        InitQuizCard();
        
        nextQuizButton.onClick.AddListener(() =>
        {
            StartCoroutine(ShowNextQuiz());
        });
    }

    private void InitQuizCard()
    {
        for (int i = 0; i < 3; i++)
        {
            var newCard = ObjectPool.Instance.GetObject();
            newCard.transform.SetParent(quizCardParent, false);
            newCard.transform.SetSiblingIndex(3-i);

            if (newCard.TryGetComponent<RectTransform>(out var rect))
            {
                rect.localScale *= Mathf.Pow(0.9f, i);
                rect.anchoredPosition += new Vector2(0, 150f * i);
            }

            if (newCard.TryGetComponent<QuizCardController>(out var controller))
            {
                controller.SetText($"{i+1}");
                _cardControllers.Add(controller);
            }
        }

        _cardControllers.Last().gameObject.SetActive(false);
        _currentIdx = 0;
    }
    
    private IEnumerator ShowNextQuiz()
    {
        if (_cardControllers[_currentIdx].TryGetComponent<RectTransform>(out var currentCardRect))
        {
            yield return currentCardRect.DOAnchorPosY(-Screen.height, .3f).OnComplete(() =>
            {
                currentCardRect.anchoredPosition = new Vector2(0, 150f * 3);
                currentCardRect.localScale = Vector3.one * Mathf.Pow(0.9f, 3);
                
                _cardControllers[_currentIdx].gameObject.SetActive(false);
                _currentIdx = (_currentIdx + 1) % 3;
            }).WaitForCompletion();
        }

        for (int i = 0; i < 2; i++)
        {
            int next = (_currentIdx + i) % 3;
            
            _cardControllers[next].gameObject.SetActive(true);
            if (_cardControllers[next].TryGetComponent<RectTransform>(out var rect))
            {
                DOTween.Sequence()
                    .Append(rect.DOScale(Vector3.one * Mathf.Pow(0.9f, i), .3f).SetEase(Ease.Linear))
                    .Join(rect.DOAnchorPos(new Vector2(0, 150f * i), .3f).SetEase(Ease.Linear));
                rect.SetSiblingIndex(3-i);
            }
        }
    }
    
    public void OnClickGameOverButton()
    {
        GameManager.Instance.QuitGame();
    }
}
