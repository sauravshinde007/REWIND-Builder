using UnityEngine;

[System.Serializable]
public class MechanicObject {
    [Header("UI Preview")]
    public Sprite uiSprite;

    [Header("Settings")]
    public Vector2Int dimensions = Vector2Int.one;
}
