using System;

[Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
}

[Serializable]
public class DialogueData
{
    public string questId;
    public DialogueLine[] lines;
}

[Serializable]
public class DialogueList
{
    public DialogueData[] dialogues;
}