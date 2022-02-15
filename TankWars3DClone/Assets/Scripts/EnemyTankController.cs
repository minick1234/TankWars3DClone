using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyTankController : MonoBehaviour
{
    public float xAxis = 0, zAxis = 0;

    public GameManager gm;

    [Header("Ai Timer Settings")] [SerializeField]
    private float RecheckMovementTime = 2f;

    private float lastRecheckTime = 0;
    [SerializeField] private float RecheckCollisionTime = 2f;
    private float lastLocationTime;
    [SerializeField] private float RecheckSameLocationTime = 1f;
    private float sameLocationRecheck = 0;

    private bool PlayerFound = false;
    private float lastShotFiredTime;

    private Vector3 lastPositionCheck;

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

        PlayerCheck();
        CheckForWalls();
    }

    private void FixedUpdate()
    {
        MoveTank();
    }

    private void MoveTank()
    {
        if (!PlayerFound)
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

            //assuming they have been in the same location with the glitch setting both values to 0, check for a valid no wall and let them go in that direction.
            if ((Time.time - sameLocationRecheck) > RecheckSameLocationTime)
            {
                if ((this.transform.position - lastPositionCheck).magnitude <= 0.25)
                {
                    print("I am stuck, moving reassigning movement.");
                    if (!WallCheck[0])
                    {
                        xAxis = -1;
                        zAxis = 0;
                    }

                    if (!WallCheck[2])
                    {
                        xAxis = 1;
                        zAxis = 0;
                    }

                    if (!WallCheck[1])
                    {
                        zAxis = 1;
                        xAxis = 0;
                    }

                    if (!WallCheck[3])
                    {
                        zAxis = -1;
                        xAxis = 0;
                    }
                }

                lastPositionCheck = this.transform.position;
                sameLocationRecheck = Time.time;
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

        if (WallCheck[0] && WallCheck[1] && !WallCheck[3] && !WallCheck[2])
        {
            int randomMovement = Random.Range(0, 100);

            if (this.tankRenders.transform.eulerAngles.y == 270)
            {
                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    xAxis = 1;
                    zAxis = 0;
                    return;
                }

                if (randomMovement >= 30 && randomMovement <= 99)
                {
                    zAxis = -1;
                    xAxis = 0;
                    return;
                }
            }
            else if (this.tankRenders.transform.eulerAngles.y <= 10 && this.tankRenders.transform.eulerAngles.y >= -10)
            {
                if (randomMovement >= 30 && randomMovement <= 99)
                {
                    xAxis = 1;
                    zAxis = 0;
                    return;
                }

                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    zAxis = -1;
                    xAxis = 0;
                    return;
                }
            }
        }
        else if (WallCheck[0] && WallCheck[3] && !WallCheck[2] && !WallCheck[1])
        {
            int randomMovement = Random.Range(0, 100);

            if (this.tankRenders.transform.eulerAngles.y == 180)
            {
                if (randomMovement >= 30 && randomMovement <= 99)
                {
                    zAxis = 0;
                    xAxis = 1;
                    return;
                }

                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    xAxis = 0;
                    zAxis = 1;
                    return;
                }
            }
            else if (this.tankRenders.transform.eulerAngles.y == 270)
            {
                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    zAxis = 0;
                    xAxis = 1;
                    return;
                }

                if (randomMovement >= 30 && randomMovement <= 99)
                {
                    xAxis = 0;
                    zAxis = 1;
                    return;
                }
            }
        }
        else if (WallCheck[2] && WallCheck[1] && !WallCheck[0] && !WallCheck[3])
        {
            int randomMovement = Random.Range(0, 100);

            if (this.tankRenders.transform.eulerAngles.y == 90)
            {
                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    zAxis = 0;
                    xAxis = -1;
                    return;
                }

                if (randomMovement >= 30 && randomMovement <= 99)
                {
                    xAxis = 0;
                    zAxis = -1;
                    return;
                }
            }
            else if (this.tankRenders.transform.eulerAngles.y <= 10 && this.tankRenders.transform.eulerAngles.y >= -10)
            {
                if (randomMovement >= 30 && randomMovement <= 99)
                {
                    zAxis = 0;
                    xAxis = -1;
                    return;
                }

                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    xAxis = 0;
                    zAxis = -1;
                    return;
                }
            }
        }
        else if (WallCheck[2] && WallCheck[3] && !WallCheck[0] && !WallCheck[1])
        {
            int randomMovement = Random.Range(0, 100);

            if (this.tankRenders.transform.eulerAngles.y == 90)
            {
                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    zAxis = 0;
                    xAxis = -1;
                    return;
                }

                if (randomMovement >= 30 && randomMovement <= 99)
                {
                    xAxis = 0;
                    zAxis = 1;
                    return;
                }
            }
            else if (this.tankRenders.transform.eulerAngles.y == 180)
            {
                if (randomMovement >= 30 && randomMovement <= 99)
                {
                    zAxis = 0;
                    xAxis = -1;
                    return;
                }

                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    xAxis = 0;
                    zAxis = 1;
                    return;
                }
            }
        }

        if (wallCheckCount == 3)
        {
            print("doing a 3 way move.");

            int randomMovement = Random.Range(0, 100);
            if (WallCheck[0] && !WallCheck[3] && !WallCheck[1] && !WallCheck[2])
            {
                if (this.tankRenders.transform.eulerAngles.y == 270)
                {
                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }
                }
                else if (this.tankRenders.transform.eulerAngles.y <= 10 &&
                         this.tankRenders.transform.eulerAngles.y >= -10)
                {
                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }

                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }
                }
                else if (this.tankRenders.transform.eulerAngles.y == 180)
                {
                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }

                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }
                }
            }
            else if (WallCheck[1] && !WallCheck[0] && !WallCheck[3] && !WallCheck[2])
            {
                if (this.tankRenders.transform.eulerAngles.y == 270)
                {
                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }
                }
                else if (this.tankRenders.transform.eulerAngles.y == 90)
                {
                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }
                }
                else if (this.tankRenders.transform.eulerAngles.y <= 10 &&
                         this.tankRenders.transform.eulerAngles.y >= -10)
                {
                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }
                }
            }
            else if (WallCheck[2] && !WallCheck[0] && !WallCheck[1] && !WallCheck[3])
            {
                if (this.tankRenders.transform.eulerAngles.y == 90)
                {
                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }

                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }
                }
                else if (this.tankRenders.transform.eulerAngles.y <= 10 &&
                         this.tankRenders.transform.eulerAngles.y >= -10)
                {
                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }
                }
                else if (this.tankRenders.transform.eulerAngles.y == 180)
                {
                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }

                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        xAxis = 0;
                        zAxis = -1;
                        return;
                    }
                }
            }
            else if (WallCheck[3] && !WallCheck[0] && !WallCheck[1] && !WallCheck[2])
            {
                if (this.tankRenders.transform.eulerAngles.y == 90)
                {
                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }
                }
                else if (this.tankRenders.transform.eulerAngles.y == 180)
                {
                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }
                }
                else if (this.tankRenders.transform.eulerAngles.y == 270)
                {
                    if (randomMovement >= 90 && randomMovement <= 99)
                    {
                        zAxis = 0;
                        xAxis = 1;
                        return;
                    }

                    if (randomMovement >= 0 && randomMovement <= 44)
                    {
                        zAxis = 0;
                        xAxis = -1;
                        return;
                    }

                    if (randomMovement >= 45 && randomMovement <= 89)
                    {
                        xAxis = 0;
                        zAxis = 1;
                        return;
                    }
                }
            }
        }
        else if (wallCheckCount == 4)
        {
            print("doing a 4 way move.");
            int randomMovement = Random.Range(0, 100);

            if (this.tankRenders.transform.eulerAngles.y == 90)
            {
                if (randomMovement >= 90 && randomMovement <= 99)
                {
                    zAxis = 0;
                    xAxis = -1;
                    return;
                }

                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    xAxis = 0;
                    zAxis = -1;
                    return;
                }

                if (randomMovement >= 30 && randomMovement <= 59)
                {
                    zAxis = 0;
                    xAxis = 1;
                    return;
                }

                if (randomMovement >= 60 && randomMovement <= 89)
                {
                    xAxis = 0;
                    zAxis = 1;
                    return;
                }
            }
            else if (this.tankRenders.transform.eulerAngles.y == 270)
            {
                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    zAxis = 0;
                    xAxis = -1;
                    return;
                }

                if (randomMovement >= 30 && randomMovement <= 59)
                {
                    xAxis = 0;
                    zAxis = -1;
                    return;
                }

                if (randomMovement >= 90 && randomMovement <= 99)
                {
                    zAxis = 0;
                    xAxis = 1;
                    return;
                }

                if (randomMovement >= 60 && randomMovement <= 89)
                {
                    xAxis = 0;
                    zAxis = 1;
                    return;
                }
            }
            else if (this.tankRenders.transform.eulerAngles.y <= 10 && this.tankRenders.transform.eulerAngles.y >= -10)
            {
                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    zAxis = 0;
                    xAxis = -1;
                    return;
                }

                if (randomMovement >= 90 && randomMovement <= 99)
                {
                    xAxis = 0;
                    zAxis = -1;
                    return;
                }

                if (randomMovement >= 30 && randomMovement <= 59)
                {
                    zAxis = 0;
                    xAxis = 1;
                    return;
                }

                if (randomMovement >= 60 && randomMovement <= 89)
                {
                    xAxis = 0;
                    zAxis = 1;
                    return;
                }
            }
            else if (this.tankRenders.transform.eulerAngles.y == 180)
            {
                if (randomMovement >= 0 && randomMovement <= 29)
                {
                    zAxis = 0;
                    xAxis = -1;
                    return;
                }

                if (randomMovement >= 30 && randomMovement <= 59)
                {
                    xAxis = 0;
                    zAxis = -1;
                    return;
                }

                if (randomMovement >= 60 && randomMovement <= 89)
                {
                    zAxis = 0;
                    xAxis = 1;
                    return;
                }

                if (randomMovement >= 90 && randomMovement <= 99)
                {
                    xAxis = 0;
                    zAxis = 1;
                    return;
                }
            }
        }
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

    private void PlayerCheck()
    {
        Vector3 tankPosition = this.gameObject.transform.position;
        RaycastHit hit;
        List<GameObject> itemsHit = new List<GameObject>();
        List<GameObject> itemsHitTempCheck = new List<GameObject>();
        bool[] somethingHit1 = new bool[4];
        bool[] somethingHit2 = new bool[4];
        bool[] somethingHit3 = new bool[4];

        somethingHit1[0] = Physics.Raycast(tankPosition + Vector3.left * 0.9f + Vector3.up * 0.75f, -transform.right,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        somethingHit2[0] = Physics.Raycast(
            tankPosition + Vector3.left * 0.9f + Vector3.up * 0.75f + Vector3.back * 0.5f, -transform.right,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        somethingHit3[0] = Physics.Raycast(
            tankPosition + Vector3.left * 0.9f + Vector3.up * 0.75f + Vector3.forward * 0.5f, -transform.right,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);


        Debug.DrawRay(tankPosition + Vector3.left * 0.9f + Vector3.up * 0.75f, -transform.right * 50,
            Color.red);
        Debug.DrawRay(tankPosition + Vector3.left * 0.9f + Vector3.up * 0.75f + Vector3.forward * 0.5f,
            -transform.right * 50,
            Color.red);
        Debug.DrawRay(tankPosition + Vector3.left * 0.9f + Vector3.up * 0.75f + Vector3.back * 0.5f,
            -transform.right * 50,
            Color.red);

        if (somethingHit1[0] && somethingHit2[0] && somethingHit3[0] && itemsHitTempCheck[0] == itemsHitTempCheck[1] &&
            itemsHitTempCheck[0] == itemsHitTempCheck[2])
        {
            itemsHit.Add(hit.collider.gameObject);
        }

        itemsHitTempCheck.Clear();

        somethingHit1[1] = Physics.Raycast(tankPosition + Vector3.right * 0.9f + Vector3.up * 0.75f, transform.right,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        somethingHit2[1] = Physics.Raycast(
            tankPosition + Vector3.right * 0.9f + Vector3.up * 0.75f + Vector3.back * 0.5f, transform.right,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        somethingHit3[1] = Physics.Raycast(
            tankPosition + Vector3.right * 0.9f + Vector3.up * 0.75f + Vector3.forward * 0.5f, transform.right,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        Debug.DrawRay(tankPosition + Vector3.right * 0.9f + Vector3.up * 0.75f,
            transform.right * 50,
            Color.green);
        Debug.DrawRay(tankPosition + Vector3.right * 0.9f + Vector3.up * 0.75f + Vector3.forward * 0.5f,
            transform.right * 50,
            Color.green);
        Debug.DrawRay(tankPosition + Vector3.right * 0.9f + Vector3.up * 0.75f + Vector3.back * 0.5f,
            transform.right * 50,
            Color.green);

        if (somethingHit1[1] && somethingHit2[1] && somethingHit3[1] && itemsHitTempCheck[0] == itemsHitTempCheck[1] &&
            itemsHitTempCheck[0] == itemsHitTempCheck[2])
        {
            itemsHit.Add(hit.collider.gameObject);
        }

        itemsHitTempCheck.Clear();


        somethingHit1[2] = Physics.Raycast(tankPosition + Vector3.forward * 0.9f + Vector3.up * 0.75f,
            transform.forward, out hit,
            50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        somethingHit2[2] = Physics.Raycast(
            tankPosition + Vector3.forward * 0.9f + Vector3.up * 0.75f + Vector3.left * 0.5f,
            transform.forward, out hit,
            50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        somethingHit3[2] = Physics.Raycast(
            tankPosition + Vector3.forward * 0.9f + Vector3.up * 0.75f + Vector3.right * 0.5f,
            transform.forward, out hit,
            50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);


        Debug.DrawRay(tankPosition + Vector3.forward * 0.9f + Vector3.up * 0.75f,
            transform.forward * 50,
            Color.blue);
        Debug.DrawRay(tankPosition + Vector3.forward * 0.9f + Vector3.up * 0.75f + Vector3.right * 0.5f,
            transform.forward * 50,
            Color.blue);
        Debug.DrawRay(tankPosition + Vector3.forward * 0.9f + Vector3.up * 0.75f + Vector3.left * 0.5f,
            transform.forward * 50,
            Color.blue);

        if (somethingHit1[2] && somethingHit2[2] && somethingHit3[2] && itemsHitTempCheck[0] == itemsHitTempCheck[1] &&
            itemsHitTempCheck[0] == itemsHitTempCheck[2])
        {
            itemsHit.Add(hit.collider.gameObject);
        }

        itemsHitTempCheck.Clear();


        somethingHit1[3] = Physics.Raycast(tankPosition + Vector3.back * 0.9f + Vector3.up * 0.75f, -transform.forward,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        somethingHit2[3] = Physics.Raycast(
            tankPosition + Vector3.back * 0.9f + Vector3.up * 0.75f + Vector3.left * 0.5f, -transform.forward,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        somethingHit3[3] = Physics.Raycast(
            tankPosition + Vector3.back * 0.9f + Vector3.up * 0.75f + Vector3.right * 0.5f, -transform.forward,
            out hit, 50,
            ~(1 << 11));
        itemsHitTempCheck.Add(hit.collider.gameObject);

        Debug.DrawRay(tankPosition + Vector3.back * 0.9f + Vector3.up * 0.75f, -transform.forward * 50,
            Color.magenta);
        Debug.DrawRay(tankPosition + Vector3.back * 0.9f + Vector3.up * 0.75f + Vector3.right * 0.5f,
            -transform.forward * 50,
            Color.magenta);
        Debug.DrawRay(tankPosition + Vector3.back * 0.9f + Vector3.up * 0.75f + Vector3.left * 0.5f,
            -transform.forward * 50,
            Color.magenta);

        if (somethingHit1[3] && somethingHit2[3] && somethingHit3[3] && itemsHitTempCheck[0] == itemsHitTempCheck[1] &&
            itemsHitTempCheck[0] == itemsHitTempCheck[2])
        {
            itemsHit.Add(hit.collider.gameObject);
        }

        itemsHitTempCheck.Clear();

        foreach (var item in itemsHit)
        {
            if (item.layer == 8)
            {
                print("Player Spotted");
                PlayerFound = true;
                xAxis = 0;
                zAxis = 0;
                RotateTurret(item);
            }
            else if (item.layer == 9 || item.layer == 10 || item.layer == 6 || item.layer != 8)
            {
                print("No player Sighted");
                PlayerFound = false;
            }
        }
    }

    private void RotateTurret(GameObject player)
    {
        Vector3 aim = this.turretObject.transform.position - player.transform.position;
        aim.Normalize();
        float dotAim = Vector3.Dot(this.turretObject.transform.right, aim);
        if (dotAim <= -0.001)
        {
            turretObject.transform.Rotate(Vector3.up, TankTurretRotationSpeed * Time.deltaTime);
        }
        else if (dotAim >= 0.001)
        {
            turretObject.transform.Rotate(Vector3.up, -TankTurretRotationSpeed * Time.deltaTime);
        }
        else
        {
            ShootTankTurret();
        }
    }


    private void ShootTankTurret()
    {
        if ((Time.time - lastShotFiredTime) < rateOfFire)
        {
        }
        else
        {
            lastShotFiredTime = Time.time;
            GameObject missleObject = Instantiate(TankMissle,
                MissleSpawnPoint.position + this.GetComponent<Rigidbody>().velocity, MissleSpawnPoint.rotation);
            missleObject.transform.Rotate(90, 0, 0);
            missleObject.GetComponent<Rigidbody>().velocity += MissleSpawnPoint.transform.forward * BulletSpeed;
            missleObject.GetComponent<Rigidbody>().freezeRotation = true;
        }
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