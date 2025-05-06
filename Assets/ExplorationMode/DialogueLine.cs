using System;

[Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
    public string sprite;
    public string background;
    public string battle; //Wheter to battle after choice
    public Choice[] choices;     // <— new
}