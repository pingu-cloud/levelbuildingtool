using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelView : MonoBehaviour
{
    public Button buttonPrefab;                     // The button prefab to instantiate
    public Vector2 buttonSize = new Vector2(100, 50); // Width and height of each button
    public int buttonsPerRow = 3;                   // Number of buttons per row
    public Vector2 spacing = new Vector2(10, 10);   // Spacing between buttons
    public Vector2 startOffset = new Vector2(50, 50); // Offset from the screen's top-left corner
    public delegate void WordSelectedDelegate(string word);
    public event WordSelectedDelegate OnWordSelected;
    public List<GameObject> targetObjects;
    private Canvas canvas;
    private TMP_InputField inputField;              // Input field created dynamically

    private List<Button> instantiatedButtons = new List<Button>(); // Track instantiated buttons

    void Start()
    {
        CreateCanvas();
    }

    public void InitializeView(LevelData levelData)
    {
        if (canvas == null)
        {
            CreateCanvas();
        }

        if (levelData != null && levelData.wordList != null)
        {
            PopulateButtons(levelData.wordList);
        }
        else
        {
            Debug.LogWarning("LevelData or wordList is missing.");
        }
    }

    void CreateCanvas()
    {
        if (canvas != null) return;  // Prevent duplicate canvas creation

        // Create a new Canvas GameObject
        GameObject canvasObject = new GameObject("DynamicCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Add CanvasScaler to scale with screen size
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(Screen.width, Screen.height);

        // Add a GraphicRaycaster so buttons can receive input
        canvasObject.AddComponent<GraphicRaycaster>();

        // Create a panel RectTransform to hold the input field and buttons
        GameObject panelObject = new GameObject("Panel");
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelObject.transform.SetParent(canvas.transform, false);
        panelObject.name = "Panel";

        // Anchor the panel to the top-left corner and set it to start from the specified offset
        panelRect.anchorMin = new Vector2(0, 1);    // Top-left corner
        panelRect.anchorMax = new Vector2(0, 1);    // Top-left corner
        panelRect.pivot = new Vector2(0, 1);        // Pivot at the top-left
        panelRect.anchoredPosition = new Vector2(startOffset.x, -startOffset.y); // Start from the specified offset

        // Create a new input field GameObject
        GameObject inputFieldObject = new GameObject("InputField");
        inputFieldObject.transform.SetParent(canvas.transform, false);

        // Add and configure TMP_InputField and its components
        inputField = inputFieldObject.AddComponent<TMP_InputField>();
        RectTransform inputFieldRect = inputFieldObject.AddComponent<RectTransform>();
        inputFieldRect.anchorMin = new Vector2(0, 1); // Anchor to top-left
        inputFieldRect.anchorMax = new Vector2(1, 1); // Anchor to top-right
        inputFieldRect.pivot = new Vector2(0.5f, 1);  // Pivot at the top-center
        inputFieldRect.anchoredPosition = new Vector2(0, -startOffset.y);
        inputFieldRect.sizeDelta = new Vector2(0, buttonSize.y); // Set height to button height

        // Add TextMeshProUGUI for input text display
        TMP_Text inputText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        inputText.text = ""; // Start with empty text
        inputText.fontSize = 24;
        inputText.alignment = TextAlignmentOptions.Left;
        inputText.transform.SetParent(inputFieldObject.transform, false);
        inputField.textComponent = inputText;

        // Add placeholder text
        TMP_Text placeholder = new GameObject("Placeholder").AddComponent<TextMeshProUGUI>();
        placeholder.text = "Enter words here...";
        placeholder.fontSize = 24;
        placeholder.color = Color.gray;
        placeholder.alignment = TextAlignmentOptions.Left;
        placeholder.transform.SetParent(inputFieldObject.transform, false);
        inputField.placeholder = placeholder;
    }

    void PopulateButtons(List<string> wordList)
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("Button prefab is not assigned.");
            return;
        }

        RectTransform panelRect = canvas.transform.Find("Panel").GetComponent<RectTransform>();

        for (int i = 0; i < wordList.Count; i++)
        {
            // Instantiate a new button as a child of the panel
            Button newButton = Instantiate(buttonPrefab, panelRect);
            instantiatedButtons.Add(newButton); // Track instantiated buttons for future cleanup

            // Set button text and configure auto-size settings
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = wordList[i];
                buttonText.enableAutoSizing = true;
                buttonText.fontSizeMin = 8;
                buttonText.fontSizeMax = 30;
            }

            // Adjust the size of the button and set the anchor/pivot to top-left
            RectTransform buttonRect = newButton.GetComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(0, 1);
            buttonRect.pivot = new Vector2(0, 1);
            buttonRect.localScale = Vector3.one;

            // Calculate position in a grid format, starting from below the input field
            int row = i / buttonsPerRow;
            int column = i % buttonsPerRow;
            float xPos = column * (buttonSize.x + spacing.x);
            float yPos = -row * (buttonSize.y + spacing.y) - buttonSize.y;

            // Set the anchored position of the button
            buttonRect.anchoredPosition = new Vector2(xPos, yPos);

            // Add button click event
            string word = wordList[i];
            newButton.onClick.AddListener(() => OnWordButtonPressed(word));
        }
    }

    public void RefreshView(LevelData levelData)
    {
        // Clear existing buttons
        foreach (var button in instantiatedButtons)
        {
            Destroy(button.gameObject);
        }
        instantiatedButtons.Clear();

        // Populate new buttons with the updated word list
        if (levelData != null)
        {
            PopulateButtons(levelData.wordList);
        }
    }

    private void OnWordButtonPressed(string word)
    {
        // Append the word to the input field with a space
        inputField.text += word + " ";
        OnWordSelected?.Invoke(word);
    }
}
