using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

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

    private DialogueLoader loader;
    private Dialogue currentDialogue;
    private int currentIndex;
    private bool isTyping, skipTyping;

    void Start()
    {
        loader = FindAnyObjectByType<DialogueLoader>();
        dialoguePanel.SetActive(false);
        StartDialogue("prolog");
    }

    public void StartDialogue(string id)
    {
        currentDialogue = loader.GetDialogue(id);
        if (currentDialogue == null) return;

        // apply default background if specified
        if (!string.IsNullOrEmpty(currentDialogue.background))
        {
            var bg = loader.GetBackground(currentDialogue.background);
            if (bg != null) backgroundImage.sprite = bg;
        }

        currentIndex = 0;
        dialoguePanel.SetActive(true);
        StartCoroutine(TypeLine());
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
            StartCoroutine(TypeLine());
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

        Debug.Log($"Attempting portrait swap for key: '{line.sprite}'");

        // — Portrait (only if non-empty key) —
        if (!string.IsNullOrEmpty(line.sprite))
        {
            Debug.Log(line.sprite);
            var sp = loader.GetPortrait(line.sprite);
            Debug.Log(sp);
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

        // — Background (only if non-empty key) —
        if (!string.IsNullOrEmpty(line.background))
        {
            var bg = loader.GetBackground(line.background);
            if (bg != null)
            {
                backgroundImage.sprite = bg;
                portraitImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Background '{line.background}' not found");
            }
        }

        // — Typewriter effect —
        foreach (char c in line.text)
        {
            if (skipTyping) break;
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        dialogueText.text = line.text;
        isTyping = false;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        if (!string.IsNullOrEmpty(currentDialogue.nextScene))
        {
            SceneManager.LoadScene(currentDialogue.nextScene);
        }
    }

    void Update()
    {
        if ((Mouse.current?.leftButton.wasPressedThisFrame == true) ||
            (Keyboard.current?.spaceKey.wasPressedThisFrame == true))
        {
            NextLine();
        }
    }
}
