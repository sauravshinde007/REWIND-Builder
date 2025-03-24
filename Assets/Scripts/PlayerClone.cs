using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerClone : MonoBehaviour
{
    private Stack<Vector3> actions;
    private Collider2D _currentButtonCollider;

    [SerializeField] private Vector3 boxSize;
    [SerializeField] private Animator _animator;

    // Animation Hashes
    private static readonly int A_Idle = Animator.StringToHash("Idle");
    private static readonly int A_Run = Animator.StringToHash("Run");
    private static readonly int A_Jump = Animator.StringToHash("Jump");
    private static readonly int A_Fall = Animator.StringToHash("Fall");

    private int _currentState;
    private Vector3 _previousPosition;
    private bool _isGrounded = false;

    [Header("Ground Check")]
    [SerializeField] private Transform feetPos;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.9f, 0.1f);
    [SerializeField] private LayerMask whatIsGround;

    public void SetActions(Stack<Vector3> a){
        actions = new Stack<Vector3>(a.Reverse());
    }

    private void Start()
    {
        _previousPosition = transform.position;
    }

    void Update(){
        if(actions.Count == 0) return;

        //next action from the stack
        Vector3 action = actions.Pop();

        StartCoroutine(MoveToPosition(action)); // Use a coroutine for movement

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

        CheckGrounded();
        UpdateAnimations();

        _previousPosition = transform.position;

        if (actions.Count == 0) Destroy(gameObject);
    }

    private IEnumerator MoveToPosition(Vector3 action)
    {
        FlipSprite(action.x - transform.position.x);

        transform.position = new Vector3(action.x, action.y, 0);
        yield return null;  // Allow animation update to register

        CheckGrounded();
        UpdateAnimations(); // Ensure proper animation updates

        _previousPosition = transform.position;

        if (actions.Count == 0) Destroy(gameObject);
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics2D.OverlapBox(feetPos.position, groundCheckSize, 0f, whatIsGround) != null;
    }

    private void UpdateAnimations()
    {
        Vector3 velocity = (transform.position - _previousPosition)/ Time.deltaTime;
        bool isMoving = Mathf.Abs(velocity.x) > 0.01f;  // Ensure a proper movement check

        int state;
        if (!_isGrounded)
        {
            state = velocity.y > 0 ? A_Jump : A_Fall;
        }
        else
        {
            state = isMoving ? A_Run : A_Idle;
        }

        if (state == _currentState) return;

        _animator.CrossFade(state, 0.1f, 0);
        _currentState = state;
    }

    private void FlipSprite(float direction)
    {
        if (direction == 0) return;

        Vector3 localScale = transform.localScale;
        localScale.x = direction > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
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