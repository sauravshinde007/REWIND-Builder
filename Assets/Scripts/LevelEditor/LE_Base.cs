using UnityEngine;

public class LE_Base : MonoBehaviour {
    // Can Use in inherited GOs
    [SerializeField] protected Camera mainCamera;
    [SerializeField] protected Transform previewGO;

    [SerializeField] private Transform previewDefaultPosition;

    private SpriteRenderer previewRenderer;

    protected void SetupPreviewAnim(){
        previewRenderer = previewGO.GetComponent<SpriteRenderer>();
        previewGO.position = Vector2.Lerp(previewGO.position, previewDefaultPosition.position, 0.3f);
    }

    public Vector3Int GetGridPos(Grid grid){
        var mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var gridPos = grid.WorldToCell(mousePosition);
        gridPos.z = 0;
        return gridPos;
    }

    protected void UpdatePreviewSprite(Sprite sprite, int order) {
        previewRenderer.sprite = sprite;
        previewRenderer.sortingOrder = order+1;
    }

    protected void CancelOnRightClick()
    {
        // Cancel on right click
        if (!Input.GetMouseButtonDown(1)) return;
        previewGO.gameObject.SetActive(false);
        previewGO.position = previewDefaultPosition.position;
        // canPlace = false;
        LE_Manager.Instance.DisablePlaceMode();
    }

    protected bool MouseOnUI(){
        previewGO.gameObject.SetActive(true);
        return false;
    }
}
