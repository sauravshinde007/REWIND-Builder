using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LE_TileManager : LE_Base
{

    [SerializeField] private Grid tileLayersParent;
    [SerializeField] private GameObject tilemapPrefab;
    public List<Tilemap> tileLayers { get; private set; }

    [SerializeField] public TileBase[] tileToPlace;

    [SerializeField] private GameObject viewContentUI;
    [SerializeField] private GameObject viewButtonPrefab;

    [SerializeField] private Color _disabledLayerColor;

    private Vector3Int previousCellPosition;
    private bool _dragging = false;

    private int _currentLayer = 0;

    [HideInInspector] public int selectedTile;
    [HideInInspector] public bool canPlace = false;

    public static LE_TileManager Instance {get; private set;}

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        tileLayers = new List<Tilemap>();
    }

    void Start()
    {
        SetupPreviewAnim();
        SetupContentUI();
    }

    void SetupContentUI()
    {
        for (int i = 0; i < tileToPlace.Length; i++)
        {
            var obj = Instantiate(viewButtonPrefab, viewContentUI.transform).GetComponent<ItemHolder>();

            var button = obj.button;
            var img = obj.image;

            var t = i;
            button.onClick.AddListener(() => OnButtonClick(t));

            img.sprite = (tileToPlace[i] as LevelTile)?.m_DefaultSprite;
        }
    }

    public void OnButtonClick(int index)
    {
        selectedTile = index;
        previewGO.gameObject.SetActive(false);
        // canPlace = true;
        LE_Manager.Instance.EnableTilePlaceMode();
    }

    public void AddNewLayer(int layer)
    {
        _currentLayer = layer;
        var map = Instantiate(tilemapPrefab, tileLayersParent.transform);
        map.name = _currentLayer.ToString();
        var tilemapRenderer = map.GetComponent<TilemapRenderer>();
        tilemapRenderer.sortingOrder = _currentLayer;
        tileLayers.Add(map.GetComponent<Tilemap>());
    }

    public void SetCurrentLayer(int layer, bool collidable)
    {
        _currentLayer = layer;
        SetLayerCollisionState(layer, collidable);
        for (var i = 0; i < tileLayers.Count; i++)
        {
            tileLayers[i].color = i == _currentLayer ? Color.white : _disabledLayerColor;
        }
    }

    public void SetLayerCollisionState(int layer, bool collidable)
    {
        tileLayers[layer].GetComponent<TilemapCollider2D>().enabled = collidable;
    }

    public void EnableAllLayers()
    {
        foreach (var t in tileLayers)
        {
            t.color = Color.white;
        }
    }
    
    void Update()
    {
        if (MouseOnUI()) return;
        CancelOnRightClick();
        if (!canPlace) return;

        // Press F to Flood fill
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (selectedTile < 0 || selectedTile >= tileToPlace.Length) return;
            var startPosition = GetGridPos(tileLayers[_currentLayer].layoutGrid);
            tileLayers[_currentLayer].FloodFill(startPosition, tileToPlace[selectedTile] as LevelTile);
        }

        if (Input.GetMouseButtonDown(0)) _dragging = true;
        if (Input.GetMouseButtonUp(0)) _dragging = false;
        
        if(_dragging) Drag();
        
        // Update Preview
        UpdateTilePreview();

    }

    Vector3Int PlaceTile(TileBase obj)
    {
        var gridPos = GetGridPos(tileLayers[_currentLayer].layoutGrid);
        tileLayers[_currentLayer].SetTile(gridPos, obj as LevelTile);
        return gridPos;
    }

    Vector3Int RemoveTile()
    {
        var gridPos = GetGridPos(tileLayers[_currentLayer].layoutGrid);
        tileLayers[_currentLayer].SetTile(gridPos, null);
        return gridPos;
    }

    void UpdateTilePreview()
    {
        if (_currentLayer >= tileLayers.Count) return;

        var gridPosition = GetGridPos(tileLayers[_currentLayer].layoutGrid);

        // Move the preview object to the current grid position
        var cellCenterWorld = tileLayers[_currentLayer].GetCellCenterWorld(gridPosition);
        previewGO.transform.position = Vector2.Lerp(previewGO.transform.position, cellCenterWorld, 0.3f);

        if (selectedTile < 0) UpdatePreviewSprite(null, _currentLayer);
        else UpdatePreviewSprite((tileToPlace[selectedTile] as LevelTile)?.m_DefaultSprite, _currentLayer);
    }

    private void Drag() {
        if (selectedTile <= -1) {
            RemoveTile();
        } else {
            PlaceTile(tileToPlace[selectedTile]);
        }
    }
}
