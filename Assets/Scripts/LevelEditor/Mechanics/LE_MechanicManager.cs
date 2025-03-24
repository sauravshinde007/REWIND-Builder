using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class LE_MechanicManager : LE_Base {
    public Mechanic[] allMechanics;
    [SerializeField] private Transform viewContentUI;
    [SerializeField] private GameObject baseUIPrefab;
    [SerializeField] private Transform mechanicPreviewParent;

    [SerializeField] private Grid gridSystem;
    public Transform mechanicsParent;
    [HideInInspector] public int selectedMechanicIndex;
    [HideInInspector] public bool canPlace = false;

    private Dictionary<Vector2Int, bool> occupiedCells = new();
    private Transform playerSpawnPoint;

    private Mechanic selectedMechanic;

    public static LE_MechanicManager Instance {get; private set;}

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    // Start is called before the first frame update
    void Start() {
        SetupPreviewAnim();
        SetupContentUI();
    }

    void SetupContentUI(){
        for(int i = 0; i < allMechanics.Length; i++){
            var obj = Instantiate(baseUIPrefab, viewContentUI).GetComponent<ItemHolder>();

            var button = obj.button;
            var img = obj.image;

            int t = i;
            button.onClick.AddListener(() => OnButtonClick(t));

            img.sprite = allMechanics[i].basicConfig.uiSprite;
        }
    }

    public void ClearPreviews(){
        foreach(Transform obj in mechanicPreviewParent){
            Destroy(obj.gameObject);
        }
    }

    public void OnButtonClick(int index){
        ClearPreviews();
        selectedMechanicIndex = index;
        if(index >= 0){
            selectedMechanic = Instantiate(allMechanics[selectedMechanicIndex], Vector2.one * 1000, Quaternion.identity, mechanicPreviewParent);
            selectedMechanic.EnablePreview();
            // canPlace = true;
        }
        previewGO.gameObject.SetActive(false);
        LE_Manager.Instance.EnableMechanicPlaceMode();
    }

    public void SetPlayerSpawnPoint(Vector2 gridPos)
    {
        if(playerSpawnPoint){ 
            Destroy(playerSpawnPoint.gameObject);
        }

        var player = Instantiate(allMechanics[0], gridPos, Quaternion.identity, mechanicsParent.parent);
        player.DisablePreview();

        playerSpawnPoint = player.transform;
        GameManager.Instance.SetPlayerSpawnPoint(playerSpawnPoint);
        GameManager.Instance.SetPlayerTransform();
    }

    // Update is called once per frame
    void Update() {

        // Checks ------------------------------------------------------------------
        if(MouseOnUI()) return;
        CancelOnRightClick();

        // Get grid position under mouse
        var gridPos = (Vector2Int)GetGridPos(gridSystem);
        if(selectedMechanic) selectedMechanic.SetPreviewPosition(gridPos);
        if(!canPlace) return;

        // --------------------------------------------------------------------------

        // Place on Click
        if (!Input.GetMouseButtonDown(0)) return;
        switch (selectedMechanicIndex)
        {
            // Player Spawn Point
            case 0:
            {
                // Check if occupied
                try{
                    bool occ = occupiedCells[gridPos];
                    return;
                }
                catch{}

                SetPlayerSpawnPoint(gridPos);
                break;
            }
            case > 0:
            {
                // Check if occupied
                try{
                    bool occ = occupiedCells[gridPos];
                    return;
                }
                catch{}

                var mech = Instantiate(allMechanics[selectedMechanicIndex], (Vector3Int)gridPos, Quaternion.identity, mechanicsParent);
                mech.DisablePreview();
                occupiedCells[gridPos] = true;
                break;
            }
            // Destroy Object under mouse
            // TODO: Highlight all objects that can be deleted
            default:
            {
                // Destroy
                var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                if(hit.collider){
                    Destroy(hit.transform.parent.gameObject);
                }

                break;
            }
        }

        // Update Preview Object
        // previewGO.transform.position = gridPos;
        // UpdatePreviewSprite(allMechanics[selectedMechanicIndex].basicConfig.uiSprite);
    }
}