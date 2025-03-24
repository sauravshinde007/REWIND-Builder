using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollapsingPlatform : MonoBehaviour, IMechanic {
    
    [SerializeField] private Collider2D platformCollider;
    [Mechanic("Collapse Time", 0.5f, 3f)]       public float collapseTime = 1f;
    [Mechanic("Fall Speed", 20, 50)]            public int fallSpeed = 30;
    [Mechanic("Shake Intensity", 0.01f, 0.1f)]  public float shakeAmount = 0.1f;
    [SerializeField] private Transform shakeGfx;
    
    private bool _collapsing = false;
    
    IEnumerator StartCollapsing()
    {
        // TODO: Play Collapse Animation
        // Temp: Making platform shake
        StartCoroutine(ShakeCoroutine());
        
        yield return new WaitForSeconds(collapseTime);
        
        platformCollider.enabled = false;
        _collapsing = true;
        Destroy(gameObject, 4f);
    }

    private void Update()
    {
        if (!_collapsing) return;
        
        transform.Translate(Vector2.down *(fallSpeed* Time.deltaTime));
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(StartCollapsing());
        }
    }
    
    private IEnumerator ShakeCoroutine()
    {
        var elapsedTime = 0.0f;

        while (elapsedTime < collapseTime)
        {
            var shakeX = Random.Range(-shakeAmount, shakeAmount);
            var shakeY = Random.Range(-shakeAmount, shakeAmount);
            shakeGfx.localPosition = new Vector3(shakeX, shakeY, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        shakeGfx.localPosition = Vector2.zero;  // Reset to original position after shaking
    }

    public bool Connect(GameObject other)
    {
        return false;
    }

    public void ChangeValues(IMechanic newMechanic)
    {
        // var mech = (CollapsingPlatform) newMechanic;
    }
}
