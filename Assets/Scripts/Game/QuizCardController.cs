using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct QuizData
{
    public string question;
    public string description;
    public int type;
    public int answer;
    public string firstOption;
    public string secondOption;
    public string thirdOption;
}

#region States
// 퀴즈 카드의 위치 상태를 정의할 클래스가 반드시 구현할(약속) 메서드의 목록
public interface IQuizCardPositionState
{
    void Trasition(bool withAnimation, Action onComplete = null);
}

// 퀴즈 카드의 위치 상태 전이를 관리할 목적
public class QuizCardPositionStateContext
{
    private IQuizCardPositionState _currentState;

    public void SetState(IQuizCardPositionState state, bool withAnimation, Action onComplete = null)
    {
        if (_currentState == state) return;
        
        _currentState = state;
        _currentState.Trasition(withAnimation, onComplete);
    }
}

public class QuizCardPositionState
{
    protected QuizCardController _quizCardController;
    protected RectTransform _rectTransform;
    protected CanvasGroup _canvasGroup;

    public QuizCardPositionState(QuizCardController quizCardController)
    {
        _quizCardController = quizCardController;
        _rectTransform = _quizCardController.gameObject.GetComponent<RectTransform>();
        _canvasGroup = _quizCardController.gameObject.GetComponent<CanvasGroup>();
    }
}

// 퀴즈 카드가 첫 번째 위치에 나타날 상태를 처리할 상태 클래스
public class QuizCardPositionStateFirst: QuizCardPositionState, IQuizCardPositionState
{
    public QuizCardPositionStateFirst(QuizCardController quizCardController) : base(quizCardController) { }

    public void Trasition(bool withAnimation, Action onComplete = null)
    {
        var animationDuration = (withAnimation) ? 0.2f : 0f;
        
        _rectTransform.DOAnchorPos(Vector2.zero, animationDuration);
        _rectTransform.DOScale(1f, animationDuration);
        _canvasGroup.DOFade(1f, animationDuration).OnComplete(() => onComplete?.Invoke());

        _rectTransform.SetAsLastSibling();
    }
}
// 퀴즈 카드가 두 번째 위치에 나타날 상태를 처리할 상태 클래스
public class QuizCardPositionStateSecond: QuizCardPositionState, IQuizCardPositionState
{
    public QuizCardPositionStateSecond(QuizCardController quizCardController) : base(quizCardController) { }

    public void Trasition(bool withAnimation, Action onComplete = null)
    {
        var animationDuration = (withAnimation) ? 0.2f : 0f;
        
        _rectTransform.DOAnchorPos(new Vector2(0f, 160f), 0);
        _rectTransform.DOScale(0.9f, animationDuration);
        _canvasGroup.DOFade(0.7f, animationDuration).OnComplete(() => onComplete?.Invoke());

        _rectTransform.SetAsFirstSibling();
    }
}
// 퀴즈 카드가 사라질 상태를 처리할 상태 클래스
public class QuizCardPositionStateRemove: QuizCardPositionState, IQuizCardPositionState
{
    public QuizCardPositionStateRemove(QuizCardController quizCardController) : base(quizCardController) { }

    public void Trasition(bool withAnimation, Action onComplete = null)
    {
        var animationDuration = (withAnimation) ? 0.2f : 0f;
        _rectTransform.DOAnchorPos(new Vector2(0f, -280f), animationDuration);
        _canvasGroup.DOFade(0f, animationDuration).OnComplete(() => onComplete?.Invoke());
        _quizCardController.PauseQuiz();
    }
}

public class QuizCardPositionStateFlip: QuizCardPositionState, IQuizCardPositionState
{
    public QuizCardPositionStateFlip(QuizCardController quizCardController, QuizCardController.PanelType type) : base(quizCardController) { _panelType = type; }
    private QuizCardController.PanelType _panelType;
    
    public void Trasition(bool withAnimation, Action onComplete = null)
    {
        var sequence = DOTween.Sequence();

        sequence.Append(_rectTransform.DORotate(new Vector3(0, 90f, 0), .2f));
        sequence.AppendCallback(() =>
        {
            _rectTransform.rotation = Quaternion.Euler(0f, -90f, 0f);
            _quizCardController.ShowPanel(_panelType);
        });
        sequence.Append(_rectTransform.DORotate(new Vector3(0, 0f, 0), .2f).SetEase(Ease.OutBounce)).OnComplete(() => onComplete?.Invoke());
    }
}
#endregion

public class QuizCardController : MonoBehaviour
{
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button[] optionButtons;

    [SerializeField] private GameObject threeOptionButtons;
    [SerializeField] private GameObject OXOptionButtons;
    
    [SerializeField] private GameObject frontPanel;
    [SerializeField] private GameObject correctBackPanel;
    [SerializeField] private GameObject incorrectBackPanel;

    [SerializeField] private RoundTimer timer;
    [SerializeField] private HeartPanelController heartPanelController;
    
    [SerializeField] private List<Color> buttonColors;
    [SerializeField] private GameObject quizUpPanel;
    [SerializeField] private GameObject correctUpPanel;
    [SerializeField] private GameObject incorrectUpPanel;
    [SerializeField] private GameObject correctEffect;
    [SerializeField] private Slider quizSlider;
    [SerializeField] private TMP_Text quizText;
    [SerializeField] private Image oSprite;
    [SerializeField] private Image xSprite;

    public enum PanelType { Front, Correct, Incorrect }
    public enum QuizCardPositionType { First, Second, Remove }
    
    private int _idx;
    private int _answerIdx;
    public delegate void QuizCardDelegate(int cardIndex);
    public delegate void QuizCardResultDelegate(bool bCorrect);
    private event QuizCardDelegate onCompleted;
    private event QuizCardResultDelegate onResult;

    private Vector2 _correctBackPanelPosition;
    private Vector2 _incorrectBackPanelPosition;
    
    // 퀴즈 카드 위치 상태
    private IQuizCardPositionState _positionStateFirst;
    private IQuizCardPositionState _positionStateSecond;
    private IQuizCardPositionState _positionStateRemove;
    private QuizCardPositionStateContext _positionStateContext;
    private IQuizCardPositionState _flipStateFront;
    private IQuizCardPositionState _flipStateCorrect;
    private IQuizCardPositionState _flipStateIncorrect;

    private QuizData _quizData;
    
    private void Awake()
    {
        _correctBackPanelPosition = correctBackPanel.GetComponent<RectTransform>().anchoredPosition;
        _incorrectBackPanelPosition = incorrectBackPanel.GetComponent<RectTransform>().anchoredPosition;

        // 상태 관리를 위한 Context 객체 생성
        _positionStateContext = new QuizCardPositionStateContext();
        _positionStateFirst = new QuizCardPositionStateFirst(this);
        _positionStateSecond = new QuizCardPositionStateSecond(this);
        _positionStateRemove = new QuizCardPositionStateRemove(this);
        _flipStateFront = new QuizCardPositionStateFlip(this, PanelType.Front);
        _flipStateCorrect = new QuizCardPositionStateFlip(this, PanelType.Correct);
        _flipStateIncorrect = new QuizCardPositionStateFlip(this, PanelType.Incorrect);
        
        timer.onTimerExpired += OnTimerExpired;
        timer.gameObject.SetActive(false);
        GameManager.Instance.onChangeHeart += OnChangedHeart;
    }

    public void SetQuiz(QuizData quizData, QuizCardDelegate onCompleted, QuizCardResultDelegate onResult, int idx)
    {
        // 퀴즈 데이터 표현
        _quizData = quizData;
        questionText.text = quizData.question;
        descriptionText.text = quizData.description;
        _answerIdx = quizData.answer;

        if (quizData.type == 0)
        {
            threeOptionButtons.SetActive(true);
            OXOptionButtons.SetActive(false);
            
            var firstButtonText = optionButtons[0].GetComponentInChildren<TMP_Text>();
            firstButtonText.text = quizData.firstOption;
            var secondButtonText = optionButtons[1].GetComponentInChildren<TMP_Text>();
            secondButtonText.text = quizData.secondOption;
            var thirdButtonText = optionButtons[2].GetComponentInChildren<TMP_Text>();
            thirdButtonText.text = quizData.thirdOption;
        }
        else if (quizData.type == 1)
        {
            threeOptionButtons.SetActive(false);
            OXOptionButtons.SetActive(true);
        }
        
        ShowPanel(PanelType.Front);
        this.onCompleted = onCompleted;
        this.onResult = onResult;
        _idx = idx;
    }
    
    public void SetQuizCardPosition(QuizCardPositionType quizCardPositionType, bool withAnimation, Action onComplete = null)
    {
        switch (quizCardPositionType)
        {
            case QuizCardPositionType.First:
                _positionStateContext.SetState(_positionStateFirst, withAnimation, () =>
                {
                    StartQuiz();
                    onComplete?.Invoke();
                });
                break;
            case QuizCardPositionType.Second:
                _positionStateContext.SetState(_positionStateSecond, withAnimation, () =>
                {
                    onComplete?.Invoke();
                });
                break;
            case QuizCardPositionType.Remove:
                _positionStateContext.SetState(_positionStateRemove, withAnimation, onComplete);
                break;
        }
    }

    private void Flip(PanelType panelType, bool withAnimation, Action onComplete = null)
    {
        switch (panelType)
        {
            case PanelType.Front:
            {
                _positionStateContext.SetState(_flipStateFront, withAnimation, onComplete);
                break;
            }
            case PanelType.Correct:
            {
                _positionStateContext.SetState(_flipStateCorrect, withAnimation, onComplete);
                break;
            }
            case PanelType.Incorrect:
            {
                _positionStateContext.SetState(_flipStateIncorrect, withAnimation, onComplete);
                break;
            }
        }
    }

    public async void OnClickQuizButton(int idx)
    {
        timer.PauseTimer();
        
        var clickedButton = optionButtons[idx + (_quizData.type == 0 ? 0 : 3)];
        if(clickedButton.TryGetComponent<Image>(out var buttonImage))
        {
            SetContentColor(idx, clickedButton, true);
            
            if (_answerIdx == idx)
            {
                onResult?.Invoke(true);

                correctUpPanel.SetActive(true);
                quizUpPanel.SetActive(false);
                incorrectUpPanel.SetActive(false);
                
                buttonImage.color = buttonColors[1];
                var effect = correctEffect.GetComponent<Image>();
                correctEffect.transform.localScale = Vector3.one;
                effect.DOFade(1f, 0f);
                
                var sequence = DOTween.Sequence();
                sequence.Append(correctEffect.transform.DOScale(Vector3.one * 1.2f, 1f))
                    .Join(effect.DOFade(0f, 1f))
                    .AppendInterval(0.5f)
                    .OnComplete(() =>
                    {
                        Flip(PanelType.Correct, true);
                        correctEffect.transform.localScale = Vector3.one;
                        effect.DOFade(1f, 0f);
                    });
            }
            else
            {
                onResult?.Invoke(false);
                
                quizSlider.value = (float)_idx / Constants.MAX_QUIZ_COUNT;
                quizText.text = $"레벨 업 까지 {Constants.MAX_QUIZ_COUNT - _idx} 문제 남았습니다.";
                
                incorrectUpPanel.SetActive(true);
                quizUpPanel.SetActive(false);
                correctUpPanel.SetActive(false);
                buttonImage.color = buttonColors[2];
             
                var sequence = DOTween.Sequence();
                sequence.Append(transform.DOPunchRotation(new Vector3(0, 0, 15f), .5f, 10, 1f))
                    .AppendInterval(0.5f)
                    .OnComplete(() =>
                    {
                        Flip(PanelType.Incorrect, true);
                    });
            }
        }
    }

    private void SetContentColor(int idx, Button clickedButton, bool bClicked)
    {
        if (bClicked)
        {
            if (clickedButton.GetComponentInChildren<TMP_Text>())
            {
                var text = clickedButton.GetComponentInChildren<TMP_Text>();
                text.color = Color.white;
            }
            else
            {
                if (idx == 0) oSprite.color = Color.white;
                if (idx == 1) xSprite.color = Color.white;
            }
        }
        else
        {
            if (clickedButton.GetComponentInChildren<TMP_Text>())
            {
                var text = clickedButton.GetComponentInChildren<TMP_Text>();
                text.color = Color.black;
            }
            else
            {
                if (idx == 0) oSprite.color = new Color32(44, 55, 89, 255);
                if (idx == 1) xSprite.color = new Color32(44, 55, 89, 255);
            }
        }
    }

    public void OnClickNextQuizButton()
    {
        onCompleted?.Invoke(_idx);
    }
    
    public void OnClickExitButton()
    {
        GameManager.Instance.QuitGame();
    }
    
    public async void OnClickRetryButton()
    {
        if (GameManager.Instance.HeartCount > 0)
        {
            await heartPanelController.RemoveHeart();
            Flip(PanelType.Front,true, StartQuiz);
        }
        else
        {
            heartPanelController.EmptyHeart();
        }
    }

    private void OnChangedHeart()
    {
        heartPanelController.InitHeartCount(GameManager.Instance.HeartCount);
    }

    public void ShowPanel(PanelType type)
    {
        switch (type)
        {
            case PanelType.Front:
                frontPanel.SetActive(true);
                correctBackPanel.SetActive(false);
                incorrectBackPanel.SetActive(false);

                correctBackPanel.GetComponent<RectTransform>().anchoredPosition = _correctBackPanelPosition;
                incorrectBackPanel.GetComponent<RectTransform>().anchoredPosition = _incorrectBackPanelPosition;
                break;
            case PanelType.Correct:
                frontPanel.SetActive(false);
                correctBackPanel.SetActive(true);
                incorrectBackPanel.SetActive(false);
                
                correctBackPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                incorrectBackPanel.GetComponent<RectTransform>().anchoredPosition = _incorrectBackPanelPosition;
                break;
            case PanelType.Incorrect:
                frontPanel.SetActive(false);
                correctBackPanel.SetActive(false);
                incorrectBackPanel.SetActive(true);
                
                correctBackPanel.GetComponent<RectTransform>().anchoredPosition = _correctBackPanelPosition;
                incorrectBackPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                break;
        }
    }
    
    public void StartQuiz()
    {
        for (var i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].image.color = buttonColors[0];
            SetContentColor(i % 3, optionButtons[i], false);
        }
        
        quizUpPanel.SetActive(true);
        correctUpPanel.SetActive(false);
        incorrectUpPanel.SetActive(false);
        
        timer.ResetTimer();
        timer.StartTimer();
    }

    public void PauseQuiz()
    {
        timer.gameObject.SetActive(false);
        timer.PauseTimer();
    }
    
    private void OnTimerExpired()
    {
        ShowPanel(PanelType.Incorrect);
    }
}