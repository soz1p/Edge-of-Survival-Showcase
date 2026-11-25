using UnityEngine;
using UnityEngine.AI;
public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;
    private float timer;
    void Start() { agent = GetComponent<NavMeshAgent>(); timer = wanderTimer; }
    void Update()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        timer += Time.deltaTime;
        if (timer < wanderTimer) return;
        timer = 0f;
        NavMeshHit hit;
        Vector3 candidate = transform.position + Random.insideUnitSphere * wanderRadius;
        if (NavMesh.SamplePosition(candidate, out hit, wanderRadius, agent.areaMask)) agent.SetDestination(hit.position);
    }
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(origin + Random.insideUnitSphere * dist, out hit, dist, layermask)
            ? hit.position : origin;
    }
}
