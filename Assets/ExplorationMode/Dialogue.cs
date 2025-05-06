using System;

[Serializable]
public class Dialogue
{
    public string id;
    public string background;
    public string nextScene;
    public DialogueLine[] lines;
}