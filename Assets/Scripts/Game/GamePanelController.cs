using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VFolders.Libs;

public class GamePanelController : MonoBehaviour
{
    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform quizCardParent;
    [SerializeField] private GameObject pangEffect;
    [SerializeField] private List<Color> bgColors = new ();
    [SerializeField] private GameObject levelPanel;

    private Image _bgImage;
    private CanvasGroup _gamePanelCanvasGroup;
    private List<QuizData> _quizDataList = new();
    private List<QuizCardController> _cardControllers = new();
    private int _quizIdx;
    private int _lastStageIndex;

    private void Awake()
    {
        _bgImage = GetComponent<Image>();
        _gamePanelCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        _quizIdx = 0;
        _lastStageIndex = UserInformations.LastStageIndex;

        ShowLevel();
    }

    private void ShowLevel()
    {
        var sequence = DOTween.Sequence();

        var image = levelPanel.GetComponent<CanvasGroup>();
        image.DOFade(1f, 0f);
        
        levelPanel.GetComponentInChildren<TMP_Text>().text = (_lastStageIndex + 1).ToString();
        
        sequence.Append(levelPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutBack))
            .AppendInterval(.3f)
            .Append(levelPanel.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(Ease.InOutQuad))
            .Join(image.DOFade(0f, 0.5f).SetEase(Ease.InOutQuad))
            .OnComplete(InitQuizCard);
    }

    private void InitQuizCard()
    {
        _quizDataList = QuizDataController.LoadQuizData(_lastStageIndex + 1);

        {
            var newCard = ObjectPool.Instance.GetObject();
            newCard.transform.SetParent(quizCardParent, false);
            
            if (newCard.TryGetComponent<QuizCardController>(out var controller))
            {
                controller.SetQuiz(_quizDataList[_quizIdx], OnCompletedQuiz, OnQuizResult, _quizIdx++);
                controller.SetQuizCardPosition(QuizCardController.QuizCardPositionType.First, false);
                _cardControllers.Add(controller);
            }
        }
        
        {
            var newCard = ObjectPool.Instance.GetObject();
            newCard.transform.SetParent(quizCardParent, false);
            
            if (newCard.TryGetComponent<QuizCardController>(out var controller))
            {
                controller.SetQuiz(_quizDataList[_quizIdx], OnCompletedQuiz, OnQuizResult, _quizIdx++);
                controller.SetQuizCardPosition(QuizCardController.QuizCardPositionType.Second, false);
                _cardControllers.Add(controller);
            }
        }
        
        {
            var newCard = ObjectPool.Instance.GetObject();
            newCard.transform.SetParent(quizCardParent, false);
            
            if (newCard.TryGetComponent<QuizCardController>(out var controller))
            {
                controller.SetQuiz(_quizDataList[_quizIdx], OnCompletedQuiz, OnQuizResult, _quizIdx++);
                controller.SetQuizCardPosition(QuizCardController.QuizCardPositionType.Remove, false);
                _cardControllers.Add(controller);
            }
        }
        
        _cardControllers.First().StartQuiz();
        _cardControllers.Last().gameObject.SetActive(false);
    }
    
    private void OnCompletedQuiz(int cardIndex)
    {
        if (cardIndex == _quizDataList.Count - 1)
        {
            //TODO: Stage 전체 Clear 처리
            //TODO: Clear 연출
            _lastStageIndex++;
            InitQuizCard();
        }
        
        ShowNextQuiz(cardIndex);
    }

    private void OnQuizResult(bool bCorrect)
    {
        if (bCorrect)
        {
            if (pangEffect.TryGetComponent<Image>(out var image))
            {
                var sequence = DOTween.Sequence();

                sequence.Append(_bgImage.DOColor(bgColors[1], 0.5f)).
                    Append(image.DOFade(1f, 0)).
                    Append(image.DOFade(0f, 3f)).
                    AppendInterval(1f).
                    Append(_bgImage.DOColor(bgColors[0], 0.5f));
            }
        }
        else
        {
            var sequence = DOTween.Sequence();

            sequence.Append(_bgImage.DOColor(bgColors[2], 0.5f)).
                AppendInterval(1f).
                Append(_bgImage.DOColor(bgColors[0], 0.5f));
        }
    }

    private void ShowNextQuiz(int currentIdx)
    {
        int currentCardIdx = currentIdx % 3;
        _cardControllers[currentCardIdx].SetQuizCardPosition(QuizCardController.QuizCardPositionType.Remove, true, () =>
        {
            if (_quizIdx < _quizDataList.Count)
            {
                _cardControllers[currentCardIdx].SetQuiz(_quizDataList[_quizIdx], OnCompletedQuiz, OnQuizResult, _quizIdx);
            }
            
            _quizIdx++;

            if (currentIdx + 1 >= _quizDataList.Count) return;
            int first = (currentIdx + 1) % 3;
            int second = (currentIdx + 2) % 3;
            
            _cardControllers[first].gameObject.SetActive(true);
            _cardControllers[second].gameObject.SetActive(true);
            
            _cardControllers[first].SetQuizCardPosition(QuizCardController.QuizCardPositionType.First, true);
            _cardControllers[second].SetQuizCardPosition(QuizCardController.QuizCardPositionType.Second, true);

            _cardControllers[first].StartQuiz();
        });
    }
}
