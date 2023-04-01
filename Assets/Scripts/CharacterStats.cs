using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
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
            float startingHealth = health;
            health = value;

            ProcessHealthChange(startingHealth, health);
        }
    }

    public float coins = 0;

    public float mana;

    virtual protected void Start()
    {
        health = startingHealth;
    }

    protected virtual void ProcessHealthChange(float startingHealth, float health)
    {
    }

    public void ProcessPickup(string pickupName)
    {
        if(pickupName == "Coin")
        {
            coins += 1;
        }
        else if(pickupName == "Heart")
        {
            health = Math.Min(maxHealth, health + 5);
        }
        else if(pickupName == "Jewel")
        {
            mana += 1;
        }
    }
}
