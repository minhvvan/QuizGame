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
    
    private enum PanelType { Front, Correct, Incorrect }

    private int _idx;
    private int _answerIdx;
    public delegate void QuizCardDelegate(int cardIndex);
    private event QuizCardDelegate onCompleted;

    private Vector2 _correctBackPanelPosition;
    private Vector2 _incorrectBackPanelPosition;
    
    private void Awake()
    {
        _correctBackPanelPosition = correctBackPanel.GetComponent<RectTransform>().anchoredPosition;
        _incorrectBackPanelPosition = incorrectBackPanel.GetComponent<RectTransform>().anchoredPosition;

        timer.onTimerExpired += OnTimerExpired;
        timer.gameObject.SetActive(false);
        GameManager.Instance.onChangeHeart += OnChangedHeart;
    }

    public void SetQuiz(QuizData quizData, QuizCardDelegate onCompleted, int idx)
    {
        // 퀴즈 데이터 표현
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
        _idx = idx;
    }

    public async void OnClickQuizButton(int idx)
    {
        timer.PauseTimer();
        
        if (_answerIdx == idx)
        {
            await Flip(PanelType.Correct);
        }
        else
        {
            heartPanelController.InitHeartCount(GameManager.Instance.HeartCount);
            await Flip(PanelType.Incorrect);
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
            GameManager.Instance.HeartCount--;
            await Flip(PanelType.Front);
            StartQuiz();
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

    private void ShowPanel(PanelType type)
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

    private async Task Flip(PanelType type)
    {
        var sequence = DOTween.Sequence();

        sequence.Append(transform.DORotate(new Vector3(0, 90f, 0), .2f));
        sequence.AppendCallback(() =>
        {
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            ShowPanel(type);
        });
        sequence.Append(transform.DORotate(new Vector3(0, 0f, 0), .2f).SetEase(Ease.OutBounce));
        
        await sequence.AsyncWaitForCompletion();
    }
}