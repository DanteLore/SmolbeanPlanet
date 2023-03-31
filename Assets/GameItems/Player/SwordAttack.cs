using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    Vector2 rightAttackOffset;
    public Collider2D swordCollider;

    public float damage = 0.25f;

    void Start()
    {
        rightAttackOffset = transform.localPosition;
        swordCollider.enabled = false;
    }

    public void AttackRight()
    {
        transform.localPosition = rightAttackOffset;
        swordCollider.enabled = true;
    }

    public void AttackLeft()
    {
        swordCollider.enabled = true;
        transform.localPosition = new Vector2(-rightAttackOffset.x, rightAttackOffset.y);
    }

    public void StopAttack()
    {
        print("Attack stopped");
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("Trigger Enter");
        if(other.tag == "Enemy")
        {
            // Deal some damage!
            CharacterStats enemyStats = other.GetComponent<CharacterStats>();

            if(enemyStats == null)
                return;

            enemyStats.Health -= damage;
            print(enemyStats.Health);
        }
    }
}
