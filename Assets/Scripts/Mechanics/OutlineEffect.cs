using UnityEngine;

public class OutlineEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private GameObject _outlineObject;
    
    public void CreateOutline()
    {
        if (_outlineObject != null) return; // Prevent multiple outlines

        _outlineObject = new GameObject("Outline")
        {
            transform =
            {
                parent = spriteRenderer.transform,
                localPosition = Vector3.zero,
                localScale = Vector3.one * 1.1f // Slightly bigger
            }
        };

        var originalRenderer = spriteRenderer;
        var outlineRenderer = _outlineObject.AddComponent<SpriteRenderer>();

        outlineRenderer.sprite = originalRenderer.sprite;
        outlineRenderer.color = Color.black; // Outline color
        outlineRenderer.sortingOrder = originalRenderer.sortingOrder - 1; // Behind original

    }

    public void RemoveOutline()
    {
        if (_outlineObject != null)
        {
            Destroy(_outlineObject);
            _outlineObject = null; // Reset reference
        }
    }
}
