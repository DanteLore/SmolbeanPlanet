using UnityEngine;

public class PlayerAnimationProxy : MonoBehaviour
{
    public void SwordAttack()
    {
        transform.parent.GetComponent<PlayerController>().SwordAttack();
    }

    public void EndSwordAttack()
    {
        transform.parent.GetComponent<PlayerController>().EndSwordAttack();
    }
}
