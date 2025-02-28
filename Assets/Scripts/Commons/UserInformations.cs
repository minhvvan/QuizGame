using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class UserInformations
{
    private const string HEART_COUNT = "HeartCount";
    private const string LAST_STAGE = "LastStage";
    private const string PLAY_SFX = "PlaySFX";
    private const string PLAY_BGM = "PlayBGM";
    
    public static int HeartCount
    {
        get
        {
            return PlayerPrefs.GetInt(HEART_COUNT, 0);
        }
        set
        {
            PlayerPrefs.SetInt(HEART_COUNT, value);
            PlayerPrefs.Save();
        }
    }

    public static int LastStageIndex
    {
        get
        {
            return PlayerPrefs.GetInt(LAST_STAGE, 0);
        }
        set
        {
            PlayerPrefs.SetInt(LAST_STAGE, value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsPlaySFX
    {
        get
        {
            return PlayerPrefs.GetInt(PLAY_SFX, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(PLAY_SFX, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    public static bool IsPlayBGM
    {
        get
        {
            return PlayerPrefs.GetInt(PLAY_BGM, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(PLAY_BGM, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
