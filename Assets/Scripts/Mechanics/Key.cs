using UnityEngine;

public class Key : MonoBehaviour, IMechanic
{

    [SerializeField] private LineRenderer connectionWirePrefab;

    [Mechanic("Key card Color", 0)] public int keyColorIndex;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GlobalInventory.Instance.AddKey(keyColorIndex);
        gameObject.SetActive(false);
    }

    public bool Connect(GameObject other)
    {
        return false;
    }

    public void ChangeValues(IMechanic newMechanic)
    {
        // var mech = (Key) newMechanic;
    }
}
