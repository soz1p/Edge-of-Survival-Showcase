using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class cshSinglePlayer : MonoBehaviour
{
    public float m_moveSpeed = 2f;
    public float m_runSpeed = 4f;
    public float m_jumpForce = 5f;
    public GameObject statueBearPrefab;
    public float spawnRadius = 20f;
    public float detectionRadius = 10f;
    private Animator m_animator;
    private CharacterController controller;
    private cshAttackArea m_attackArea;
    private Vector3 velocity;
    private bool jumpRequested;
    private bool failed;
    private float timer;
    private bool bearSpawned;
    private GameObject statueBearInstance;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        m_attackArea = GetComponentInChildren<cshAttackArea>();
        if (controller == null) { Debug.LogError("Missing CharacterController.", this); enabled = false; }
    }

    void Update()
    {
        
        if (failed || controller == null) return;
        timer += Time.deltaTime;
        if (!bearSpawned && timer >= 5f)
        {
            bearSpawned = true;
            if (statueBearPrefab != null)
            {
                Vector2 offset = Random.insideUnitCircle * spawnRadius;
                Vector3 position = transform.position + new Vector3(offset.x, 0f, offset.y);
                statueBearInstance = Instantiate(statueBearPrefab, position, Quaternion.identity);
            }
        }
        if (statueBearInstance != null &&
            Vector3.Distance(transform.position, statueBearInstance.transform.position) <= detectionRadius)
        {
            failed = true;
            SceneManager.LoadScene("tryagain");
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space)) OnVirtualPadJump();
        MovePlayer();
    }

    void MovePlayer()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
            ? m_runSpeed : m_moveSpeed;
        Vector3 direction = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f,
            Input.GetAxis("Vertical")), 1f);
        if (controller.isGrounded)
        {
            velocity.x = direction.x * speed;
            velocity.z = direction.z * speed;
            velocity.y = jumpRequested ? m_jumpForce : -2f;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction);
        }
        jumpRequested = false;
        velocity.y -= 20f * Time.deltaTime;
        // Horizontal speed must not multiply gravity or jump velocity.
        controller.Move(velocity * Time.deltaTime);
        if (m_animator != null)
        {
            m_animator.SetFloat("Move", direction.magnitude);
            m_animator.SetBool("Jump", !controller.isGrounded);
        }
    }

    public void OnVirtualPadJump()
    {
        
        if (enabled && controller != null && controller.isGrounded) jumpRequested = true;
    }

    public bool CanAttack()
    {
        return enabled && true && m_attackArea != null && m_attackArea.colliders.Count > 0;
    }

    public void OnVirtualPadAttack()
    {
        if (!CanAttack()) return;
        if (m_animator != null) m_animator.SetTrigger("Attack");
        // Damage/destruction can remove colliders: iterate a snapshot, not the live list.
        Collider[] targets = m_attackArea.colliders.ToArray();
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (Collider target in targets)
        {
            if (target == null) continue;
            center += target.transform.position;
            count++;
            cshBreakableObject breakable = target.GetComponent<cshBreakableObject>();
            cshEnemyController enemy = target.GetComponent<cshEnemyController>();
            if (breakable != null) breakable.PlayEffect();
            if (enemy != null) enemy.Damage();
            else if (breakable != null) Destroy(target.gameObject);
        }
        if (count == 0) return;
        center /= count;
        center.y = transform.position.y;
        if ((center - transform.position).sqrMagnitude > 0.001f) transform.LookAt(center);
    }

    void OnDestroy()
    {
        if (statueBearInstance != null) Destroy(statueBearInstance);
    }
}
