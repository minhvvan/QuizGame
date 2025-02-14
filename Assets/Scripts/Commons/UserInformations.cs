using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class UserInformations
{
    private const string HEART_COUNT = "HeartCount";
    private const string LAST_STAGE = "LastStage";
    
    public static int HeartCount
    {
        get
        {
            return PlayerPrefs.GetInt(HEART_COUNT, 0);
        }
        set
        {
            PlayerPrefs.SetInt(HEART_COUNT, value);
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
        }
    }
}
