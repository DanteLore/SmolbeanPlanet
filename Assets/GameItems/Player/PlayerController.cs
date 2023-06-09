using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CharacterStats
{
    public float moveSpeed = 0.5f;
    public ContactFilter2D movementFilter;
    public float collisionOffset = 0.05f;
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public float attackDamage = 5.0f;

    bool canMove = true;
    bool canAttack = true;
    Vector2 movementInput;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sprintRenderer;
    PlayerInput playerInput;

    IInteractableObject interadtableObject;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        sprintRenderer = GetComponentInChildren<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
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
        if(direction == Vector2.zero || !canMove)
            return false;

        List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
        int count =  rb.Cast(direction, movementFilter, castCollisions, moveSpeed * Time.fixedDeltaTime + collisionOffset);
        if(count == 0)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            return true;
        }
        return false;
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    void OnFire()
    {
        if(canAttack)
        {
            canAttack = false;
            animator.SetTrigger("SwordAttack");
        }
    }

    void OnInteract()
    {
        if(interadtableObject != null)
        {
            interadtableObject.Interact();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.gameObject.GetComponent<IInteractableObject>();
        if(interactable != null)
        {
            interadtableObject = interactable;
            interactable.EnteredRange();
        }

        var pickup = other.gameObject.GetComponent<IPickupObject>();
        if(pickup != null)
        {
            ProcessPickup(pickup.PickupName);
            pickup.PickedUp();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.gameObject.GetComponent<IInteractableObject>();
        if(interactable != null)
        {
            interactable.ExitedRange();
            interadtableObject = null;
        }
    }

    public void SwordAttack()
    {
        Vector3 attackCentre = (sprintRenderer.flipX) ? transform.TransformPoint(new Vector3(-1 * attackPoint.localPosition.x, attackPoint.localPosition.y, 0)) : attackPoint.position;

        foreach(var obj in Physics2D.OverlapCircleAll(attackCentre, attackRadius))
        {
            CharacterStats stats;
            if(obj.tag == "Enemy" && obj.TryGetComponent<CharacterStats>(out stats))
            {
                stats.Health -= attackDamage;
            }
        }
    }

    public void EndSwordAttack()
    {
        canAttack = true;
    }

    protected override void ProcessHealthChange(float startingHealth, float health)
    {
        if(health <= 0)
            Dead();
        else if(health < startingHealth && animator)
            animator.SetTrigger("Ouch");
    }

    public void Dead()
    {
        if(animator)
            animator.SetTrigger("Defeated");
        else
            RemoveSelf();
    }

    public void RemoveSelf()
    {
        // Game over!!  Return to main menu, respawn etc.
        canMove = false;
        playerInput.enabled = false;
        animator.enabled = false;
    }

    public void OnDrawGizmosSelected()
    {
        if(attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
