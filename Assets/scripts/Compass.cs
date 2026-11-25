using UnityEngine;
public class Compass : MonoBehaviour
{
    public GameObject escapeGatePrefab;
    // Existing scenes serialize this as an absolute world position.
    public Vector3 escapeGateSpawnOffset = new Vector3(0.93f, 6.2f, 5.1f);
    private bool collected;
    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        if (escapeGatePrefab == null) { Debug.LogError("Assign the escape gate prefab.", this); return; }
        collected = true;
        Instantiate(escapeGatePrefab, escapeGateSpawnOffset, Quaternion.identity);
        foreach (HealthBar bar in FindObjectsOfType<HealthBar>()) bar.UnlockEscape();
        Destroy(gameObject);
    }
}
