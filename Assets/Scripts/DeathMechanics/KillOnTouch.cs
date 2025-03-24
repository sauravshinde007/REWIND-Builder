using UnityEngine;

[RequireComponent((typeof(Collider2D)))]
public class KillOnTouch : MonoBehaviour, IMechanic
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance?.ResetLevel();
    }

    public bool Connect(GameObject other)
    {
        return false;
    }

    public void ChangeValues(IMechanic newMechanic)
    {
        // var mech = (KillOnTouch)newMechanic;
    }
}
