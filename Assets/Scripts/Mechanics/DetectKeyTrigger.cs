using UnityEngine;

public class DetectKeyTrigger : MonoBehaviour
{
    [SerializeField] private Door door;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (door == null) return;

        if (other.CompareTag("Player"))
        {
            if (GlobalInventory.Instance.CanOpenDoor(door.doorColorIndex))
            {
                Destroy(gameObject);
                door.AddKey();
            }
        }
    }
}
