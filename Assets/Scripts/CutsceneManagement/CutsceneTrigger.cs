using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private bool onlyOnce = true;
    [SerializeField] private bool triggerOnCollision = true;
    [SerializeField] private Transform targetLocation;
    [SerializeField] private float stayDuration = 2f;

    public void CutSceneLogic(Transform target=null, float dur=-1) {
        if (!target) { target = targetLocation; }
        if (Mathf.Approximately(dur, -1)) { dur = stayDuration;}

        GameManager.Instance?.TriggerCutScene(target, dur);

        if(onlyOnce) Destroy(this);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        if(triggerOnCollision)CutSceneLogic(targetLocation);
    }
}