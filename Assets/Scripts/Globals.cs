using UnityEngine;

public class Globals : MonoBehaviour
{
    
    public static Globals Instance { get; private set; }

    private void Awake(){
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(this);
        }
    }
    
    // Editor Specific
    public bool inLevelEditor = false;

    // Player Enabled
    public bool playerEnabled = true;
    // Player Input
    public bool playerInputEnabled = true;
    // Full Movement
    public bool playerCanMove = true;
    // Jumping
    public bool playerCanJump = true;
    // Interactions
    public bool playerCanInteract = true;
}
