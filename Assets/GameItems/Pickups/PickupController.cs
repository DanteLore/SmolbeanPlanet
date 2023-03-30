using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickupObject
{
    void PickedUp();
    string PickupName { get; }
}

public class PickupController : MonoBehaviour, IPickupObject
{   
    Vector3 burstVector;
    Collider2D pickupCollider;
    float createdTime;
    public float burstDuration = 0.3f;
    public float SplashRadius = 0.8f;
    public float lifespanSeconds = 5.0f;
    public string pickupName = "Unknown";
    public string PickupName { get { return pickupName; } }

    // Start is called before the first frame update
    void Start()
    {
        pickupCollider = GetComponent<Collider2D>();
        pickupCollider.enabled = false;

        float r = SplashRadius;
        burstVector = new Vector3(Random.Range(-r, r), 0.0f, 0.0f);
        burstVector = new Vector3(burstVector.x, Random.Range(-r, -r / 5), 0.0f);

        createdTime = Time.timeSinceLevelLoad;
        lifespanSeconds += Random.Range(-lifespanSeconds/10, lifespanSeconds/10);
    }

    // Update is called once per frame
    void Update()
    {
        if(burstDuration > 0.0f)
        {   
            burstDuration -= Time.deltaTime;
            transform.position += burstVector * Time.deltaTime;
        }
        else if(!pickupCollider.enabled)
        {
            pickupCollider.enabled = true;
        }

        float age = Time.timeSinceLevelLoad - createdTime;
        if(age > lifespanSeconds)
        {
            Destroy(gameObject);
        }
    }

    public void PickedUp()
    {
        Destroy(gameObject);
    }
}
