using UnityEngine;
public class cshBreakableObject : MonoBehaviour
{
    public GameObject destroyEffectPrefab;
    public void PlayEffect()
    {
        if (destroyEffectPrefab != null) Instantiate(destroyEffectPrefab, transform.position, transform.rotation);
    }
}
