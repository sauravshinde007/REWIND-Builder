using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    #region Singleton
    public static GameManager Instance;

    private void Awake(){
    // If an instance already exists and it's not this, destroy this instance.
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    [SerializeField] private bool spawnPlayer = true;
    
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private CinemachineVirtualCamera playerFollowCam;

    private Transform _player;
    
    [Header("Death Mechanics Management")]
    [SerializeField] private float deathJumpForce = 30f;
    [SerializeField] private float waitAfterDeath = 0.8f;
    
    [Header("Cutscene Management")] 
    [SerializeField] private float cameraSpeed = 30;
    private bool _cutScenePlaying = false;
    private Queue<Transform> _cutsceneQueue = new Queue<Transform>();


    // Start is called before the first frame update
    private void Start() {
        if (Globals.Instance.inLevelEditor) return;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        playerFollowCam.Follow = playerFollowCam.LookAt = _player;
    }

    public void TriggerCutScene(Transform focusLocation, float stayDuration)
    {
        StartCoroutine(CutSceneStarter(focusLocation, stayDuration));
    }
    
    private IEnumerator CutSceneStarter(Transform focusLocation, float stayDuration) {
        if (_cutScenePlaying) {
            _cutsceneQueue.Enqueue(focusLocation);
            yield return null;
        }
    
        _cutScenePlaying = true;
            
        // Show the location
        playerFollowCam.Follow = playerFollowCam.LookAt = focusLocation;
        Globals.Instance.playerEnabled = true;
        
        yield return new WaitForSeconds(stayDuration);

        // Crude solution for Level Editor
        if (!_player) _player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Show Player Again
        playerFollowCam.Follow = playerFollowCam.LookAt = _player;
        Globals.Instance.playerEnabled = false;
        
        // Set current playing cutscene to false (Done)
        _cutScenePlaying = false;
        playerFollowCam.Follow = _player;
        playerFollowCam.LookAt = _player;
    }


    public void ResetLevel()
    {
        StartCoroutine(BeforeDeath());
    }

    public void SetPlayerTransform()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        if (!_player)
        {
            Debug.LogError("No player found, please report this error");
        }
    }

    public void SetPlayerSpawnPoint(Transform _playerSpawnPoint)
    {
        playerSpawnPoint = _playerSpawnPoint;
    }

    void ResetPlayerPosition()
    {
        _player.position = playerSpawnPoint.position;
    }

    private IEnumerator BeforeDeath()
    {
        // Disable Player Inputs
        Globals.Instance.playerInputEnabled = false;
        
        // Simple Death Jump
        _player.GetComponent<Collider2D>().enabled = false;
        var playerRb = _player.GetComponent<Rigidbody2D>();
        playerRb.velocity = Vector2.zero;
        playerRb.AddForce(Vector2.up * deathJumpForce, ForceMode2D.Impulse);
        
        // Disable Camera Movement
        playerFollowCam.enabled = false;
        
        // Wait till done
        yield return new WaitForSeconds(0.8f);
        
        // Re-Enable
        _player.GetComponent<Collider2D>().enabled = true;
        playerFollowCam.enabled = true;
        playerRb.velocity = Vector2.zero;
        
        // Enable Player Inputs
        Globals.Instance.playerInputEnabled = true;
        
        // Just resetting the whole level (or Pause Game if level editor)
        if (Globals.Instance.inLevelEditor)
        {
            LE_Manager.Instance.PauseGame();
            ResetPlayerPosition();
        }
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
