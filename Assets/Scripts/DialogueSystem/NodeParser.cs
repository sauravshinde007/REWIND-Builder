using System.Collections;
using System.Linq;
using UnityEngine;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

// This should be a trigger
[RequireComponent(typeof(Collider2D))]
public class NodeParser : MonoBehaviour
{
    public DialogueGraph dialogueGraph;
    
    private Coroutine _parser;
    private Coroutine _dialogue;

    public void BeginConversation()
    {
        Globals.Instance.playerCanMove = false;
        NodeManager.Instance.dialoguePanel.SetActive(true);
        foreach (var node in dialogueGraph.nodes.Cast<BaseNode>().Where(node => node is StartNode))
        {
            dialogueGraph.current = node;
            break;
        }

        _parser = StartCoroutine(ParseNode());
    }

    private IEnumerator ParseNode()
    {
        var currentNode = dialogueGraph.current;
        switch (currentNode)
        {
            case StartNode:
                HandleStartNode();
                break;

            case EndNode:
                HandleEndNode();
                yield break;

            case DialogueNode dialogueNode:
                yield return HandleDialogueNode(dialogueNode);
                break;
            
            case ChoiceNode choiceNode:
                yield return HandleChoiceNode(choiceNode);
                break;
            
            case TaskNode taskNode:
                taskNode.ExecuteTask();
                NextNode("next");
                yield return null;
                break;

            default:
                throw new ArgumentOutOfRangeException(currentNode.GetName(), "Unhandled node type");
        }
    }
    
    private void NextNode(string fieldName)
    {
        if (_parser != null)
        {
            StopCoroutine(_parser);
            _parser = null;
        }
        
        foreach (var p in dialogueGraph.current.Ports)
        {
            if (p.fieldName != fieldName) continue;
            dialogueGraph.current = p.Connection.node as BaseNode;
            break;
        }
        
        _parser = StartCoroutine(ParseNode());
    }
    
    private void NextChoice(int choiceIndex)
    {
        if (dialogueGraph.current is not ChoiceNode)
        {
            Debug.Log("Wrong Usage, only use on choices");
            return;
        }
        
        if (_parser != null)
        {
            StopCoroutine(_parser);
            _parser = null;
        }
        
        var choiceName = "choices " + choiceIndex;
        
        foreach (var p in dialogueGraph.current.Ports)
        {
            if (p.fieldName != choiceName) continue;
            dialogueGraph.current = p.Connection.node as BaseNode;
            break;
        }
        
        _parser = StartCoroutine(ParseNode());
    }
    
    #region Handlers
    private void HandleStartNode()
    {
        NextNode("exit");
    }

    private void HandleEndNode()
    {
        Globals.Instance.playerCanMove = true;
        NodeManager.Instance.dialoguePanel.SetActive(false);
    }
    
    private IEnumerator HandleDialogueNode(DialogueNode dialogueNode)
    {
        // Ensure any existing dialogue coroutine is stopped
        if (_dialogue != null)
        {
            StopCoroutine(_dialogue);
            _dialogue = null;
        }

        // Start new dialogue display coroutine
        _dialogue = StartCoroutine(DisplayDialogue(dialogueNode));
        yield return _dialogue;

        // Wait for the user to release the mouse button before progressing
        yield return new WaitUntil(() => Input.GetButtonUp("Interact"));
        NextNode("exit");
    }
    
    private IEnumerator HandleChoiceNode(ChoiceNode choiceNode)
    {
        // Display choices to the player
        NodeManager.Instance.DisplayChoices(choiceNode.choices);

        // Wait for the player to select an option
        var selectedIndex = -1;
        
        yield return new WaitUntil(() => (selectedIndex = NodeManager.Instance.GetSelectedChoice()) >= 0);
        // Move to the next node based on the selected choice
        NextChoice(selectedIndex);
    }

    #endregion
    
    private IEnumerator DisplayDialogue(DialogueNode dialogueNode)
    {
        SetDialogueUI(dialogueNode);

        var fullDialogue = dialogueNode.GetDialogueLine();
        NodeManager.Instance.npcDialogue.text = "";

        foreach (var character in fullDialogue)
        {
            NodeManager.Instance.npcDialogue.text += character;

            // if (Input.GetMouseButtonDown(0))
            // {
            //     NodeManager.Instance.npcDialogue.text = fullDialogue;
            //     break;
            // }

            switch (character)
            {
                case '.':
                    yield return new WaitForSeconds(1 / (NodeManager.Instance.dialogueSpeed * 0.25f));
                    break;
                case ',':
                    yield return new WaitForSeconds(1 / (NodeManager.Instance.dialogueSpeed * 0.5f));
                    break;
            }
            yield return new WaitForSeconds(1 / NodeManager.Instance.dialogueSpeed);
        }
    }

    private static void SetDialogueUI(DialogueNode dialogueNode)
    {
        NodeManager.Instance.npcName.text = dialogueNode.GetName();
        NodeManager.Instance.npcImage.sprite = dialogueNode.GetSprite();
        NodeManager.Instance.npcDialogue.text = string.Empty;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BeginConversation();
        }
    }
}
