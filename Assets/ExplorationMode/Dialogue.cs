using System;

[Serializable]
public class Dialogue
{
    public string id;
    public string background;  // default background for this dialogue
    public string nextScene;   // empty = no scene load
    public DialogueLine[] lines;
}