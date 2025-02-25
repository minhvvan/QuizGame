using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StageCellItem
{
    public enum StageCellType { Normal, Clear, Lock }

    public StageCellType stageCellType;
    public int stageIndex;
}
