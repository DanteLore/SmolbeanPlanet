using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    Vector2 rightAttackOffset;
    public Collider2D swordCollider;

    public float damage = 5.0f;

    void Start()
    {
        swordCollider.enabled = false;
        rightAttackOffset = transform.localPosition;
    }

    public void AttackRight()
    {
        swordCollider.enabled = true;
        transform.localPosition = rightAttackOffset;
    }

    public void AttackLeft()
    {
        swordCollider.enabled = true;
        transform.localPosition = new Vector2(-rightAttackOffset.x, rightAttackOffset.y);
    }

    public void StopAttack()
    {
        //print("Attack stopped");
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //print("Trigger Enter");
        if(other.tag == "Enemy")
        {
            // Deal some damage!
            CharacterStats enemyStats = other.GetComponent<CharacterStats>();

            enemyStats.Health -= damage;
            print(enemyStats.Health);
        }
    }
}
