using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Button : MonoBehaviour, IPressable, IMechanic
{
    [Header("Start Variables")]
    [Mechanic("Is Enabled")] public bool isEnabled = false; // Can Be Pressed By Player
    [Mechanic("Time Required to Press", 0, 2)] public float buttonHoldTime = 0.5f; // Hold Down For to activate


    [SerializeField] Slider timeIndicator; // Held for indicator

    [Header("Connections")]
    [SerializeField] private HashSet<Door> connectedTo = new HashSet<Door>();

    // Animations
    [FormerlySerializedAs("_animator")] [SerializeField] private Animator animator;
    [SerializeField] private float pressAnimationTime = 0.2f;

    // Animation Hashes
    private static readonly int A_Press = Animator.StringToHash("Press");
    private static readonly int A_Pressed_Idle = Animator.StringToHash("Pressed_Idle");
    private static readonly int A_Release = Animator.StringToHash("Release");
    private static readonly int A_Released_Idle = Animator.StringToHash("Released_Idle");

    private int _currentState;
    private float _lockedTill;
    private bool _canPress = false;
    private LineRenderer _lineRenderer;

    // Private vars
    private float TimePressed
    {
        get => _TimePressed;
        set
        {
            _TimePressed = value;

            // Update Slider if it exists
            if (timeIndicator)
            {
                timeIndicator.maxValue = buttonHoldTime;
                timeIndicator.value = value;
            }

            // Run Release function when done pressing
            if (value >= buttonHoldTime)
            {
                OnRelease();
                Toggle();
            }
        }
    }
    private float _TimePressed = 0f;
    [SerializeField] private InputActionReference interact;


    #region IPressable
    public bool IsPressed { get; set; }
    public bool IsOn { get; set; }

    // Events
    public delegate void VoidFunc();

    public event VoidFunc OnPress;
    public event VoidFunc OnRelease;

    // Execute when pressed
    public virtual void Press()
    {
        IsPressed = true;
    }
    // Execute when released
    public virtual void Release()
    {
        IsPressed = false;
        TimePressed = 0f; // Reset time to 0 on release
    }

    // Don't think these need to be virtual
    public void Enable() { isEnabled = true; }
    public void Disable() { isEnabled = false; }
    #endregion

    void Start()
    {
        // Set slider
        if (timeIndicator != null)
        {
            timeIndicator.maxValue = buttonHoldTime;
            timeIndicator.gameObject.SetActive(false);
        }

        OnPress += Press;
        OnRelease += Release;

    }

    private void OnEnable()
    {
        if (interact == null || interact.action == null) return;
        
        interact.action.performed += ctx => { OnPress?.Invoke(); };
        interact.action.canceled += ctx => { OnRelease?.Invoke(); };
        interact.action.Enable();
    }

    private void OnDisable()
    {
        if (interact != null && interact.action != null)
        {
    
            interact.action.performed -= ctx => OnPress?.Invoke();
            interact.action.canceled -= ctx => OnRelease?.Invoke();
            interact.action.Disable();
        }
    }

    private void Toggle()
    {
        IsOn = !IsOn;
        foreach (var c in connectedTo.Where(c => c))
        {
            c.Toggle();
        }
    }

    private void Update()
    {
        // Return if not enabled, not in range or no input specified
        if (!isEnabled) return;

        if(!_canPress) return;
        if (IsPressed) TimePressed += Time.deltaTime;

        var state = GetState();
        if(state == _currentState) return;
        animator.CrossFade(state, 0, 0);
        _currentState = state;

    }

    // This works better than OnTriggerStay
    private void OnTriggerEnter2D(Collider2D other)
    {
        _canPress = true;
        timeIndicator?.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _canPress = false;
        timeIndicator?.gameObject.SetActive(false);
    }

    // Debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        foreach (var c in connectedTo.Where(c => c != null))
        {
            Gizmos.DrawLine(transform.position, c.transform.position);
        }
    }

    private int GetState(){
        if(Time.time < _lockedTill) return _currentState;

        if(IsPressed && _currentState == A_Released_Idle) return LockState(A_Press, pressAnimationTime);
        else if(!IsPressed && _currentState == A_Pressed_Idle) return LockState(A_Release, pressAnimationTime);

        if(IsPressed) return A_Pressed_Idle;
        else return A_Released_Idle;

        int LockState(int s, float t){
            _lockedTill = Time.time + t;
            return s;
        }
    }

    public bool Connect(GameObject other)
    {
        if (!other.CompareTag("Door")) return false;
        
        var door = other.GetComponent<Door>();
        connectedTo.Add(door);
        return true;
    }

    public void ChangeValues(IMechanic newMechanic)
    {
        var mech = (Button)newMechanic;
        enabled = mech;
        buttonHoldTime = mech.buttonHoldTime;
    }

    //New Input System

}