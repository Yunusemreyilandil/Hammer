using System;
using UnityEngine;

public class CollisionContactParticleSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystemPrefab; 
    [SerializeField] private float minimumVelocity = 0.1f;        

    private void OnCollisionEnter(Collision other)
    {
        if (other.relativeVelocity.magnitude > minimumVelocity)
        {
            var particleSystem = Instantiate(particleSystemPrefab, other.contacts[0].point, Quaternion.identity);

            
            particleSystem.transform.SetParent(other.transform);

            particleSystem.Play();

           
            Destroy(particleSystem.gameObject, 2f);
        }
    }
}
