using UnityEngine;

public class EnemyTankController : MonoBehaviour
{
    //These are private variables for the sake of reference in the code. 
    //The xaxis and zaxis is used for the movement and the rotation of the turret, this is done to avoid 2 getaxis calls.
    private float xAxis = 0, zAxis = 0;

    //This bool checks for the movement along the x and z to be able to tell the turret to only rotate when one or the other is false.
    //Hence in the movement i assign the movementx true and movementz false when im moving along the x so that the turret can only turn if i press the up and down keys instead of it turning consitently 
    //while i move along the x.
    private bool[] MovementXOrZ = {false, false};
    private bool[] MovingDownOrLeft = {false, false};

    [SerializeField] private GameManager gm;
    [SerializeField] private float RecheckMovementTime = 2f;
    private float lastRecheckTime = 0;
    [SerializeField] private float CheckForSamePositionTimer = 2f;
    private float lastPositionTimeCheck = 0;
    private bool newMovementGenerated = false;


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
        print("i am here.");
        if ((this.tankRenders.transform.eulerAngles.y == 0 || this.tankRenders.transform.eulerAngles.y == 180))
        {
            int randDirection = Random.Range(0, 2);

            if (randDirection == 0)
            {
                zAxis = -1;
            }
            else if (randDirection == 1)
            {
                zAxis = 1;
            }
        }
        else if ((this.tankRenders.transform.eulerAngles.y == 90 || this.tankRenders.transform.eulerAngles.y == -90))
        {
            int randDirection = Random.Range(0, 2);
            if (randDirection == 0)
            {
                xAxis = 1;
            }
            else if (randDirection == 1)
            {
                xAxis = -1;
            }
        }
    }

    private void Update()
    {
        if (TankHealth <= 0)
        {
            gm.KillEnemy(this.gameObject);
            Destroy(this.gameObject);
            GameObject explodeTankEffect = Instantiate(tankExplosionEffect, this.gameObject.transform.position,
                Quaternion.identity);
            explodeTankEffect.GetComponent<ParticleSystem>().Play();
            Destroy(explodeTankEffect, 3f);
        }
    }

    private void FixedUpdate()
    {
        CheckForWalls();
        MoveTank();
    }

    private void MoveTank()
    {
        print("x axis " + xAxis);
        print("z axis " + zAxis);

        //similar to the movement for the player but the tank here generates its movement values then  applies that to here.
        //it will keep doing the movement checks and only after a few seconds.
        if ((Time.time - lastRecheckTime) > RecheckMovementTime)
        {
            lastRecheckTime = Time.time;
        }

        if (!newMovementGenerated)
        {
            GenerateRandomDirection();
        }

        Vector3 newXAxisVector;
        Vector3 newZAxisVector;

        tanksRigidBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                                     RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationY |
                                     RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        //movement right
        if (xAxis > 0)
        {
            tanksRigidBody.constraints = RigidbodyConstraints.None;

            newXAxisVector = new Vector3(xAxis * TankSpeed * Time.deltaTime, 0, 0);
            tanksRigidBody.position += newXAxisVector;

            tankRenders.transform.rotation = Quaternion.Euler(0, 90, 0);
            tanksRigidBody.constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            MovementXOrZ[0] = true;
            MovementXOrZ[1] = false;
            MovingDownOrLeft[0] = false;
            MovingDownOrLeft[1] = false;
        }

        //movement left
        else if (xAxis < 0)
        {
            tanksRigidBody.constraints = RigidbodyConstraints.None;

            newXAxisVector = new Vector3(xAxis * TankSpeed * Time.deltaTime, 0, 0);
            tanksRigidBody.position += newXAxisVector;

            tankRenders.transform.rotation = Quaternion.Euler(0, -90, 0);
            tanksRigidBody.constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            MovementXOrZ[0] = true;
            MovementXOrZ[1] = false;
            MovingDownOrLeft[0] = false;
            MovingDownOrLeft[1] = true;
        }

        //movement up
        else if (zAxis > 0)
        {
            tanksRigidBody.constraints = RigidbodyConstraints.None;

            newZAxisVector = new Vector3(0, 0, zAxis * TankSpeed * Time.deltaTime);
            tanksRigidBody.position += newZAxisVector;

            tankRenders.transform.rotation = Quaternion.Euler(0, 0, 0);
            tanksRigidBody.constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            MovementXOrZ[0] = false;
            MovementXOrZ[1] = true;
            MovingDownOrLeft[0] = false;
            MovingDownOrLeft[1] = false;
        }

        //movement down
        else if (zAxis < 0)
        {
            tanksRigidBody.constraints = RigidbodyConstraints.None;

            newZAxisVector = new Vector3(0, 0, zAxis * TankSpeed * Time.deltaTime);
            tanksRigidBody.position += newZAxisVector;

            tankRenders.transform.rotation = Quaternion.Euler(0, 180, 0);
            tanksRigidBody.constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            MovementXOrZ[0] = false;
            MovementXOrZ[1] = true;
            MovingDownOrLeft[0] = true;
            MovingDownOrLeft[1] = false;
        }

        newMovementGenerated = false;
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

        // if (WallCheck[0] && WallCheck[2])
        // {
        //     print("going to continue in direction");
        //     zAxis = zAxis;
        // }
        // else if (WallCheck[1] && WallCheck[3])
        // {
        //     xAxis = xAxis;
        // }


        //Left wall and top wall
        if (WallCheck[0] && WallCheck[1])
        {
            int randomMovement = Random.Range(0, 2);
            if (randomMovement == 0)
            {
                print("Moving right");
                xAxis = 1;
                zAxis = 0;
            }
            else if (randomMovement == 1)
            {
                print("Moving down");
                zAxis = -1;
                xAxis = 0;
            }

            print("Available movement right and bottom");
        }
        //left wall and bottom wall
        else if (WallCheck[0] && WallCheck[3])
        {
            int randomMovement = Random.Range(0, 2);
            if (randomMovement == 0)
            {
                xAxis = 1;
                zAxis = 0;
                print("Moving right");
            }
            else if (randomMovement == 1)
            {
                zAxis = 1;
                xAxis = 0;
                print("Moving up");
            }

            print("Available movement on right and up");
        }
        //right wall and top wall
        else if (WallCheck[2] && WallCheck[1])
        {
            int randomMovement = Random.Range(0, 2);
            if (randomMovement == 0)
            {
                xAxis = -1;
                zAxis = 0;
                print("Moving left");
            }
            else if (randomMovement == 1)
            {
                zAxis = -1;
                xAxis = 0;
                print("Moving down");
            }

            print("Available left and down move.");
        }
        //right wall and bottom wall
        else if (WallCheck[2] && WallCheck[3])
        {
            int randomMovement = Random.Range(0, 2);
            if (randomMovement == 0)
            {
                print("Moving left");
                zAxis = 0;
                xAxis = -1;
            }
            else if (randomMovement == 1)
            {
                print("Moving up");
                xAxis = 0;
                zAxis = 1;
            }

            print("Available left and up move.");
        }
        //if 3 walls are false allowing for 3 places available for movement.
        else if (wallCheckCount == 3)
        {
            int randomDirection = Random.Range(0, 3);
            if (WallCheck[0])
            {
                if (randomDirection == 0)
                {
                    print("Moving down");
                    zAxis = -1;
                    xAxis = 0;
                }
                else if (randomDirection == 1)
                {
                    print("Moving up");
                    zAxis = 1;
                    xAxis = 0;
                }
                else if (randomDirection == 2)
                {
                    print("Moving right");
                    xAxis = 1;
                    zAxis = 0;
                }
            }
            else if (WallCheck[1])
            {
                if (randomDirection == 0)
                {
                    print("Moving down");
                    zAxis = -1;
                    xAxis = 0;
                }
                else if (randomDirection == 1)
                {
                    print("Moving right");
                    xAxis = 1;
                    zAxis = 0;
                }
                else if (randomDirection == 2)
                {
                    print("Moving left");
                    xAxis = -1;
                    zAxis = 0;
                }
            }
            else if (WallCheck[2])
            {
                if (randomDirection == 0)
                {
                    print("Moving left");
                    xAxis = -1;
                    zAxis = 0;
                }
                else if (randomDirection == 1)
                {
                    print("Moving up");
                    zAxis = 1;
                    zAxis = 0;
                }
                else if (randomDirection == 2)
                {
                    print("Moving down");
                    zAxis = -1;
                    xAxis = 0;
                }
            }
            else if (WallCheck[3])
            {
                if (randomDirection == 0)
                {
                    print("Moving up");
                    zAxis = 1;
                    xAxis = 0;
                }
                else if (randomDirection == 1)
                {
                    print("Moving right");
                    xAxis = 1;
                    zAxis = 0;
                }
                else if (randomDirection == 2)
                {
                    print("Moving left");
                    xAxis = -1;
                    zAxis = 0;
                }
            }

            print("Available movement on 3 sides");
        }
        //if all the walls are false available for movmenet of all sides.
        else if (wallCheckCount == 4)
        {
            int randomDirection = Random.Range(0, 4);
            if (randomDirection == 0)
            {
                print("Moving up");
                xAxis = -1;
                zAxis = 0;
            }
            else if (randomDirection == 1)
            {
                print("Moving down");
                zAxis = -1;
                xAxis = 0;
            }
            else if (randomDirection == 2)
            {
                print("Moving right");
                xAxis = 1;
                zAxis = 0;
            }
            else if (randomDirection == 3)
            {
                print("Moving left");
                xAxis = -1;
                zAxis = 0;
            }

            print("Available movement on all sides");
        }
        //if we are just moving in the same direction because none of the checks are true.


        newMovementGenerated = true;
    }


    private void RotateTurret()
    {
        //This now has to check if the player was spotted in one of the raycasts for the playerCheck method, and when we rotate the turret and its lined up with the player, we are able to call the shootMissle method in order to fire at the player.
        //This whole method has to be rethought.

        //The reason i have a check for if we move left or down is to make sure we keep the rotation consistant with where we are facing, so that when i turn left or down, the controls dont swithc opposite.
        //This way keeps it so that it keeps turning right if we press d when facing down, instead of the opposite. Its just a small quality of life thing.
        if (MovingDownOrLeft[0])
        {
            if ((xAxis > 0 || xAxis < 0) && !MovementXOrZ[0])
                turretObject.transform.Rotate(Vector3.up, xAxis * -TankTurretRotationSpeed * Time.deltaTime,
                    Space.World);
        }
        else if (MovingDownOrLeft[1])
        {
            if ((zAxis > 0 || zAxis < 0) && !MovementXOrZ[1])
                turretObject.transform.Rotate(Vector3.up, zAxis * TankTurretRotationSpeed * Time.deltaTime,
                    Space.World);
        }
        else
        {
            if ((xAxis > 0 || xAxis < 0) && !MovementXOrZ[0])
                turretObject.transform.Rotate(Vector3.up, xAxis * TankTurretRotationSpeed * Time.deltaTime,
                    Space.World);
            else if ((zAxis > 0 || zAxis < 0) && !MovementXOrZ[1])
                turretObject.transform.Rotate(Vector3.up, zAxis * -TankTurretRotationSpeed * Time.deltaTime,
                    Space.World);
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
}