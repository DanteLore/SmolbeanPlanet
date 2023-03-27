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
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("w00t");
        if(other.tag == "Enemy")
        {
            // Deal some damage!
            Enemy enemy = other.GetComponent<Enemy>();

            if(enemy == null)
                return;

            enemy.Health -= damage;
            print(enemy.Health);
        }
    }
}
