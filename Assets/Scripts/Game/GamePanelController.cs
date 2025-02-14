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

    private List<QuizData> _quizDataList = new();
    private List<QuizCardController> _cardControllers = new();
    private int _quizIdx;
    private int _lastStageIndex;

    private void Start()
    {
        _quizIdx = 0;
        _lastStageIndex = UserInformations.LastStageIndex;
        
        InitQuizCard();
    }

    private void InitQuizCard()
    {
        _quizDataList = QuizDataController.LoadQuizData(_lastStageIndex + 1);

        for (int i = 0; i < 3; i++)
        {
            var newCard = ObjectPool.Instance.GetObject();
            newCard.transform.SetParent(quizCardParent, false);

            if (newCard.TryGetComponent<RectTransform>(out var rect))
            {
                rect.localScale *= Mathf.Pow(0.9f, i);
                rect.anchoredPosition += new Vector2(0, 150f * i);
            }

            if (newCard.TryGetComponent<QuizCardController>(out var controller))
            {
                controller.SetQuiz(_quizDataList[i], OnCompletedQuiz, _quizIdx++);
                _cardControllers.Add(controller);
            }
        }
        
        for (int i = 0; i < _cardControllers.Count; i++)
        {
            _cardControllers[i].transform.SetSiblingIndex(_cardControllers.Count - 1 - i);
        }

        _cardControllers.Last().gameObject.SetActive(false);
    }
    
    private void OnCompletedQuiz(int cardIndex)
    {
        if (cardIndex == _quizDataList.Count - 1)
        {
            //TODO: Clear 연출
            _lastStageIndex++;
            InitQuizCard();
        }
        
        StartCoroutine(ShowNextQuiz(cardIndex));
    }

    private IEnumerator ShowNextQuiz(int currentIdx)
    {
        int currentCardIdx = currentIdx % 3;
        if (_cardControllers[currentCardIdx].TryGetComponent<RectTransform>(out var currentCardRect))
        {
            yield return currentCardRect.DOAnchorPosY(-Screen.height, .3f).OnComplete(() =>
            {
                currentCardRect.anchoredPosition = new Vector2(0, 150f * 3);
                currentCardRect.localScale = Vector3.one * Mathf.Pow(0.9f, 3);

                if (_quizIdx < _quizDataList.Count)
                {
                    _cardControllers[currentCardIdx].SetQuiz(_quizDataList[_quizIdx], OnCompletedQuiz, _quizIdx);
                }
                
                _cardControllers[currentCardIdx].gameObject.SetActive(false);
                _quizIdx++;
            }).WaitForCompletion();
        }

        for (int i = 1; i <= 2; i++)
        {
            if (currentIdx + i >= _quizDataList.Count) break;
            int next = (currentIdx + i) % 3;
            
            _cardControllers[next].gameObject.SetActive(true);
            if (_cardControllers[next].TryGetComponent<RectTransform>(out var rect))
            {
                DOTween.Sequence()
                    .Append(rect.DOScale(Vector3.one * Mathf.Pow(0.9f, i-1), .3f).SetEase(Ease.Linear))
                    .Join(rect.DOAnchorPos(new Vector2(0, 150f * (i-1)), .3f).SetEase(Ease.Linear));
                 rect.SetSiblingIndex(3-i);
            }
        }
    }
}
