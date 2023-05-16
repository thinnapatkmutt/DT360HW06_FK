using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public FlockManager myManager;
    float speed;
    public LayerMask obstacle;
    RaycastHit hit;
    

    // Start is called before the first frame update
    void Start()
    {
        speed = Random.Range(myManager.minSpeed, myManager.maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        ApplyFlockingRules();
        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    void ApplyFlockingRules()
    {
        // check our neighbors within neighborDistance
        GameObject[] all = myManager.allFish;

        Vector3 nbCenter = Vector3.zero;     // Rule #1
        float nbSpeed = 0.0f;                // Rule #2
        Vector3 nbAvoid = Vector3.zero;      // Rule #3
        int nbSize = 0;

        foreach (GameObject fish in all)
        {
            if (fish != this.gameObject)
            {
                // calculate the distance & check if it is our neighbor.
                float nDistance = Vector3.Distance(fish.transform.position, this.transform.position);
                if (nDistance <= myManager.neighborDistance)
                {
                    // collect data for each rules in group behavior
                    // Rule#1 : grouping toward the center
                    nbCenter += fish.transform.position;
                    nbSize++;

                    // Rule#2 : moving along the flock
                    nbSpeed += fish.GetComponent<Flock>().speed;

                    // Rule#3 : moving away when too close
                    if (nDistance < 1.0f)
                    {
                        nbAvoid += this.transform.position - fish.transform.position;
                    }
                }
            }
        }

        // if we have neighbors, then we calculate all 3 rules:
        // 1. average of center
        // 2. average of speed [average of heading direction]
        // 3. if too close to any neighbor, move away from the neighbor.
        // Then, calculate the right direction for group behavior.
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1f, obstacle))
        {
            Vector3 odir = Vector3.Reflect(transform.forward, hit.normal);

            Debug.DrawRay(transform.position, transform.forward, Color.red);
            Debug.DrawRay(hit.point, odir, Color.blue);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                   Quaternion.LookRotation(odir), //reflect
                                                   myManager.rotationSpeed).normalized;
        }
        else if (nbSize > 0)
        {
            // RESULT:
            // turn toward the direction of group behaviors: Quaternion.Slerp()
            // computer 'speed' for Translate() afterward.

            // do average
            nbCenter = nbCenter / nbSize;
            nbSpeed = nbSpeed / nbSize;

            // computer target direction
            Vector3 targetDir = (nbCenter + nbAvoid) - this.transform.position;

            // turning toward target direction
            transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                                   Quaternion.LookRotation(targetDir), //reflect
                                                   myManager.rotationSpeed * Time.deltaTime); 
        }
    }
}
