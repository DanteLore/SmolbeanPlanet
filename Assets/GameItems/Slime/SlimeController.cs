
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : CharacterStats
{
    public Transform attackCentrePoint;
    public float attackRadius = 1.0f;
    public float targetRadius = 3.0f;
    public float attackDamage = 1.0f;
    public float moveSpeed = 1.0f;
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;

    private GameObject player;
    private Animator animator;
    private Vector2 movementInput;
    private Rigidbody2D rigidBody;
    private SpriteRenderer sprintRenderer;
    private Collider2D mainCollider;
    private bool isAlive = true;

    protected override void Start()
    {
        base.Start();

        TryGetComponent<Animator>(out animator);

        player = GameObject.FindWithTag("Player");
        rigidBody = GetComponent<Rigidbody2D>();
        sprintRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<Collider2D>();
    }

    public void Dead()
    {
        animator.SetTrigger("Defeated");
        isAlive = false;
        mainCollider.enabled = false;
    }

    public void RemoveSelf()
    {
        var drops = GetComponent<DropController>();
        if(drops)
            drops.Drop();

        Destroy(gameObject);
    }

    protected override void ProcessHealthChange(float startingHealth, float health)
    {
        if(health <= 0)
            Dead();
        else if(health < startingHealth)
            animator.SetTrigger("Ouch");
    }

    void Update()
    {
        if(!isAlive)
        {
            movementInput = Vector2.zero;
            return;
        }

        var distanceToPlayer = Vector3.Distance(attackCentrePoint.position, player.transform.position);

        if(distanceToPlayer <= attackRadius)
        {
            // Since this is a continuous "poison" attack, scale it with time
            var stats = player.GetComponent<CharacterStats>();
            stats.Health -= attackDamage * Time.deltaTime;
        }

        if(distanceToPlayer <= targetRadius)
        {
            movementInput = (player.transform.position - transform.position).normalized;
        }
        else
        {
            movementInput = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        bool success = false;

        if(movementInput != Vector2.zero)
        {
            success = TryMove(movementInput);

            if(!success)
            {
                success = TryMove(new Vector2(movementInput.x, 0.0f));
            }

            if(!success)
            {
                success = TryMove(new Vector2(0.0f, movementInput.y));
            }
        }
        
        animator.SetBool("IsMoving", success);
        if(movementInput.x < 0)
            sprintRenderer.flipX = true;
        else if(movementInput.x > 0)
            sprintRenderer.flipX = false;
    }

    private bool TryMove(Vector2 direction)
    {
        if(direction == Vector2.zero)
            return false;

        List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
        int count = rigidBody.Cast(direction, movementFilter, castCollisions, moveSpeed * Time.fixedDeltaTime + collisionOffset);
        if(count == 0)
        {
            rigidBody.MovePosition(rigidBody.position + direction * moveSpeed * Time.fixedDeltaTime);
            return true;
        }
        return false;
    }

    public void OnDrawGizmosSelected()
    {
        if(attackCentrePoint != null)
            Gizmos.DrawWireSphere(attackCentrePoint.position, attackRadius);
    }
}
