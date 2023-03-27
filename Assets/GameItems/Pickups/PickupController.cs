using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickupObject
{
    void Pickup();
}

public class PickupController : MonoBehaviour, IPickupObject
{   
    Vector3 burstVector;

    Collider2D collider;


    float createdTime;

    public float burstDuration = 0.3f;
    public float SplashRadius = 0.8f;

    public float lifespanSeconds = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider2D>();
        collider.enabled = false;

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
        else if(!collider.enabled)
        {
            collider.enabled = true;
        }

        float age = Time.timeSinceLevelLoad - createdTime;
        if(age > lifespanSeconds)
        {
            Destroy(gameObject);
        }
    }

    public void Pickup()
    {
        Destroy(gameObject);
    }
}
