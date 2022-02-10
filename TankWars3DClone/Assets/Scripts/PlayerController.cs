using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("General Tank Settings")]
    //The speed at which the tank should move.
    [SerializeField]
    private float TankSpeed = 5f;

    //This is the tanks health, starts out as 3!
    [SerializeField] private int TankHealth;

    [SerializeField] private GameObject tankExplosionEffect;

    //The speed at which the turret of the tank rotates at.
    [SerializeField] private float TankTurretRotationSpeed = 5f;

    [SerializeField] private KeyCode turnTurretLeft;
    [SerializeField] private KeyCode turnTurretRight;

    [SerializeField] private Rigidbody tanksRigidBody;

    [Header("Tank Raycast Settings")] 
    [SerializeField] private float rayCastMaxDistance = 1.6f;

    [SerializeField] private float raycastOffset = 1f;

    //The reference to the tanks turret object in order to apply rotation to it.

    [Header("Tank Shooting Settings")] [SerializeField]
    //Reference to what key will enable the user to shoot with the tank!
    private KeyCode KeyToShoot;

    [SerializeField] private GameObject turretObject;


    //RateOfFire - this needs to still be implemented.
    [SerializeField] private float rateOfFire;

    //The reference to the tankmissle object for the projectile
    [SerializeField] private GameObject TankMissle;
    [SerializeField] private Transform MissleSpawnPoint;


    [Header("Debug Settings")] [SerializeField]
    private bool VisualizeMovementRays = false;

    //Array for storing the check, for if a wall is to the left, top, right, bottom - in that order.
    private readonly bool[] WallCheck =
    {
        false, false, false, false
    };


    // Start is called before the first frame update
    private void Start()
    {
    }

    public int getPlayerTankHealth()
    {
        return TankHealth;
    }

    private void Update()
    {
        if (TankHealth <= 0)
        {
            Destroy(this.gameObject);
            GameObject explodeTankEffect = Instantiate(tankExplosionEffect, this.gameObject.transform.position,
                Quaternion.identity);
            explodeTankEffect.GetComponent<ParticleSystem>().Play();
            Destroy(explodeTankEffect, 3f);
        }

        CheckForWalls();
        RotateTurret();
        ShootTankTurret();
    }

    private void FixedUpdate()
    {
        MoveTank();
    }

    private void MoveTank()
    {
        //The x axis of the movement between 0 and 1.
        float xAxis = Input.GetAxis("Horizontal");
        //the y axis of the movement but in this case its the z between 0 and 1.
        float zAxis = Input.GetAxis("Vertical");

        Vector3 newXAxisVector;
        Vector3 newZAxisVector;

        tanksRigidBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                                     RigidbodyConstraints.FreezePositionX;
            //movement right
            if (xAxis > 0 && !WallCheck[2])
            {
                tanksRigidBody.constraints = RigidbodyConstraints.None;
                tanksRigidBody.constraints =
                    RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                newXAxisVector = new Vector3(xAxis * TankSpeed * Time.deltaTime, 0, 0);
                tanksRigidBody.position += newXAxisVector;

                transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            //movement left
            else if (xAxis < 0 && !WallCheck[0])
            {
                tanksRigidBody.constraints = RigidbodyConstraints.None;
                tanksRigidBody.constraints =
                    RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                newXAxisVector = new Vector3(xAxis * TankSpeed * Time.deltaTime, 0, 0);
                tanksRigidBody.position += newXAxisVector;

                transform.rotation = Quaternion.Euler(0, -90, 0);
            }
            //movement up
            else if (zAxis > 0 && !WallCheck[1])
            {
                tanksRigidBody.constraints = RigidbodyConstraints.None;
                tanksRigidBody.constraints =
                    RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX;
                newZAxisVector = new Vector3(0, 0, zAxis * TankSpeed * Time.deltaTime);
                tanksRigidBody.position += newZAxisVector;

                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            //movement down
            else if (zAxis < 0 && !WallCheck[3])
            {
                tanksRigidBody.constraints = RigidbodyConstraints.None;
                tanksRigidBody.constraints =
                    RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX;
                newZAxisVector = new Vector3(0, 0, zAxis * TankSpeed * Time.deltaTime);
                tanksRigidBody.position += newZAxisVector;

                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

    //this method needs to be corrected still.
    private void RotateTurret()
    {
        var xRot = Input.GetAxis("Horizontal");
        var zRot = Input.GetAxis("Vertical");

        if (xRot > 0 || xRot < 0)
            turretObject.transform.Rotate(Vector3.up, xRot * TankTurretRotationSpeed * Time.deltaTime, Space.World);
        else if (zRot > 0 || zRot < 0)
            turretObject.transform.Rotate(Vector3.up, zRot * TankTurretRotationSpeed * Time.deltaTime, Space.World);


        //I left in both turning ways, incase you want to turn the turret based on the x and y axis controls.
        //or if you want to turn using the keys you have set.
        if (Input.GetKey(turnTurretRight))
        {
            turretObject.transform.Rotate(Vector3.up, TankTurretRotationSpeed * Time.deltaTime, Space.World);
        }
        else if (Input.GetKey(turnTurretLeft))
        {
            turretObject.transform.Rotate(Vector3.up, -TankTurretRotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void CheckForWalls()
    {
        //write the comment explaining this and how it works. will be done after.


        Vector3 playerPosition = this.gameObject.transform.position;

        //the wall check should be able to be simplified by checking only 1 array and making sure that none of the values becomes true in the array.
        WallCheck[0] =
            Physics.Raycast(playerPosition,
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            |     Physics.Raycast(playerPosition + Vector3.back * raycastOffset,
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(playerPosition + Vector3.forward * raycastOffset,
                Vector3.left , rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.back * (raycastOffset * 0.5f),
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            |  Physics.Raycast(playerPosition + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.left , rayCastMaxDistance,
                1 << 6);


        //check for a wall to the top
        WallCheck[1] =
            Physics.Raycast(playerPosition,
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            |     Physics.Raycast(playerPosition + Vector3.left * raycastOffset,
                Vector3.forward , rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(playerPosition + Vector3.right * raycastOffset,
                Vector3.forward , rayCastMaxDistance,
                1 << 6)
            |  Physics.Raycast(playerPosition + Vector3.left * (raycastOffset * 0.5f),
                Vector3.forward , rayCastMaxDistance,
                1 << 6)
            |  Physics.Raycast(playerPosition + Vector3.right * (raycastOffset * 0.5f),
                Vector3.forward , rayCastMaxDistance,
                1 << 6);


        //check for a wall to the right
        WallCheck[2] =
            Physics.Raycast(playerPosition,
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            |     Physics.Raycast(playerPosition + Vector3.back * raycastOffset,
                Vector3.right , rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(playerPosition + Vector3.forward * raycastOffset,
                Vector3.right , rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.back * (raycastOffset * 0.5f),
                Vector3.right , rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.right , rayCastMaxDistance,
                1 << 6);


        //Check for a wall to the bottom
        WallCheck[3] =
            Physics.Raycast(playerPosition,
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            |     Physics.Raycast(playerPosition + Vector3.left * raycastOffset,
                Vector3.back , rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(playerPosition + Vector3.right * raycastOffset,
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            |    Physics.Raycast(playerPosition + Vector3.right * (raycastOffset * 0.5f),
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            |  Physics.Raycast(playerPosition + Vector3.left * (raycastOffset * 0.5f),
                Vector3.back , rayCastMaxDistance,
                1 << 6);

        if (VisualizeMovementRays)
        {
            Debug.DrawRay(
                playerPosition,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                playerPosition *1f + Vector3.forward * raycastOffset,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                playerPosition *1f + Vector3.back * raycastOffset,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                playerPosition *1f + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                playerPosition *1f + Vector3.back * (raycastOffset * 0.5f),
                Vector3.left * rayCastMaxDistance, Color.black);
            
            
            Debug.DrawRay(
                playerPosition,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                playerPosition *1f + Vector3.left * raycastOffset,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                playerPosition *1f + Vector3.right * raycastOffset,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                playerPosition *1f + Vector3.left * (raycastOffset * 0.5f),
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                playerPosition *1f + Vector3.right * (raycastOffset * 0.5f),
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            
            
            
            Debug.DrawRay(
                playerPosition,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                playerPosition *1f + Vector3.forward * raycastOffset,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                playerPosition *1f + Vector3.back * raycastOffset,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                playerPosition *1f + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                playerPosition *1f + Vector3.back * (raycastOffset * 0.5f),
                Vector3.right * rayCastMaxDistance, Color.cyan);
            
            
            
            Debug.DrawRay(
                playerPosition,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                playerPosition *1f + Vector3.left * raycastOffset,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                playerPosition *1f + Vector3.right * raycastOffset,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                playerPosition *1f + Vector3.right * (raycastOffset * 0.5f),
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                playerPosition *1f + Vector3.left * (raycastOffset * 0.5f),
                Vector3.back * rayCastMaxDistance, Color.green);
            
            
            
        }
    }

    public void DoDamage()
    {
        TankHealth -= 1;
    }

    private void ShootTankTurret()
    {
        if (Input.GetKeyDown(KeyToShoot))
        {
            GameObject missleObject = Instantiate(TankMissle,
                MissleSpawnPoint.position + this.GetComponent<Rigidbody>().velocity, MissleSpawnPoint.rotation);
            missleObject.transform.Rotate(90, 0, 0);
            missleObject.GetComponent<Rigidbody>().velocity += MissleSpawnPoint.transform.forward * 20;
            missleObject.GetComponent<Rigidbody>().freezeRotation = true;
        }
    }
}