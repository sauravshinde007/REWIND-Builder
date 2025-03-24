using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public enum ActionType
{
    NONE,
    PRESS,
    RELEASE
}

public class PlayerActionStore : MonoBehaviour {
    
    #region Singleton
    public static PlayerActionStore Instance {get; private set;}

    private void Awake(){
    // If an instance already exists and it's not this, destroy this instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    public bool isRecording = false;
    [SerializeField] private GameObject playerClone;

    private Stack<Vector3> actions = new Stack<Vector3>();

    private void Update(){
        if(!isRecording) return;

        // 0 - No Action
        // 1 - Press button
        // 2 - Release button
        var actionType = 0;

        //Check player's button press
        if(Input.GetButtonDown("Interact")) actionType = (int)ActionType.PRESS;
        if(Input.GetButtonUp("Interact")) actionType = (int)ActionType.RELEASE;

        var vec3 = new Vector3(transform.position.x, transform.position.y, actionType);

        // Add to list
        actions.Push(vec3);
    }

    private void CreateClone(Vector2 pos){
        var pc = Instantiate(playerClone, pos, Quaternion.identity).GetComponent<PlayerClone>();
        pc.SetActions(actions);
    }

    public void StartRecording(){
        isRecording = true;
        StopClones();
    }

    private void KillAllClones(){
        var all = GameObject.FindObjectsOfType<PlayerClone>();
        foreach(var obj in all){
            Destroy(obj.gameObject);
        }
    }

    public void StopClones(){
        actions.Clear();
        StopAllCoroutines();
        KillAllClones();
    }

    public void StopRecording(Vector2 pos){
        if(!isRecording) return;

        isRecording = false;
        StopAllCoroutines();
        var cor = SpawnClones(pos);
        StartCoroutine(cor);
    }

    private IEnumerator SpawnClones(Vector2 pos){
        while(true){
            CreateClone(pos);
            yield return new WaitForSeconds(2);
        }
    }
}