using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Controls")]
    [Tooltip("X axis Speed of the player")]
    [SerializeField] private float speed = 450f;
    [SerializeField] private float jumpForce = 10f;
    [Range(0f, 1f)] [SerializeField] private float stopFactor = 0.5f;
    [SerializeField] private Transform feetPos;
    [SerializeField] private Vector3 groundCheckSize = new Vector3(1, 0.05f, 1);
    [SerializeField] private float gravity = 10;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Better Platformer")]
    [SerializeField] private float hangTime = 0.1f;
    private float _hangTimeCtr = 0;
    [SerializeField] private float jumpBufferLength = 0.1f;
    private float _jumpBufferCtr = 0;

    [Header("Checkpoint Controls")]
    [SerializeField] private float checkpointDetectionRadius = 2f;
    private Transform _currentCheckpoint;

    [Header("Pull")]
    [SerializeField] private float _pullDistance = 1f;
    [SerializeField] LayerMask boxMask;
    [SerializeField] private string _pushableTag = "Pushable";
    GameObject box;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    // Animation Hashes
    private static readonly int A_Idle = Animator.StringToHash("Idle");
    private static readonly int A_Run = Animator.StringToHash("Run");
    private static readonly int A_Jump = Animator.StringToHash("Jump");
    private static readonly int A_Fall = Animator.StringToHash("Fall");

    private int _currentState;
    private float _lockedTill;

    // Private variables
    private Rigidbody2D _rb;
    private bool _isGrounded = false;

    //New Input System
    private bool _canJump = false;
    private float _moveInput;
    private bool _interactInput;
    private bool _jumpButtonPressed = false;
    private bool _jumpButtonReleased = false;

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void GetInput()
    {
        _isGrounded = Physics2D.OverlapBoxAll(feetPos.position, groundCheckSize, 0f, whatIsGround).Length > 0;
        _rb.gravityScale = _isGrounded ? 0 : gravity;

        if (_isGrounded)
        {
            _jumpButtonReleased = false;
            _hangTimeCtr = hangTime;
        }
        else { _hangTimeCtr -= Time.deltaTime; }

        if (_jumpButtonPressed)
        {
            _jumpButtonPressed = false;
            _jumpBufferCtr = jumpBufferLength;
            SoundManager.Instance.PlaySFX(SoundManager.Instance.p_jump);
        }
        else { _jumpBufferCtr -= Time.deltaTime; }

        if (_hangTimeCtr > 0f && _jumpBufferCtr > 0) {
            _canJump = true;
            _jumpBufferCtr = 0;
        }

        if (_jumpButtonReleased && _rb.velocity.y > 0) {
            _jumpButtonReleased = false;
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * stopFactor);
        }

        if (_interactInput)
        {
            ToggleCheckpoint();
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!Globals.Instance.playerCanJump) return;
        if (context.started) { _jumpButtonPressed = true; }
        if (context.canceled) { _jumpButtonReleased = true; }
    }

    private void Pull()
    {
        Physics2D.queriesStartInColliders = false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right * transform.localScale.x, _pullDistance, boxMask);

        Debug.DrawRay(transform.position, transform.right * transform.localScale.x * _pullDistance, Color.red, 0.1f);

        if (hit.collider != null && hit.collider.CompareTag(_pushableTag) && _interactInput) {
            box = hit.collider.gameObject;
            FixedJoint2D joint = box.GetComponent<FixedJoint2D>();
            joint.enabled = true;
            joint.connectedBody = this.GetComponent<Rigidbody2D>();
        }
        else if (box != null && !_interactInput)
        {
            box.GetComponent<FixedJoint2D>().enabled = false;
        }

    }

    private void Update()
    {
        if (!Globals.Instance.playerCanMove)
        {
            var s = A_Idle;
            if (s == _currentState) return;
            _animator.CrossFade(s, 0, 0);
            _currentState = s;
        }

        GetInput();
        Flip();
        Pull();

        var state = GetState();
        if (state == _currentState) return;
        _animator.CrossFade(state, 0, 0);
        _currentState = state;
    }

    private void FixedUpdate()
    {
        if (!Globals.Instance.playerCanMove)
        {
            _rb.velocity = Vector2.zero;
            return;
        }
        var vel = _rb.velocity;
        vel.x = _moveInput * speed * Time.fixedDeltaTime;
        _rb.velocity = vel;
        
        if (_canJump)
        {
            _rb.velocity = Vector2.zero;
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _canJump = false;
        }
    }

    //Grabbing checkpoint
    private void ToggleCheckpoint()
    {
        if (!_currentCheckpoint) {
            var colliders = Physics2D.OverlapCircleAll(transform.position, checkpointDetectionRadius);
            foreach (var col in colliders)
            {
                if (!col.CompareTag("Checkpoint")) continue;
                _currentCheckpoint = col.transform;
                _currentCheckpoint.SetParent(transform);
                Debug.Log("Checkpoint attached");
                break;
            }
        }
        else
        {
            _currentCheckpoint.SetParent(null);
            _currentCheckpoint = null;
            Debug.Log("Checkpoint detached");
        }
    }

    private void Flip()
    {
        transform.rotation = _moveInput switch
        {
            > 0 => Quaternion.Euler(0f, 0f, 0f),
            < 0 => Quaternion.Euler(0, 180f, 0),
            _ => transform.rotation
        };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(feetPos.position, groundCheckSize);
    }

    private int GetState()
    {
        if (Time.time < _lockedTill) return _currentState;

        if (_isGrounded) return _moveInput == 0 ? A_Idle : A_Run;
        return _rb.velocity.y > 0 ? A_Jump : A_Fall;

        int LockState(int s, float t)
        {
            _lockedTill = Time.time + t;
            return s;
        }
    }

    private void SwitchWithClone()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var mousePos2D = new Vector2(mousePosition.x, mousePosition.y);

        var hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider == null || !hit.collider.CompareTag("Clone")) return;
        // Swap positions between the player and the Clone
        (transform.position, hit.collider.transform.position) = (hit.collider.transform.position, transform.position);

        // Destroy the Clone
        Destroy(hit.collider.transform.gameObject);
    }

    //New Input System Functions

    public void Move(InputAction.CallbackContext context)
    {
        if(Globals.Instance.playerInputEnabled)
            _moveInput = context.ReadValue<Vector2>().x;
    }

    public void Grab(InputAction.CallbackContext context)
    {
        if (!Globals.Instance.playerCanInteract) return;
        if (context.started)
        {
            Debug.Log("Interact Pressed");
            _interactInput = true;
        }
        if (context.canceled)
        {
            Debug.Log("Interact Released");
            _interactInput = false;
        }
    }

    //To switch with clone
    public void Touch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SwitchWithClone();
        }
    }
}