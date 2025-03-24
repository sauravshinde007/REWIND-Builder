using UnityEngine;

public class DoorTrigger : MonoBehaviour, IMechanic {

    [SerializeField] private Door doorToOpen;
    [SerializeField] private LineRenderer connectionWirePrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!doorToOpen) return;
        doorToOpen.AddKey();
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.blue;
        if (doorToOpen != null) Gizmos.DrawLine(transform.position, doorToOpen.transform.position);
    }

    public bool Connect(GameObject other)
    {
        if (!other.CompareTag("Door") || doorToOpen) return false;
        
        var door = other.GetComponent<Door>();
        doorToOpen = door;
        
        return true;
    }

    public void ChangeValues(IMechanic newMechanic)
    {
        // var mech = (Key) newMechanic;
    }
}
