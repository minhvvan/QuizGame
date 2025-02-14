using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class QuizDataController
{
    static string ROW_SEPARATOR = @"\r\n|\n\r|\n|\r";
    static string COL_SEPARATOR = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    private static char[] TRIM_CHARS = { '\"' };
    
    public static List<QuizData> LoadQuizData(int stageIndex)
    {
        var fileName = "QuizData-" + stageIndex;
        
        TextAsset quizDataAsset = Resources.Load(fileName) as TextAsset;
        var lines = Regex.Split(quizDataAsset.text, ROW_SEPARATOR);
        
        var quizDataList = new List<QuizData>();
        
        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], COL_SEPARATOR);
            QuizData quizData = new QuizData();

            for (var j = 0; j < values.Length; j++)
            {
                var value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                
                switch (j)
                {
                    case 0:
                        quizData.question = value;
                        break;
                    case 1:
                        quizData.description = value;
                        break;
                    case 2:
                        quizData.type = int.Parse(value);
                        break;
                    case 3:
                        quizData.answer = int.Parse(value);
                        break;
                    case 4:
                        quizData.firstOption = value;
                        break;
                    case 5:
                        quizData.secondOption = value;
                        break;
                    case 6:
                        quizData.thirdOption = value;
                        break;
                }
            }
            
            // QuizData quizData = new QuizData();
            // quizData.question = values[0];
            // quizData.description = values[1];
            // quizData.type = int.Parse(values[2]);
            // quizData.answer = int.Parse(values[3]);
            //
            // string[] options = { values[4], values[5], values[6] };
            // quizData.options = options;
            
            quizDataList.Add(quizData);
        }
        return quizDataList;
    }
}