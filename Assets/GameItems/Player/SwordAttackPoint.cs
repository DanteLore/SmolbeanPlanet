using UnityEngine;

public class SwordAttackPoint : MonoBehaviour
{
    void OnDrawGizmosSelected()
 {
     this.transform.parent.SendMessage("OnDrawGizmosSelected");
 }
}
