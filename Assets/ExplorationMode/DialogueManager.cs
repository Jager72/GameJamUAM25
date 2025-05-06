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
    public GameObject choicesPanel;         // the container for your choice buttons
    public Button choiceButtonPrefab;   // prefab with Button + a TMP_Text child

    private DialogueLoader loader;
    private Dialogue currentDialogue;
    private int currentIndex;
    private bool isTyping, skipTyping;
    private Coroutine typingCoroutine;

    void Start()
    {
        loader = FindAnyObjectByType<DialogueLoader>();
        dialoguePanel.SetActive(false);
        StartDialogue("ser_rozmowa");
    }

    public void StartDialogue(string id)
    {
        // If we're mid‐typing, stop that coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentDialogue = loader.GetDialogue(id);
        if (currentDialogue == null) return;

        // Apply default background
        if (!string.IsNullOrEmpty(currentDialogue.background))
        {
            var bg = loader.GetBackground(currentDialogue.background);
            if (bg != null) backgroundImage.sprite = bg;
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

        var line = currentDialogue.lines[currentIndex];
        nameText.text = line.speaker;
        dialogueText.text = "";

        // PORTRAIT
        if (!string.IsNullOrEmpty(line.sprite))
        {
            var sp = loader.GetPortrait(line.sprite);
            if (sp != null)
            {
                portraitImage.sprite = sp;
                portraitImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Portrait '{line.sprite}' not found");
            }
        }

        // BACKGROUND
        if (!string.IsNullOrEmpty(line.background))
        {
            var bg = loader.GetBackground(line.background);
            if (bg != null)
            {
                backgroundImage.sprite = bg;
            }
        }

        // TYPEWRITER
        foreach (char c in line.text)
        {
            if (skipTyping) break;
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        dialogueText.text = line.text;
        isTyping = false;

        // CHOICES
        if (line.choices != null && line.choices.Length > 0)
        {
            ShowChoices(line.choices);
        }
    }

    void ShowChoices(Choice[] choices)
    {
        // Clear old
        foreach (Transform t in choicesPanel.transform)
            Destroy(t.gameObject);

        // Build new
        foreach (var c in choices)
        {
            var btn = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            btn.GetComponentInChildren<TMP_Text>().text = c.text;
            // assign the handler
            btn.onClick.AddListener(() => OnChoiceSelected(c.nextId));
        }

        choicesPanel.SetActive(true);
    }

    void OnChoiceSelected(string nextId)
    {
        Debug.Log($"[DialogueManager] Choice clicked, jumping to '{nextId}'");
        StartDialogue(nextId);
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(false);

        if (!string.IsNullOrEmpty(currentDialogue.nextScene))
        {
            SceneManager.LoadScene(currentDialogue.nextScene);
        }
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
