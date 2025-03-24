using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour, IMechanic
{
    [Mechanic("Start Point")] public bool isStartPoint;

    public Checkpoint otherPoint;
    [SerializeField] private Animator _animator;
    [SerializeField] private LineRenderer _connectionWirePrefab;

    // Animation Hashes
    private static readonly int A_Playing = Animator.StringToHash("Playing");
    private static readonly int A_Recording = Animator.StringToHash("Recording");
    private static readonly int A_Idle = Animator.StringToHash("Idle");

    private bool _isRecording = false;
    private bool _isPlaying = false;
    private int _currentState = 0;
    private LineRenderer _lineRenderer;

    private bool _connected = false;

    void Update(){
        var state = GetState();
        if(state == _currentState) return;
        _animator.CrossFade(state, 0, 0);
        _currentState = state;
    }

    void OnTriggerEnter2D(Collider2D other){
        // If Player in range
        if(other.CompareTag("Player")){
            if (_isPlaying){

                if (isStartPoint){
                    _isPlaying = false;
                    otherPoint._isPlaying = false;

                    _isRecording = true;
                    otherPoint._isRecording = true;
                    PlayerActionStore.Instance.StartRecording();
                }

                else{
                    _isPlaying = false;
                    otherPoint._isPlaying = false;
                    PlayerActionStore.Instance.StopClones();
                }

                return;
            }

            else{
                if(isStartPoint && !_isRecording){
                    _isRecording = true;
                    otherPoint._isRecording = true;
                    PlayerActionStore.Instance.StartRecording();
                }

                if(!isStartPoint && _isRecording){
                    _isRecording = false;
                    otherPoint._isRecording = false;
                    PlayerActionStore.Instance.StopRecording(otherPoint.transform.position);

                    _isPlaying = true;
                    otherPoint._isPlaying = true;
                }
            }
        }
    }

    // Animation
    private int GetState(){
        if(_isPlaying) return A_Playing;
        if(_isRecording) return A_Recording;
        return A_Idle;
    }

    public bool Connect(GameObject other)
    {
        if (!other.CompareTag("Checkpoint") || otherPoint || _connected) return false;
        
        var point = other.GetComponent<Checkpoint>();
        
        if(point._connected) return false;
        
        otherPoint = point;
        otherPoint.otherPoint = this;
        otherPoint.isStartPoint = !isStartPoint;

        _connected = true;
        otherPoint._connected = true;
        
        return true;
    }

    public void ChangeValues(IMechanic newMechanic)
    {
        var mech = (Checkpoint)newMechanic;
        isStartPoint = mech.isStartPoint;
    }
}
