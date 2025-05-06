using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image backgroundImage;
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.03f;

    [Header("Choices UI")]
    public GameObject choicesPanel;
    public Button choiceButtonPrefab;

    private DialogueLoader loader;
    private Dialogue currentDialogue;
    private int currentIndex;
    private bool isTyping, skipTyping;
    private Coroutine typingCoroutine;

    // track the current line data
    private DialogueLine curLine;

    void Start()
    {
        loader = FindAnyObjectByType<DialogueLoader>();
        if (loader == null)
        {
            Debug.LogError("DialogueManager: No DialogueLoader found in scene.");
            enabled = false;
            return;
        }

        dialoguePanel.SetActive(false);
        StartDialogue("steal");
    }

    public void StartDialogue(string id)
    {
        Debug.Log($"[DialogueManager] StartDialogue('{id}')");

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentDialogue = loader.GetDialogue(id);
        if (currentDialogue == null)
        {
            Debug.LogError($"[DialogueManager] No dialogue found with id='{id}'");
            return;
        }

        // default background
        if (!string.IsNullOrEmpty(currentDialogue.background))
        {
            var bg = loader.GetBackground(currentDialogue.background);
            if (bg != null)
                backgroundImage.sprite = bg;
        }

        currentIndex = 0;
        dialoguePanel.SetActive(true);
        choicesPanel.SetActive(false);
        typingCoroutine = StartCoroutine(TypeLine());
    }

    public void NextLine()
    {
        if (isTyping)
        {
            skipTyping = true;
            return;
        }

        // If this is the last line in the dialogue
        bool atLast = currentIndex >= currentDialogue.lines.Length - 1;
        curLine = currentDialogue.lines[currentIndex];

        // If no choices on this line and a nextId is defined, jump to that dialogue
        if (atLast && (curLine.choices == null || curLine.choices.Length == 0)
            && !string.IsNullOrEmpty(curLine.nextId))
        {
            StartDialogue(curLine.nextId);
            return;
        }

        // Otherwise, advance or end
        currentIndex++;
        if (currentIndex < currentDialogue.lines.Length)
        {
            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        skipTyping = false;

        curLine = currentDialogue.lines[currentIndex];
        nameText.text = curLine.speaker;
        dialogueText.text = string.Empty;

        // portrait
        if (!string.IsNullOrEmpty(curLine.sprite))
        {
            var sp = loader.GetPortrait(curLine.sprite);
            if (sp != null)
            {
                portraitImage.sprite = sp;
                portraitImage.enabled = true;
            }
            else
                Debug.LogWarning($"Portrait '{curLine.sprite}' not found");
        }

        // background (per-line override)
        if (!string.IsNullOrEmpty(curLine.background))
        {
            var bg = loader.GetBackground(curLine.background);
            if (bg != null)
                backgroundImage.sprite = bg;
        }

        // typewriter effect
        foreach (char c in curLine.text)
        {
            if (skipTyping) break;
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        dialogueText.text = curLine.text;
        isTyping = false;

        // present choices if they exist
        if (curLine.choices != null && curLine.choices.Length > 0)
        {
            ShowChoices(curLine.choices);
        }
    }

    void ShowChoices(Choice[] choices)
    {
        foreach (Transform t in choicesPanel.transform)
            Destroy(t.gameObject);

        foreach (var c in choices)
        {
            var btn = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            btn.GetComponentInChildren<TMP_Text>().text = c.text;
            btn.onClick.AddListener(() => OnChoiceSelected(c.nextId));
        }

        choicesPanel.SetActive(true);
    }

    void OnChoiceSelected(string nextId)
    {
        Debug.Log($"[DialogueManager] Choice clicked, nextId='{nextId}'");
        choicesPanel.SetActive(false);
        StartDialogue(nextId);
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(false);

        if (!string.IsNullOrEmpty(currentDialogue.nextScene))
            SceneManager.LoadScene(currentDialogue.nextScene);
    }

    void Update()
    {
        if (choicesPanel.activeSelf) return;

        if ((Mouse.current?.leftButton.wasPressedThisFrame == true) ||
            (Keyboard.current?.spaceKey.wasPressedThisFrame == true))
        {
            NextLine();
        }
    }
}
