using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodeManager : MonoBehaviour
{
     [SerializeField] public GameObject dialoguePanel;
     [SerializeField] public GameObject choicesPanel;
     [SerializeField] private Transform choicesContainerContent;
     [SerializeField] private UnityEngine.UI.Button choiceButtonPrefab;
     
     public TMP_Text npcName;
     public TMP_Text npcDialogue;
     public Image npcImage;
     public float dialogueSpeed = 10;

     private int selectedChoice = -1;
     
     public static NodeManager Instance;

     private void Awake()
     {
          if (Instance == null)
          {
               Instance = this;
          }
          else
          {
               Destroy(gameObject);
          }
     }

     private void Start()
     {
          dialoguePanel.SetActive(false);
          choicesPanel.SetActive(false);
     }

     public void DisplayChoices(string[] choices)
     {
          selectedChoice = -1;
          choicesPanel.SetActive(true);
          // Clear Previous Choices
          foreach (Transform choice in choicesContainerContent)
          {
               Destroy(choice.gameObject);
          }
          
          // Fill choices
          for (var i = 0; i < choices.Length; i++)
          {
               var choice = choices[i];
               var choiceButton = Instantiate(choiceButtonPrefab, choicesContainerContent);
               choiceButton.GetComponentInChildren<TMP_Text>().text = choice;
               var temp = i;
               choiceButton.onClick.AddListener(() => ButtonPressListener(temp));
          }
     }

     private void ButtonPressListener(int choiceIndex)
     {
          choicesPanel.SetActive(false);
          selectedChoice = choiceIndex;
     }

     public int GetSelectedChoice()
     {
          return selectedChoice;
     }
}
