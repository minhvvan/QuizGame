using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPopupPanelController : MonoBehaviour
{
    [SerializeField] private Switch sfxSwitch;
    [SerializeField] private Switch bgmSwitch;

    private void Start()
    {
        sfxSwitch.SetValue(UserInformations.IsPlaySFX);
        bgmSwitch.SetValue(UserInformations.IsPlayBGM);
        
        sfxSwitch.OnSwitchChanged += switchValue =>
        {
            UserInformations.IsPlaySFX = switchValue;
        };
        
        bgmSwitch.OnSwitchChanged += switchValue =>
        {
            UserInformations.IsPlayBGM = switchValue;
        };
    }
}
