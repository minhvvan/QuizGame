using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] private TMP_Text stageIndexText;
    [SerializeField] private TMP_Text stageTitleText;
    [SerializeField] private TMP_Text clearedStageIndexText;

    public int Index { get; private set; }

    public void SetItem(StageCellItem stageCellItem, int index)
    {
        stageIndexText.text = $"{stageCellItem.stageIndex + 1}";
        clearedStageIndexText.text = $"{stageCellItem.stageIndex + 1}";

        Index = index;
    }
}