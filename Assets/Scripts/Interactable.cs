using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Interactable : MonoBehaviour {

    [SerializeField] private TMP_Text infoText;


    bool playerInRange{
        get { return _playerInRange; }
        set{
            _playerInRange = value;
            infoText.enabled = value;
        }
    }
    bool _playerInRange = false;

    void Start(){ infoText.enabled = false; }

    void Update(){
        if(playerInRange && Input.GetButtonDown("Interact")){
            Debug.Log("Interacted with: " + gameObject.name);
        }
    }

    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other){
        if(other.CompareTag("Player")) playerInRange = false;
    }
}
