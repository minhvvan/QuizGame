using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizCardController : MonoBehaviour
{
    [SerializeField] private TMP_Text _tmpText;

    public void SetText(string str)
    {
        _tmpText.SetText(str);
    }
}
