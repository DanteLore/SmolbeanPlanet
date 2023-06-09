using UnityEngine;

public class PickupGravity : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float gravityRadius = 1.0f;
    private int pickupLayerMask;

    void Start()
    {
        pickupLayerMask = LayerMask.GetMask("Pickups");
    }

    void Update()
    {
        foreach(var obj in Physics2D.OverlapCircleAll(transform.position, gravityRadius, pickupLayerMask))
        {
            if(obj.tag == "Pickup")
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, transform.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, gravityRadius);
    }
}
