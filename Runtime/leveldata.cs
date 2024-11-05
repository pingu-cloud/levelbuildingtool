// Scripts/Model/LevelData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "PuzzleGame/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public string sceneName;
    public string sentence;                    // Correct sentence for this level
    public List<string> wordList;              // Pool of selectable words

    public List<RuntimeAnimatorController> solveAnimators;  // List of runtime animator controllers
    public List<string> solveTriggers;                     // List of trigger names for each animator
}
