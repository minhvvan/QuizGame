using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageCellButton : MonoBehaviour
{
    [SerializeField] private GameObject normalImageObject;
    [SerializeField] private GameObject clearImageObject;
    [SerializeField] private GameObject lockImageObject;
    [SerializeField] private TMP_Text[] stageIndexText;

    private StageCellItem _stageCellItem = new();
    
    public void SetStageCell(StageCellItem stageCellItem)
    {
        _stageCellItem.stageIndex = stageCellItem.stageIndex;
        stageIndexText[0].text = (_stageCellItem.stageIndex + 1).ToString();
        stageIndexText[1].text = (_stageCellItem.stageIndex + 1).ToString();

        _stageCellItem.stageCellType = stageCellItem.stageCellType;
        switch (_stageCellItem.stageCellType)
        {
            case StageCellItem.StageCellType.Normal:
            {
                normalImageObject.SetActive(true);
                clearImageObject.SetActive(false);
                lockImageObject.SetActive(false);
                break;
            }
            case StageCellItem.StageCellType.Clear:
            {
                normalImageObject.SetActive(false);
                clearImageObject.SetActive(true);
                lockImageObject.SetActive(false);
                break;
            }
            case StageCellItem.StageCellType.Lock:
            {
                normalImageObject.SetActive(false);
                clearImageObject.SetActive(false);
                lockImageObject.SetActive(true);
                break;
            }
        }
    }

    public void OnClickStageCellButton()
    {
        if (_stageCellItem.stageCellType != StageCellItem.StageCellType.Clear) return;
    }
}
