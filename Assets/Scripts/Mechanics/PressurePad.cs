using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PressurePad : MonoBehaviour, IPressable, IMechanic
{
    [Header("Start Variables")]
    [Mechanic("Is Enabled")] public bool isEnabled = false; // Can Be Pressed By Player

    [Header("Connections")] private HashSet<Door> connectedTo = new HashSet<Door>();
    
    // Animations
    [SerializeField] private Animator _animator;
    [SerializeField] private float pressAnimationTime = 0.2f;
    
    private List<Collider2D> colliders = new List<Collider2D>();

    // Animation Hashes
    private static readonly int A_Press = Animator.StringToHash("Press");
    private static readonly int A_Pressed_Idle = Animator.StringToHash("Pressed_Idle");
    private static readonly int A_Release = Animator.StringToHash("Release");
    private static readonly int A_Released_Idle = Animator.StringToHash("Released_Idle");

    private int _currentState;
    private float _lockedTill;
    private LineRenderer _lineRenderer;

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
        SetState(true);
    }
    // Execute when released
    public virtual void Release()
    {
        IsPressed = false;
        SetState(false);
    }

    // Don't think these need to be virtual
    public void Enable() { isEnabled = true; }
    public void Disable() { isEnabled = false; }
    #endregion

    private void Start()
    {
        OnPress += Press;
        OnRelease += Release;
    }

    private void SetState(bool on)
    {
        IsOn = on;
        foreach (var c in connectedTo.Where(c => c != null))
        {
            if (IsOn) c.AddKey();
            else c.RemoveKey();
        }
    }

    private void Update()
    {
        // Return if not enabled, not in range or no input specified
        if (!isEnabled) return;

        var state = GetState();
        if(state == _currentState) return;
        _animator.CrossFade(state, 0, 0);
        _currentState = state;
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        colliders.Add(other);
        if (IsPressed) return;
        var rb = other.GetComponent<Rigidbody2D>();
        if(rb == null) return;
        
        if(colliders.Count >= 1) OnPress();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        colliders.Remove(other);
        if(colliders.Count == 0) OnRelease();
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
        var mech = (PressurePad)newMechanic;
        isEnabled = mech.isEnabled;
    }
}