using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ObjectPool))]
public class GamePanelController : MonoBehaviour
{
    [SerializeField] private GameObject quizCardPrefab;
    [SerializeField] private Transform quizCardParent;
    [SerializeField] private GameObject pangEffect;
    [SerializeField] private List<Color> bgColors = new ();
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject resultEffect;
    [SerializeField] private GameObject resultPopup;

    private Image _bgImage;
    private CanvasGroup _gamePanelCanvasGroup;
    private List<QuizData> _quizDataList = new();
    private List<QuizCardController> _cardControllers = new();
    private ObjectPool _objectPool;
    private int _quizIdx;
    private int _lastStageIndex;

    private void Awake()
    {
        _bgImage = GetComponent<Image>();
        _gamePanelCanvasGroup = GetComponent<CanvasGroup>();
        _objectPool = GetComponent<ObjectPool>();
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
            var newCard = _objectPool.GetObject(quizCardPrefab);
            newCard.transform.SetParent(quizCardParent, false);
            
            if (newCard.TryGetComponent<QuizCardController>(out var controller))
            {
                controller.SetQuiz(_quizDataList[_quizIdx], OnCompletedQuiz, OnQuizResult, _quizIdx++);
                controller.SetQuizCardPosition(QuizCardController.QuizCardPositionType.First, false);
                _cardControllers.Add(controller);
            }
        }
        
        {
            var newCard = _objectPool.GetObject(quizCardPrefab);
            newCard.transform.SetParent(quizCardParent, false);
            
            if (newCard.TryGetComponent<QuizCardController>(out var controller))
            {
                controller.SetQuiz(_quizDataList[_quizIdx], OnCompletedQuiz, OnQuizResult, _quizIdx++);
                controller.SetQuizCardPosition(QuizCardController.QuizCardPositionType.Second, false);
                _cardControllers.Add(controller);
            }
        }
        
        {
            var newCard = _objectPool.GetObject(quizCardPrefab);
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
            UserInformations.LastStageIndex = ++_lastStageIndex;
            _cardControllers[cardIndex % 3].SetQuizCardPosition(QuizCardController.QuizCardPositionType.Remove, true, () =>
                {
                    StartCoroutine(PlayResultAnim());
                });
        }
        else
        {
            ShowNextQuiz(cardIndex);
        }
    }
    
    private IEnumerator PlayResultAnim()
    {
        if (resultPanel.TryGetComponent<Animator>(out var animator))
        {
            resultPanel.SetActive(true);
            resultEffect.gameObject.SetActive(true);
            resultPopup.gameObject.SetActive(false);
            
            animator.Play("ResultAnim", 0, 0f);
            
            yield return null;
    
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    
            while (stateInfo.normalizedTime < 1.0f)
            {
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
            
            resultEffect.gameObject.SetActive(false);
            resultPopup.gameObject.SetActive(true);
        }
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
            else
            {
                _cardControllers[currentCardIdx].Idx = _quizIdx;
            }
            
            _quizIdx++;

            int first = (currentIdx + 1) % 3;
            int second = (currentIdx + 2) % 3;

            if (_cardControllers[first].Idx < _quizDataList.Count)
            {
                _cardControllers[first].gameObject.SetActive(true);
                _cardControllers[first].SetQuizCardPosition(QuizCardController.QuizCardPositionType.First, true);
            
                _cardControllers[first].StartQuiz();
            }

            if (_cardControllers[second].Idx  < _quizDataList.Count)
            {
                _cardControllers[second].gameObject.SetActive(true);
                _cardControllers[second].SetQuizCardPosition(QuizCardController.QuizCardPositionType.Second, true);
            }
        });
    }

    public void OnClickNextLevel()
    {
        ShowLevel();
        resultPopup.gameObject.SetActive(false);
    }

    public void OnClickQuit()
    {
        GameManager.Instance.QuitGame();
        resultPopup.gameObject.SetActive(false);
    }
}
