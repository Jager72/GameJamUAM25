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
    public GameObject BattleArenaGO;
    public Image BattleArenaBackground;

    private DialogueLoader loader;
    private Dialogue currentDialogue;
    private int currentIndex;
    private bool isTyping, skipTyping;
    private Coroutine typingCoroutine;

    // track the current line data
    private DialogueLine curLine;

    void Start()
    {
        // Use FindObjectOfType for compatibility
        loader = FindAnyObjectByType<DialogueLoader>();
        if (loader == null)
        {
            Debug.LogError("DialogueManager: No DialogueLoader found in scene.");
            enabled = false;
            return;
        }

        dialoguePanel.SetActive(false);
        StartDialogue("prolog");
    }

    public void StartDialogue(string id)
    {
        Debug.Log($"[DialogueManager] StartDialogue('{id}')");

        // Stop any ongoing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // Load dialogue data
        currentDialogue = loader.GetDialogue(id);
        if (currentDialogue == null)
        {
            Debug.LogError($"[DialogueManager] No dialogue found with id='{id}'");
            EndDialogue();
            return;
        }
        if (currentDialogue.lines == null || currentDialogue.lines.Length == 0)
        {
            Debug.LogWarning($"[DialogueManager] Dialogue '{id}' has no lines.");
            EndDialogue();
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
        // Prevent advancing if still typing
        if (isTyping)
        {
            skipTyping = true;
            return;
        }

        // Guard against null dialogue or empty lines
        if (currentDialogue == null || currentDialogue.lines == null || currentDialogue.lines.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] NextLine called but no dialogue is active or lines are empty.");
            EndDialogue();
            return;
        }

        // Determine if we're on the last line
        bool atLast = currentIndex >= currentDialogue.lines.Length - 1;
        curLine = currentDialogue.lines[currentIndex];

        // If last line, no choices, and a nextId is defined, jump to next dialogue or battle
        if (atLast && (curLine.choices == null || curLine.choices.Length == 0) && !string.IsNullOrEmpty(curLine.nextId))
        {
            choicesPanel.SetActive(false);
            if (!string.IsNullOrEmpty(curLine.battle))
                DoBattle(curLine.battle);
            else
                StartDialogue(curLine.nextId);
            return; // exit without incrementing index
        }

        // Otherwise advance to the next line or end
        currentIndex++;
        if (currentDialogue.lines != null && currentIndex < currentDialogue.lines.Length)
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

        // Safe guard: ensure line exists
        if (currentDialogue == null || currentDialogue.lines == null || currentIndex < 0 || currentIndex >= currentDialogue.lines.Length)
        {
            Debug.LogWarning("[DialogueManager] TypeLine called with invalid index or dialogue.");
            EndDialogue();
            yield break;
        }

        curLine = currentDialogue.lines[currentIndex];
        nameText.text = curLine.speaker;
        dialogueText.text = string.Empty;

        // portrait
        if (!string.IsNullOrEmpty(curLine.sprite))
        {
            var sp = loader.GetPortrait(curLine.sprite);
            if (sp != null)
            {
                Debug.Log(sp);
                portraitImage.sprite = sp;
                portraitImage.enabled = true;

            }
            else
                Debug.LogWarning($"Portrait '{curLine.sprite}' not found");
        }
        else
        {
            portraitImage.enabled = false;
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
        Debug.Log($"[DialogueManager] Choice clicked, jumping to '{nextId}'");
        choicesPanel.SetActive(false);
        StartDialogue(nextId);
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(false);

        if (!string.IsNullOrEmpty(currentDialogue?.nextScene))
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
    bool tempFlag = false;
    void DoBattle(string battle)
    {
        if (tempFlag)
        {
            return;
        }
        tempFlag = true;
        Debug.Log($"Doing Battle {battle}");
        choicesPanel.SetActive(false);

        Transform arenaT = BattleArenaGO.transform;
        GameObject prefab = Resources.Load<GameObject>($"Scenes/{battle}");
        if (prefab == null)
        {
            Debug.LogError($"DoBattle: could not find prefab 'Scenes/{battle}' in Resources.");
            return;
        }

        BattleArenaBackground.gameObject.SetActive(true);
        Instantiate(prefab, arenaT.position, arenaT.rotation);
    }
}
