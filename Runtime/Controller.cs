using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelController : MonoBehaviour
{
    public LevelData currentLevelData;   // Reference to the LevelData asset for this specific level
    public LevelView levelView;          // Reference to the LevelView for displaying UI

    private List<string> selectedWords = new List<string>();   // Tracks words selected by the user
    private List<string> sentenceWords;                        // The correct sequence of words from the sentence

    private void Start()
    {
        InitializeLevel();
    }

    private void InitializeLevel()
    {
        if (currentLevelData != null && levelView != null)
        {
            selectedWords.Clear(); // Clear previous selections
            sentenceWords = new List<string>(currentLevelData.sentence.Split(' ')); // Split sentence into words
            levelView.InitializeView(currentLevelData); // Initialize the view with current level data
            levelView.OnWordSelected += OnWordSelected; // Subscribe to word selection event
        }
        else
        {
            Debug.LogWarning("LevelData or LevelView is missing.");
        }
    }

    private void OnWordSelected(string word)
    {
        int nextIndex = selectedWords.Count;

        // Check if the word is correct and in the right order
        if (nextIndex < sentenceWords.Count && word == sentenceWords[nextIndex])
        {
            selectedWords.Add(word);
            Debug.Log($"Correct word selected: {word}");

            // Check if the entire sentence is correctly selected
            if (selectedWords.Count == sentenceWords.Count)
            {
                Debug.Log("Sentence completed correctly! Puzzle solved.");
                StartCoroutine(PlayEndAnimationAndTransition());
            }
        }
        else
        {
            Debug.Log($"Incorrect word selected or wrong order: {word}. Try again.");
            // Optionally, handle incorrect selection or reset selections if required
            // selectedWords.Clear();  // Uncomment this line if you want to reset selections on a wrong choice
        }
    }

    private void CheckSolution()
    {
        bool isSolved = sentenceWords.SequenceEqual(selectedWords);

        if (isSolved)
        {
            Debug.Log("Puzzle solved!");
            StartCoroutine(PlayEndAnimationAndTransition());
        }
    }

    private IEnumerator PlayEndAnimationAndTransition()
    {
        // Play the solve animations by triggering each Animator with the specified trigger
        if (currentLevelData.solveAnimators != null && currentLevelData.solveTriggers != null)
        {
            int animationCount = Mathf.Min(currentLevelData.solveAnimators.Count, currentLevelData.solveTriggers.Count, levelView.targetObjects.Count);

            for (int i = 0; i < animationCount; i++)
            {
                RuntimeAnimatorController animatorController = currentLevelData.solveAnimators[i];
                string triggerName = currentLevelData.solveTriggers[i];
                GameObject targetObject = levelView.targetObjects[i];

                if (animatorController != null && !string.IsNullOrEmpty(triggerName) && targetObject != null)
                {
                    Animator animator = targetObject.GetComponent<Animator>();

                    if (animator != null)
                    {
                        // Ensure the correct RuntimeAnimatorController is assigned
                        if (animator.runtimeAnimatorController != animatorController)
                        {
                            animator.runtimeAnimatorController = animatorController;
                        }

                        // Set the trigger to play the animation
                        animator.SetTrigger(triggerName);
                    }
                    else
                    {
                        Debug.LogWarning("Animator component not found on " + targetObject.name);
                    }
                }
                else
                {
                    Debug.LogWarning("RuntimeAnimatorController, trigger name, or target object is missing in LevelData.");
                }
            }
        }

        // Wait for the duration of the longest animation in the solveAnimators list (you may need to set an approximate wait time)
        yield return new WaitForSeconds(2f); // Set to a generic duration if exact time isn’t available

        // After waiting, proceed to the next level
        GoToNextLevel();
    }


    private void GoToNextLevel()
    {
        int nextLevelNumber = currentLevelData.levelNumber + 1;
        LevelData nextLevelData = FindNextLevelData(nextLevelNumber);

        if (nextLevelData != null)
        {
            if (!string.IsNullOrEmpty(nextLevelData.sceneName))
            {
                SceneManager.LoadScene(nextLevelData.sceneName);
            }
            else
            {
                Debug.LogWarning("Next level scene name is missing or invalid.");
            }
        }
        else
        {
            Debug.Log("All levels completed!");
            // Optionally, handle end-of-game behavior here
        }
    }

    // Helper function to find the next LevelData based on level number
    private LevelData FindNextLevelData(int levelNumber)
    {
        var levelData = Resources.FindObjectsOfTypeAll<LevelData>()
                                 .FirstOrDefault(level => level.levelNumber == levelNumber);

        if (levelData == null)
        {
            Debug.LogWarning($"Next LevelData for level {levelNumber} not found.");
        }

        return levelData;
    }

    private void OnDestroy()
    {
        if (levelView != null)
        {
            levelView.OnWordSelected -= OnWordSelected; // Unsubscribe to prevent memory leaks
        }
    }
}
