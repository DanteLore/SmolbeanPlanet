using UnityEngine;

public class PlayerAnimationProxy : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        player = transform.parent.GetComponent<PlayerController>();
    }

    public void SwordAttack()
    {
        player.SwordAttack();
    }

    public void EndSwordAttack()
    {
        player.EndSwordAttack();
    }

    public void RemoveSelf()
    {
        player.RemoveSelf();
    }
}
