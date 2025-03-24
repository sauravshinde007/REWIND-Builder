using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DialogueNode : BaseNode
{
    [Input] public int entry;
    [Output] public int exit;

    public string npcName;
    [TextArea(2, 20)]
    public string dialogueLine;
    public Sprite npcSprite;
    
    public override string GetName() { return npcName; }

    public override Sprite GetSprite() { return npcSprite; } 
    
    public override string GetDialogueLine() { return dialogueLine; }
    
    
}