using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDisabler : MonoBehaviour {
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInput playerInput;

    public void Update()
    {
        if (!Globals.Instance.playerEnabled)
        {
            playerMovement.enabled = false;
            playerInput.enabled = false;
        }
        else
        {
            playerMovement.enabled = true;
            playerInput.enabled = true;
        }
    }
    
}
