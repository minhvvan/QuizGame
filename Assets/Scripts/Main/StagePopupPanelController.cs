using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PopupPanelController))]
public class StagePopupPanelController : MonoBehaviour
{
    [SerializeField] private GameObject stageCellPrefab;
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private ScrollViewController scrollViewController;
    
    private List<StageCellItem> _stageCellItems = new List<StageCellItem>();
    
    void Start()
    {
        GetComponent<PopupPanelController>().SetTitle("STAGE");

        int lastStageIndex = UserInformations.LastStageIndex;
        for (int i = 0; i < Constants.MAX_STAGE_COUNT; i++)
        {
            StageCellItem newCell = new();

            if (i < lastStageIndex)
            {
                newCell.stageCellType = StageCellItem.StageCellType.Clear;
            }
            else if (i == lastStageIndex)
            {
                newCell.stageCellType = StageCellItem.StageCellType.Normal;
            }
            else
            {
                newCell.stageCellType = StageCellItem.StageCellType.Lock;
            }

            newCell.stageIndex = i;
            _stageCellItems.Add(newCell);
        }
        
        scrollViewController.SetData(_stageCellItems);
    }
}