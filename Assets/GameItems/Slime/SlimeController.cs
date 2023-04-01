
using UnityEngine;

public class SlimeController : CharacterStats
{
    private Animator animator;

    public Transform attackCentrePoint;

    public float attackRadius = 1.0f;

    public float attackDamage = 1.0f;

    protected override void Start()
    {
        base.Start();

        TryGetComponent<Animator>(out animator);
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
        var drops = GetComponent<DropController>();
        if(drops)
            drops.Drop();

        Destroy(gameObject);
    }

    protected override void ProcessHealthChange(float startingHealth, float health)
    {
        if(health <= 0)
            Dead();
        else if(health < startingHealth && animator)
            animator.SetTrigger("Ouch");
    }

    void Update()
    {
        Vector3 attackCentre = attackCentrePoint.position;

        foreach(var obj in Physics2D.OverlapCircleAll(attackCentre, attackRadius))
        {
            CharacterStats stats;
            if(obj.tag == "Player" && obj.TryGetComponent<CharacterStats>(out stats))
            {
                // Since this is a continuous "poison" attack, scale it with time
                stats.Health -= attackDamage * Time.deltaTime;
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        if(attackCentrePoint != null)
            Gizmos.DrawWireSphere(attackCentrePoint.position, attackRadius);
    }
}
