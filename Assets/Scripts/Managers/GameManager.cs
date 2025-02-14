using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private int heartCount;
    public int HeartCount
    {
        get => heartCount;
        set
        {
            heartCount = value;
            onChangeHeart?.Invoke();
        }
    }
    
    public Action onChangeHeart;

    protected override void Awake()
    {
        base.Awake();
        HeartCount = UserInformations.HeartCount;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("Main");
    }
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    private void OnApplicationQuit()
    {
        UserInformations.HeartCount = heartCount;
    }
}
