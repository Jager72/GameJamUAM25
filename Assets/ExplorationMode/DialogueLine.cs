using System;


[Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
    public string sprite;      // filename of PNG without extension, or "" to keep previous
    public string background;  // same for backgrounds
}