using System.Collections.Generic;
using UnityEngine;
public class cshAttackArea : MonoBehaviour
{
    private readonly List<Collider> colliderList = new List<Collider>();
    public List<Collider> colliders
    {
        get { colliderList.RemoveAll(c => c == null); return colliderList; }
    }
    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("BreakableObject") || other.CompareTag("Enemy")) && !colliders.Contains(other))
            colliderList.Add(other);
    }
    void OnTriggerExit(Collider other) { colliderList.Remove(other); }
}
