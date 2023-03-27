using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public ContactFilter2D movementFilter;
    public float collisionOffset = 0.05f;

    public SwordAttack swordAttack;

    bool canMove = true;

    Vector2 movementInput;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sprintRenderer;

    IInteractableObject interadtableObject;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprintRenderer = GetComponent<SpriteRenderer>();
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
        animator.SetTrigger("SwordAttack");
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
            pickup.Pickup();
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
        LockMovement();
        if(sprintRenderer.flipX)
        {
            swordAttack.AttackLeft();
        }
        else
        {
            swordAttack.AttackRight();
        }
    }

    public void EndSwordAttack()
    {
        UnlockMovement();
        swordAttack.StopAttack();
    }

    public void LockMovement()
    {
        canMove = false;
    }

    public void UnlockMovement()
    {
        canMove = true;
    }
}
