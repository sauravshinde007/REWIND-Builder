using UnityEngine;
using XNode;

public class BaseNode : Node
{
    public virtual bool IsDialogueNode() { return false; }

    public virtual string GetName() { return null; }
    
    public virtual string GetDialogueLine() { return null; }

    public virtual Sprite GetSprite() { return null; }
}