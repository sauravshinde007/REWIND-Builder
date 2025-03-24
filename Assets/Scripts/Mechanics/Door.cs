using UnityEngine;

public class Door : MonoBehaviour, IOpenable, IMechanic{
    [Mechanic("Required Keys", 0, 4)]public int requiredKeys = 1;
    [Mechanic("Close After Time", 2f, 20f)]public float closeAfterTime = 5f;
    [Mechanic("Door Color", 0)] public int doorColorIndex = 0;
    [Mechanic("Close At Start")]public bool closeAtStart = true;
    [Mechanic("Stay Open")]public bool stayOpen = true;
    [Mechanic("Show On Open")]public bool showOnOpen = false;
    [Mechanic("Show On Close")]public bool showOnClose = false;

    
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private Animator _animator;

    // Animation Hashes
    private static readonly int A_Open = Animator.StringToHash("Open");
    private static readonly int A_Open_Idle = Animator.StringToHash("Open_Idle");
    private static readonly int A_Close = Animator.StringToHash("Close");
    private static readonly int A_Close_Idle = Animator.StringToHash("Close_Idle");

    [SerializeField] private float openTime = 0.2f;

    private int _currentState;
    private float _lockedTill;
    private float _openTimeCtr;

    private void Start()
    {
        Setup();
    }

    public void Setup()
    {
        if (closeAtStart) RemoveKey();
        else AddKey();
    }

    private void Update(){

        // Close door after certain time
        if (!stayOpen && IsOpen)
        {
            _openTimeCtr += Time.deltaTime;
            if (_openTimeCtr >= closeAfterTime)
            {
                _openTimeCtr = 0;
                RemoveKey();
            }
        }
        
        var state = GetState();
        if(state == _currentState) return;
        _animator.CrossFade(state, 0, 0);
        _currentState = state;
    }

    #region IOpenable

    public int Keys { get; set; }
    public bool IsOpen {get;set;}

    public void Toggle(){
        IsOpen = !IsOpen;
        if (IsOpen)
        {
            AddKey();
        }
        else
        {
            RemoveKey();
        }
    }

    public virtual void AddKey() {
        Keys++;
        if(Keys >= requiredKeys){
            Keys = requiredKeys;
            Open();
        }
    }
    public virtual void RemoveKey() {
        Keys--;
        if(Keys < requiredKeys) Close();
        if(Keys <= 0){ 
            Keys = 0;
            Close();
        }
    }

    public virtual void Open(){ 
        if(showOnOpen) GetComponent<CutsceneTrigger>().CutSceneLogic();
        doorCollider.enabled = false;
        IsOpen = true;
    }
    public virtual void Close(){ 
        if(showOnClose) GetComponent<CutsceneTrigger>().CutSceneLogic();
        doorCollider.enabled = true;
        IsOpen = false;
    }
    #endregion

    private int GetState(){
        if(Time.time < _lockedTill) return _currentState;

        if(IsOpen && _currentState == A_Close_Idle) return LockState(A_Open, openTime);
        else if(!IsOpen && _currentState == A_Open_Idle) return LockState(A_Close, openTime);

        if(IsOpen) return A_Open_Idle;
        else return A_Close_Idle;

        int LockState(int s, float t){
            _lockedTill = Time.time + t;
            return s;
        }
    }

    public bool Connect(GameObject other)
    {
        return false;
    }

    public void ChangeValues(IMechanic newMechanic)
    {
        var mech = (Door)newMechanic;
        requiredKeys = mech.requiredKeys;
    }
}