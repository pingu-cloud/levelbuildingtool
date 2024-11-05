using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow
{
    private LevelData currentLevelData;
    private LevelView levelView;
    private string newWord = "";
    private List<string> selectedWords = new List<string>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        // Begin scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        GUILayout.Label("Level Configuration", EditorStyles.boldLabel);

        // Option to create a new LevelData asset if none is assigned
        if (currentLevelData == null)
        {
            if (GUILayout.Button("Create New LevelData"))
            {
                CreateNewLevelData();
            }
        }

        // Select existing LevelData asset
        currentLevelData = (LevelData)EditorGUILayout.ObjectField("Level Data", currentLevelData, typeof(LevelData), false);

        // Select LevelView object from the scene
        levelView = (LevelView)EditorGUILayout.ObjectField("Level View", levelView, typeof(LevelView), true);

        if (currentLevelData != null)
        {
            DrawLevelDataSettings();
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a LevelData asset.", MessageType.Info);
        }

        if (levelView != null)
        {
            DrawLevelViewSettings();
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a LevelView instance from the scene.", MessageType.Info);
        }

        // Add a button to refresh the view in the scene
        if (GUILayout.Button("Refresh UI in Runtime"))
        {
            if (levelView != null && currentLevelData != null)
            {
                levelView.RefreshView(currentLevelData);
            }
            else
            {
                Debug.LogWarning("LevelView or LevelData is missing.");
            }
        }

        // End scroll view
        GUILayout.EndScrollView();
    }

    private void CreateNewLevelData()
    {
        // Create a new LevelData instance
        currentLevelData = CreateInstance<LevelData>();

        // Prompt user to save the new asset in the Project window
        string path = EditorUtility.SaveFilePanelInProject("Save New LevelData", "NewLevelData", "asset", "Specify where to save the LevelData asset.");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(currentLevelData, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = currentLevelData;
        }
    }

    private void DrawLevelDataSettings()
    {
        EditorGUILayout.Space();

        // Level number and scene name
        currentLevelData.levelNumber = EditorGUILayout.IntField("Level Number", currentLevelData.levelNumber);
        currentLevelData.sceneName = EditorGUILayout.TextField("Scene Name", currentLevelData.sceneName);
        currentLevelData.sentence = EditorGUILayout.TextField("Target Sentence", currentLevelData.sentence);

        EditorGUILayout.Space();

        // Add word functionality
        GUILayout.Label("Word List", EditorStyles.boldLabel);
        newWord = EditorGUILayout.TextField("New Word", newWord);
        if (GUILayout.Button("Add Word"))
        {
            if (!string.IsNullOrEmpty(newWord))
            {
                currentLevelData.wordList.Add(newWord);
                newWord = "";
            }
        }

        // Display word list
        for (int i = 0; i < currentLevelData.wordList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(currentLevelData.wordList[i]);
            if (GUILayout.Button("Remove"))
            {
                currentLevelData.wordList.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        // Word Selection for Sentence Formation
        GUILayout.Label("Form Sentence", EditorStyles.boldLabel);
        DrawWordButtons();
    }

    private void DrawLevelViewSettings()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Level View Settings", EditorStyles.boldLabel);

        SerializedObject serializedLevelView = new SerializedObject(levelView);

        // Button Prefab
        SerializedProperty buttonPrefabProp = serializedLevelView.FindProperty("buttonPrefab");
        EditorGUILayout.PropertyField(buttonPrefabProp);

        // Button Size, Buttons per Row, Spacing, and Start Offset
        SerializedProperty buttonSizeProp = serializedLevelView.FindProperty("buttonSize");
        EditorGUILayout.PropertyField(buttonSizeProp, new GUIContent("Button Size"));

        SerializedProperty buttonsPerRowProp = serializedLevelView.FindProperty("buttonsPerRow");
        EditorGUILayout.PropertyField(buttonsPerRowProp, new GUIContent("Buttons Per Row"));

        SerializedProperty spacingProp = serializedLevelView.FindProperty("spacing");
        EditorGUILayout.PropertyField(spacingProp, new GUIContent("Spacing"));

        SerializedProperty startOffsetProp = serializedLevelView.FindProperty("startOffset");
        EditorGUILayout.PropertyField(startOffsetProp, new GUIContent("Start Offset"));

        // Target Objects list
        SerializedProperty targetObjectsProp = serializedLevelView.FindProperty("targetObjects");
        EditorGUILayout.PropertyField(targetObjectsProp, true);

        serializedLevelView.ApplyModifiedProperties();
    }

    private void DrawWordButtons()
    {
        if (currentLevelData == null || currentLevelData.wordList == null) return;

        EditorGUILayout.BeginHorizontal();
        foreach (string word in currentLevelData.wordList)
        {
            if (GUILayout.Button(word, GUILayout.Width(80)))
            {
                selectedWords.Add(word);
                CheckSentenceCompletion();
            }
        }
        EditorGUILayout.EndHorizontal();

        // Display selected words
        EditorGUILayout.Space();
        GUILayout.Label("Selected Words: " + string.Join(" ", selectedWords));
    }

    private void CheckSentenceCompletion()
    {
        string targetSentence = currentLevelData.sentence;
        string formedSentence = string.Join(" ", selectedWords);

        if (formedSentence == targetSentence)
        {
            Debug.Log("Correct sentence formed!");
            // Additional actions upon correct sentence formation can be added here
        }
        else if (!targetSentence.StartsWith(formedSentence))
        {
            Debug.Log("Incorrect sequence. Resetting selected words.");
            selectedWords.Clear();
        }
    }
}
