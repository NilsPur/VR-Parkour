using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CoinCollector : MonoBehaviour
{
    public LocomotionTechnique locomotion;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning("COLLISION");

        if(collision.gameObject.CompareTag("coin"))
        {
            locomotion.parkourCounter.coinCount++;
            collision.gameObject.SetActive(false);
        }
    }
}
