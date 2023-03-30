using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    private Animator animator;

    public float maxHealth = 100;    
    public float startingHealth = 100;
    private float health;

    public float Health {
        get
        {
            return health;
        }
        set 
        { 
            health = value;

            if(health <= 0.0f)
                Dead();
        }
    }

    public float coins = 0;

    public float mana;

    void Start()
    {
        TryGetComponent<Animator>(out animator);

        health = startingHealth;
    }

    public void Dead()
    {
        if(animator != null)
        {
            animator.SetTrigger("Defeated");
        }
        else
        {
            RemoveSelf();
        }
    }

    public void RemoveSelf()
    {
        Destroy(gameObject);
    }

    public void ProcessPickup(string pickupName)
    {
        if(pickupName == "Coin")
        {
            coins += 1;
        }
        else if(pickupName == "Heart")
        {
            health = Math.Min(maxHealth, health + 1);
        }
        else if(pickupName == "Jewel")
        {
            mana += 1;
        }
    }
}