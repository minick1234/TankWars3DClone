using System;
using UnityEngine;

public class MissleLogic : MonoBehaviour
{
    private Vector3 oldVelocity;
    [SerializeField] private GameObject missleExplosion;


    private RaycastHit hit;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        //Destroy this missle object if nothing happens after 3 seconds.
        Destroy(this.gameObject, 3f);
    }

    private void FixedUpdate()
    {
        //store the velocity of the old velocity before the reflection.
        oldVelocity = GetComponent<Rigidbody>().velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8 || collision.gameObject.layer == 10)
        {
            GameObject newExplosion = Instantiate(missleExplosion, this.transform.position, Quaternion.identity);
            newExplosion.GetComponent<ParticleSystem>().Play();
            Destroy(newExplosion, 3f);
            Destroy(gameObject);

            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<PlayerController>().DoDamage();
            }else if (collision.gameObject.CompareTag("Enemy"))
            {
                collision.gameObject.GetComponent<EnemyTankController>().DoDamage();
            }
        }
        else
        {
            //Reflect the bullet based on the original value of the velocity the bullet was moving, and the first collision contact point.
            //the normal is the vector at which angle technically we came from.
            Vector3 newReflectedVelocity = Vector3.Reflect(oldVelocity, collision.contacts[0].normal);
            //reassign the velocity to the new reflected velocity value to travel in that direction.
            this.GetComponent<Rigidbody>().velocity = newReflectedVelocity;

            //Assign the rotation to also change from the old velocity direction to the new velocity direction
            Quaternion newReflectedRotation = Quaternion.FromToRotation(oldVelocity, newReflectedVelocity);

            //With the new calculated reflectedrotation we do some weird quaternion math and multiply the old value to the new reflected value back to the rotation of this missle object.
            //so that it rotates in the correct direction
            this.transform.rotation = newReflectedRotation * this.transform.rotation;
        }
    }
}