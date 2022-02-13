using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyTankController : MonoBehaviour
{
    //These are private variables for the sake of reference in the code. 
    //The xaxis and zaxis is used for the movement and the rotation of the turret, this is done to avoid 2 getaxis calls.

    public float xAxis = 0, zAxis = 0;

    public GameManager gm;
    [SerializeField] private float RecheckMovementTime = 2f;
    private float lastRecheckTime = 0;
    [SerializeField] private float RecheckCollisionTime = 2f;
    private float lastLocationTime;
    private Vector3 lastPosition;

    [Header("General Tank Settings")]
    //The object render that will rotate. This is a seperate group of gameobjects just for the tank to rotate, this is done to avoid turning the turret when the tank rotates, so we maintain the same position and rotation on the turret.
    [SerializeField]
    public GameObject tankRenders;

    //The speed at which the tank should move.
    [SerializeField] private float TankSpeed = 5f;

    [SerializeField] private float BulletSpeed = 30f;

    //This is the tanks health, starts out as 3!
    [SerializeField] private int TankHealth;

    [SerializeField] private GameObject tankExplosionEffect;

    //The speed at which the turret of the tank rotates at.
    [SerializeField] private float TankTurretRotationSpeed = 5f;

    [SerializeField] private Rigidbody tanksRigidBody;

    [Header("Tank Raycast Settings")] [SerializeField]
    private float rayCastMaxDistance = 1.6f;

    [SerializeField] private float raycastOffset = 1f;

    //The reference to the tanks turret object in order to apply rotation to it.

    [Header("Tank Shooting Settings")] [SerializeField]
    public GameObject turretObject;


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
        if (WallCheck[0] && xAxis == -1)
        {
            xAxis = 1;
        }
        else if (WallCheck[1] && zAxis == 1)
        {
            zAxis = -1;
        }
        else if (WallCheck[2] && xAxis == 1)
        {
            xAxis = -1;
        }
        else if (WallCheck[3] && zAxis == -1)
        {
            zAxis = 1;
        }
    }

    private void Update()
    {
        if (TankHealth <= 0)
        {
            gm.KillEnemy(this.gameObject);
            GameObject explodeTankEffect = Instantiate(tankExplosionEffect, this.gameObject.transform.position,
                Quaternion.identity);
            explodeTankEffect.GetComponent<ParticleSystem>().Play();
            Destroy(this.gameObject);
            Destroy(explodeTankEffect, 3f);
        }

        CheckForWalls();
    }

    private void FixedUpdate()
    {
        MoveTank();
    }

    private void MoveTank()
    {
        Vector3 newXAxisVector = new Vector3(xAxis * TankSpeed * Time.deltaTime, 0, 0);
        Vector3 newZAxisVector = new Vector3(0, 0, zAxis * TankSpeed * Time.deltaTime);


        //similar to the movement for the player but the tank here generates its movement values then  applies that to here.
        //it will keep doing the movement checks and only after a few seconds.
        lastRecheckTime += Time.deltaTime;
        if (lastRecheckTime < RecheckMovementTime)
        {
            //LITERALLY DO NOTHING.
        }
        else
        {
            lastRecheckTime = 0;
            GenerateRandomDirection();
        }

        tanksRigidBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                                     RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationY |
                                     RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;


        //movement right
        if (xAxis > 0)
        {
            if (WallCheck[2])
            {
                xAxis = 0;
            }

            tanksRigidBody.constraints = RigidbodyConstraints.None;
            tanksRigidBody.position += newXAxisVector;

            tankRenders.transform.rotation = Quaternion.Euler(0, 90, 0);
            tanksRigidBody.constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        //movement left
        else if (xAxis < 0)
        {
            if (WallCheck[0])
            {
                xAxis = 0;
            }

            tanksRigidBody.constraints = RigidbodyConstraints.None;
            tanksRigidBody.position += newXAxisVector;

            tankRenders.transform.rotation = Quaternion.Euler(0, -90, 0);
            tanksRigidBody.constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        //movement up
        else if (zAxis > 0)
        {
            if (WallCheck[1])
            {
                zAxis = 0;
            }

            tanksRigidBody.constraints = RigidbodyConstraints.None;
            tanksRigidBody.position += newZAxisVector;

            tankRenders.transform.rotation = Quaternion.Euler(0, 0, 0);
            tanksRigidBody.constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        //movement down
        else if (zAxis < 0)
        {
            if (WallCheck[3])
            {
                zAxis = 0;
            }

            tanksRigidBody.constraints = RigidbodyConstraints.None;
            tanksRigidBody.position += newZAxisVector;

            tankRenders.transform.rotation = Quaternion.Euler(0, 180, 0);
            tanksRigidBody.constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void PlayerCheck()
    {
        //this has to shoot a ray from each of the sides of the tank for a long long distance, and checks to make sure the ray collides with the wall.
        //if the ray is not colliding with a wall and hits the player first then we have engaged and seen the player and can make the necessary actions to shoot the player.
    }

    private void GenerateRandomDirection()
    {
        //This takes the specific directions that the walls are not at, and allows the ai to take a certain action based on a percentage, always determining different directions,
        //i need some type of turn cooldown so that it doesnt make the decision 100times a second but instead it wont make a new decision for at least 1 second lets say.

        int wallCheckCount = 0;
        for (int i = 0; i < WallCheck.Length; i++)
        {
            if (!WallCheck[i])
            {
                wallCheckCount++;
            }
        }

        if (wallCheckCount == 3)
        {
            print("doing a 3 way move.");

            int randomDirection = Random.Range(0, 3);

            if (WallCheck[0])
            {
                if (randomDirection == 0)
                {
                    xAxis = 1;
                }
                else if (randomDirection == 1)
                {
                    zAxis = -1;
                }
                else if (randomDirection == 2)
                {
                    zAxis = 1;
                }
            }
            else if (WallCheck[1])
            {
                if (randomDirection == 0)
                {
                    xAxis = 1;
                }
                else if (randomDirection == 1)
                {
                    xAxis = -1;
                }
                else if (randomDirection == 2)
                {
                    zAxis = -1;
                }
            }
            else if (WallCheck[2])
            {
                if (randomDirection == 0)
                {
                    zAxis = 1;
                }
                else if (randomDirection == 1)
                {
                    xAxis = -1;
                }
                else if (randomDirection == 2)
                {
                    zAxis = -1;
                }
            }
            else if (WallCheck[3])
            {
                if (randomDirection == 0)
                {
                    xAxis = 1;
                }
                else if (randomDirection == 1)
                {
                    xAxis = -1;
                }
                else if (randomDirection == 2)
                {
                    zAxis = 1;
                }
            }
        }
        else if (wallCheckCount == 2)
        {
            print("doing a 2 way move.");
            
            int randomDirection = Random.Range(0, 2);
            if (this.tankRenders.transform.eulerAngles.y == 90)
            {
                if (randomDirection == 0)
                {
                    if (WallCheck[1] && !WallCheck[3])
                    {
                        zAxis = -1;
                        return;
                    }
                    else if (WallCheck[3] && !WallCheck[1])
                    {
                        zAxis = 1;
                        return;
                    }
                }

                xAxis = 1;
            }
            else if (this.tankRenders.transform.eulerAngles.y == 270)
            {
                if (randomDirection == 0)
                {
                    if (WallCheck[1] && !WallCheck[3])
                    {
                        zAxis = -1;
                        return;
                    }
                    else if (WallCheck[3] && !WallCheck[1])
                    {
                        zAxis = 1;
                        return;
                    }
                }

                xAxis = -1;
            }
            else if (this.tankRenders.transform.eulerAngles.y >= 0 && this.tankRenders.transform.eulerAngles.y < 10 || this.tankRenders.transform.eulerAngles.y > -10 && this.tankRenders.transform.eulerAngles.y <= 0)
            {
                if (randomDirection == 0)
                {
                    if (WallCheck[0] && !WallCheck[2])
                    {
                        xAxis = 1;
                        return;
                    }
                    else if (WallCheck[2] && !WallCheck[0])
                    {
                        xAxis = -1;
                        return;
                    }
                }

                zAxis = 1;
            }
            else if (this.tankRenders.transform.eulerAngles.y == 180)
            {
                if (randomDirection == 0)
                {
                    if (WallCheck[0] && !WallCheck[2])
                    {
                        xAxis = 1;
                        return;
                    }
                    else if (WallCheck[2] && !WallCheck[0])
                    {
                        xAxis = -1;
                        return;
                    }
                }

                zAxis = -1;
            }

            if (WallCheck[0] && WallCheck[1] && !WallCheck[3] && !WallCheck[2])
            {
                int randomMovement = Random.Range(0, 2);
                if (randomMovement == 0)
                {
                    xAxis = 1;
                }
                else if (randomMovement == 1)
                {
                    zAxis = -1;
                }
            }
            else if (WallCheck[0] && WallCheck[3] && !WallCheck[2] && !WallCheck[1])
            {
                int randomMovement = Random.Range(0, 2);
                if (randomMovement == 0)
                {
                    xAxis = 1;
                }
                else if (randomMovement == 1)
                {
                    zAxis = 1;
                }
            }
            else if (WallCheck[2] && WallCheck[1] && !WallCheck[0] && !WallCheck[3])
            {
                int randomMovement = Random.Range(0, 2);
                if (randomMovement == 0)
                {
                    xAxis = -1;
                }
                else if (randomMovement == 1)
                {
                    zAxis = -1;
                }
            }
            else if (WallCheck[2] && WallCheck[3] && !WallCheck[0] && !WallCheck[1])
            {
                int randomMovement = Random.Range(0, 2);
                if (randomMovement == 0)
                {
                    xAxis = -1;
                }
                else if (randomMovement == 1)
                {
                    zAxis = 1;
                }
            }
        }
        else if (wallCheckCount == 4)
        {
            print("doing a 4 way move.");
            int randomDirection = Random.Range(0, 4);
            if (randomDirection == 0)
            {
                xAxis = -1;
            }
            else if (randomDirection == 1)
            {
                zAxis = -1;
            }
            else if (randomDirection == 2)
            {
                xAxis = 1;
            }
            else if (randomDirection == 3)
            {
                zAxis = 1;
            }
        }
    }


    private void RotateTurret()
    {
        //This now has to check if the player was spotted in one of the raycasts for the playerCheck method, and when we rotate the turret and its lined up with the player, we are able to call the shootMissle method in order to fire at the player.
        //This whole method has to be rethought.

        //The reason i have a check for if we move left or down is to make sure we keep the rotation consistant with where we are facing, so that when i turn left or down, the controls dont swithc opposite.
        //This way keeps it so that it keeps turning right if we press d when facing down, instead of the opposite. Its just a small quality of life thing.


        // if (MovingDownOrLeft[0])
        // {
        //     if ((xAxis > 0 || xAxis < 0) && !MovementXOrZ[0])
        //         turretObject.transform.Rotate(Vector3.up, xAxis * -TankTurretRotationSpeed * Time.deltaTime,
        //             Space.World);
        // }
        // else if (MovingDownOrLeft[1])
        // {
        //     if ((zAxis > 0 || zAxis < 0) && !MovementXOrZ[1])
        //         turretObject.transform.Rotate(Vector3.up, zAxis * TankTurretRotationSpeed * Time.deltaTime,
        //             Space.World);
        // }
        // else
        // {
        //     if ((xAxis > 0 || xAxis < 0) && !MovementXOrZ[0])
        //         turretObject.transform.Rotate(Vector3.up, xAxis * TankTurretRotationSpeed * Time.deltaTime,
        //             Space.World);
        //     else if ((zAxis > 0 || zAxis < 0) && !MovementXOrZ[1])
        //         turretObject.transform.Rotate(Vector3.up, zAxis * -TankTurretRotationSpeed * Time.deltaTime,
        //             Space.World);
        // }
    }

    private void CheckForWalls()
    {
        //write the comment explaining this and how it works. will be done after.

        Vector3 thisEnemyTankPosition = this.gameObject.transform.position;

        //the wall check should be able to be simplified by checking only 1 array and making sure that none of the values becomes true in the array.
        WallCheck[0] =
            Physics.Raycast(thisEnemyTankPosition,
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.back * raycastOffset,
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(thisEnemyTankPosition + Vector3.forward * raycastOffset,
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.back * (raycastOffset * 0.5f),
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.left, rayCastMaxDistance,
                1 << 6);


        //check for a wall to the top
        WallCheck[1] =
            Physics.Raycast(thisEnemyTankPosition,
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.left * raycastOffset,
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(thisEnemyTankPosition + Vector3.right * raycastOffset,
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.left * (raycastOffset * 0.5f),
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.right * (raycastOffset * 0.5f),
                Vector3.forward, rayCastMaxDistance,
                1 << 6);


        //check for a wall to the right
        WallCheck[2] =
            Physics.Raycast(thisEnemyTankPosition,
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.back * raycastOffset,
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(thisEnemyTankPosition + Vector3.forward * raycastOffset,
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.back * (raycastOffset * 0.5f),
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.right, rayCastMaxDistance,
                1 << 6);


        //Check for a wall to the bottom
        WallCheck[3] =
            Physics.Raycast(thisEnemyTankPosition,
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.left * raycastOffset,
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(thisEnemyTankPosition + Vector3.right * raycastOffset,
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.right * (raycastOffset * 0.5f),
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(thisEnemyTankPosition + Vector3.left * (raycastOffset * 0.5f),
                Vector3.back, rayCastMaxDistance,
                1 << 6);

        //Left these in just to visualize what i am doing with the wall checks.
        if (VisualizeMovementRays)
        {
            Debug.DrawRay(
                thisEnemyTankPosition,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.forward * raycastOffset,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.back * raycastOffset,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.back * (raycastOffset * 0.5f),
                Vector3.left * rayCastMaxDistance, Color.black);


            Debug.DrawRay(
                thisEnemyTankPosition,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.left * raycastOffset,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.right * raycastOffset,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.left * (raycastOffset * 0.5f),
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.right * (raycastOffset * 0.5f),
                Vector3.forward * rayCastMaxDistance, Color.magenta);


            Debug.DrawRay(
                thisEnemyTankPosition,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.forward * raycastOffset,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.back * raycastOffset,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.back * (raycastOffset * 0.5f),
                Vector3.right * rayCastMaxDistance, Color.cyan);


            Debug.DrawRay(
                thisEnemyTankPosition,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.left * raycastOffset,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.right * raycastOffset,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.right * (raycastOffset * 0.5f),
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                thisEnemyTankPosition * 1f + Vector3.left * (raycastOffset * 0.5f),
                Vector3.back * rayCastMaxDistance, Color.green);
        }
    }

    public void DoDamage()
    {
        TankHealth -= 1;
    }

    private void ShootTankTurret()
    {
        //This will now be called from the enemy tanks playerchecker to make sure we first rotate the turret correctly, 
        //then when the aim is in the the correct area, we call this to fire the turret.


        GameObject missleObject = Instantiate(TankMissle,
            MissleSpawnPoint.position + this.GetComponent<Rigidbody>().velocity, MissleSpawnPoint.rotation);
        missleObject.transform.Rotate(90, 0, 0);
        missleObject.GetComponent<Rigidbody>().velocity += MissleSpawnPoint.transform.forward * BulletSpeed;
        missleObject.GetComponent<Rigidbody>().freezeRotation = true;
    }


    private void OnCollisionEnter(Collision collision)
    {
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        lastLocationTime += Time.deltaTime;
        if (lastLocationTime < RecheckCollisionTime)
        {
            //LITERALLY DO NOTHING.
        }
        else
        {
            if (collisionInfo.gameObject.tag.Equals("Enemy"))
            {
                if (xAxis == 1)
                {
                    xAxis = -1;
                }
                else if (xAxis == -1)
                {
                    xAxis = 1;
                }
                else if (zAxis == 1)
                {
                    zAxis = -1;
                }
                else if (zAxis == -1)
                {
                    zAxis = 1;
                }
            }

            lastLocationTime = 0;
        }
    }
}