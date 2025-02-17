using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundTimer : MonoBehaviour
{
    [Serializable]
    public class FillSettings
    {
        public Color color;
    }

    [Serializable]
    public class BackgroundSettings
    {
        public Color color;
    }

    public FillSettings fillSettings;
    public BackgroundSettings backgroundSettings;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private float totalTime;
    [SerializeField] private Image headCapImage;
    [SerializeField] private Image tailCapImage;
    [SerializeField] private TMP_Text timeText;
    
    public float CurrentTime { get; private set; }
    private bool _isPaused;
    public Action onTimerExpired;

    public void StartTimer()
    {
        _isPaused = false;
        gameObject.SetActive(true);
        StartCoroutine(ProgressTimer());
    }

    public void PauseTimer()
    {
        _isPaused = true;
    }

    public void ResetTimer()
    {
        CurrentTime = 0;
        fillImage.fillAmount = 0;
        headCapImage.gameObject.SetActive(false);
        tailCapImage.gameObject.SetActive(false);
        headCapImage.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    private IEnumerator ProgressTimer()
    {
        while (!_isPaused)
        {
            CurrentTime += Time.deltaTime;
            fillImage.fillAmount = (totalTime - CurrentTime) / totalTime;

            if (CurrentTime >= totalTime)
            {
                headCapImage.gameObject.SetActive(false);
                tailCapImage.gameObject.SetActive(false);
                _isPaused = false;
                onTimerExpired?.Invoke();
                yield break;
            }
            else
            {
                headCapImage.gameObject.SetActive(true);
                tailCapImage.gameObject.SetActive(true);
                fillImage.gameObject.SetActive(true);
                headCapImage.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, fillImage.fillAmount * 360));

                var time = totalTime - CurrentTime;
                timeText.text = time.ToString("F0");
            }
            
            yield return null;
        }
    }
}
