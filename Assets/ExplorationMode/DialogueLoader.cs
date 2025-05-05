using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DialogueLoader : MonoBehaviour
{
    private DialogueDatabase db;
    private Dictionary<string, Sprite> portraits;
    private Dictionary<string, Sprite> backgrounds;

    void Awake()
    {
        TextAsset ta = Resources.Load<TextAsset>("Dialogues/dialogues");
        if (ta == null)
        {
            Debug.LogError("Nie znaleziono dialogues.json!");
            return;
        }
        Debug.Log($"[Loader] Raw JSON:\n{ta.text}");
        db = JsonUtility.FromJson<DialogueDatabase>(ta.text);

        // 2) Cache all Portrait sprites by FILE name
        portraits = Resources
            .LoadAll<Sprite>("Portraits")
            .ToDictionary(s => s.texture.name, s => s);
        Debug.Log($"[Loader] Portrait keys: {string.Join(", ", portraits.Keys)}");

        // 3) Cache all Background sprites by FILE name
        backgrounds = Resources
            .LoadAll<Sprite>("Backgrounds")
            .ToDictionary(s => s.texture.name, s => s);
        Debug.Log($"[Loader] Background keys: {string.Join(", ", backgrounds.Keys)}");
    }

    public Dialogue GetDialogue(string id)
    {
        var dlg = db.dialogues.FirstOrDefault(d => d.id == id);
        if (dlg == null) Debug.LogError($"Brak dialogu o id '{id}'");
        return dlg;
    }

    public Sprite GetPortrait(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        return portraits.TryGetValue(key, out var sp) ? sp : null;
    }

    public Sprite GetBackground(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        return backgrounds.TryGetValue(key, out var bg) ? bg : null;
    }
}
