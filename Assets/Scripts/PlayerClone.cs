using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerClone : MonoBehaviour
{
    private Stack<Vector3> actions = new Stack<Vector3>();
    private Collider2D _currentButtonCollider;

    [SerializeField] private Vector3 boxSize;
    //[SerializeField] private LayerMask buttonLayer;

    public void SetActions(Stack<Vector3> a){
        actions = new Stack<Vector3>(a.Reverse());
    }

    void Update(){
        if(actions.Count == 0) return;

        //next action from the stack
        Vector3 action = actions.Pop();


        transform.position = new Vector3(action.x, action.y, 0);

        //Perform action based on the action type
        int actionType = (int)action.z;

        switch(actionType){
            case 1:
                PerformButtonPress();
                break;
            case 2:
                PerformButtonRelease();
                break;
        }
        if (actions.Count == 0) Destroy(gameObject);
    }

    private void PerformButtonPress()
    {
        if (_currentButtonCollider == null) return;

        //Simulate OnPress
        var button = _currentButtonCollider.GetComponent<Button>();
        if (button != null) {
            //Debug.Log("Pressing button");
            button.Press();
        }
    }

    private void PerformButtonRelease()
    {
        if (_currentButtonCollider == null) return;

        //Simulate OnPress
        var button = _currentButtonCollider.GetComponent<Button>();
        if (button != null)
        {
            //Debug.Log("Releasing button");
            button.Release();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Button"))
        {
            
            _currentButtonCollider = collision;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Button"))
        { 
            _currentButtonCollider = null;
        }
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}