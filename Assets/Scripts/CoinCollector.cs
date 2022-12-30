using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CoinCollector : MonoBehaviour
{
    public LocomotionTechnique locomotion;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("coin"))
        {
            locomotion.parkourCounter.coinCount++;
            collider.gameObject.SetActive(false);
        }
    }
}
