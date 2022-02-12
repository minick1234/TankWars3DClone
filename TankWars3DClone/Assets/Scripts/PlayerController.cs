using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //These are private variables for the sake of reference in the code. 
    //The xaxis and zaxis is used for the movement and the rotation of the turret, this is done to avoid 2 getaxis calls.
    private float xAxis, zAxis;

    //This bool checks for the movement along the x and z to be able to tell the turret to only rotate when one or the other is false.
    //Hence in the movement i assign the movementx true and movementz false when im moving along the x so that the turret can only turn if i press the up and down keys instead of it turning consitently 
    //while i move along the x.
    private bool[] MovementXOrZ = {false, false};
    private bool[] MovingDownOrLeft = {false, false};

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

    [SerializeField] private KeyCode turnTurretLeft;
    [SerializeField] private KeyCode turnTurretRight;

    [SerializeField] private Rigidbody tanksRigidBody;

    [Header("Tank Raycast Settings")] [SerializeField]
    private float rayCastMaxDistance = 1.6f;

    [SerializeField] private float raycastOffset = 1f;

    //The reference to the tanks turret object in order to apply rotation to it.

    [Header("Tank Shooting Settings")] [SerializeField]
    //Reference to what key will enable the user to shoot with the tank!
    private KeyCode KeyToShoot;

    [SerializeField] public GameObject turretObject;


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
        //The x axis of the movement between 0 and 1.
        xAxis = Input.GetAxis("Horizontal");
        //the y axis of the movement but in this case its the z between 0 and 1.
        zAxis = Input.GetAxis("Vertical");

        if (TankHealth <= 0)
        {
            Destroy(this.gameObject);
            GameObject explodeTankEffect = Instantiate(tankExplosionEffect, this.gameObject.transform.position,
                Quaternion.identity);
            explodeTankEffect.GetComponent<ParticleSystem>().Play();
            Destroy(explodeTankEffect, 3f);
        }

        RotateTurret();

        //Technically this can be rewritten to use the logic for the velocity in fixedupdate as its using the physics system to apply a force to the object, but i also use the key input here
        //so if i put it in the fixedupdate the button wont register all the time when pressed because the timing is different.
        //Therefore for this assignment i will leave it here to keep things relatively simple.
        ShootTankTurret();
    }

    private void FixedUpdate()
    {
        CheckForWalls();
        MoveTank();
    }

    private void MoveTank()
    {
        Vector3 newXAxisVector;
        Vector3 newZAxisVector;

        tanksRigidBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                                     RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationY |
                                     RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        //movement right
        if (xAxis > 0 && !WallCheck[2])
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
        else if (xAxis < 0 && !WallCheck[0])
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
        else if (zAxis > 0 && !WallCheck[1])
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
        else if (zAxis < 0 && !WallCheck[3])
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
    }

    //this method needs to be corrected still.
    private void RotateTurret()
    {
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

        //Plan on adding a slight aim assist when the tank turret is close enough to an enemy, we just turn it the rest of the way to aim at the enemy, and if a button is pressed again we cancel the input and avoid it.
        //although im not sure about this as it might remove the difficulty factor and make it a little less fun.
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
            | Physics.Raycast(playerPosition + Vector3.back * raycastOffset,
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(playerPosition + Vector3.forward * raycastOffset,
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.back * (raycastOffset * 0.5f),
                Vector3.left, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.left, rayCastMaxDistance,
                1 << 6);


        //check for a wall to the top
        WallCheck[1] =
            Physics.Raycast(playerPosition,
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.left * raycastOffset,
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(playerPosition + Vector3.right * raycastOffset,
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.left * (raycastOffset * 0.5f),
                Vector3.forward, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.right * (raycastOffset * 0.5f),
                Vector3.forward, rayCastMaxDistance,
                1 << 6);


        //check for a wall to the right
        WallCheck[2] =
            Physics.Raycast(playerPosition,
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.back * raycastOffset,
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(playerPosition + Vector3.forward * raycastOffset,
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.back * (raycastOffset * 0.5f),
                Vector3.right, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.right, rayCastMaxDistance,
                1 << 6);


        //Check for a wall to the bottom
        WallCheck[3] =
            Physics.Raycast(playerPosition,
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.left * raycastOffset,
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            |
            Physics.Raycast(playerPosition + Vector3.right * raycastOffset,
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.right * (raycastOffset * 0.5f),
                Vector3.back, rayCastMaxDistance,
                1 << 6)
            | Physics.Raycast(playerPosition + Vector3.left * (raycastOffset * 0.5f),
                Vector3.back, rayCastMaxDistance,
                1 << 6);

        //Left these in just to visualize what i am doing with the wall checks.
        if (VisualizeMovementRays)
        {
            Debug.DrawRay(
                playerPosition,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.forward * raycastOffset,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.back * raycastOffset,
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.left * rayCastMaxDistance, Color.black);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.back * (raycastOffset * 0.5f),
                Vector3.left * rayCastMaxDistance, Color.black);


            Debug.DrawRay(
                playerPosition,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.left * raycastOffset,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.right * raycastOffset,
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.left * (raycastOffset * 0.5f),
                Vector3.forward * rayCastMaxDistance, Color.magenta);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.right * (raycastOffset * 0.5f),
                Vector3.forward * rayCastMaxDistance, Color.magenta);


            Debug.DrawRay(
                playerPosition,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.forward * raycastOffset,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.back * raycastOffset,
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.forward * (raycastOffset * 0.5f),
                Vector3.right * rayCastMaxDistance, Color.cyan);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.back * (raycastOffset * 0.5f),
                Vector3.right * rayCastMaxDistance, Color.cyan);


            Debug.DrawRay(
                playerPosition,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.left * raycastOffset,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.right * raycastOffset,
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.right * (raycastOffset * 0.5f),
                Vector3.back * rayCastMaxDistance, Color.green);
            Debug.DrawRay(
                playerPosition * 1f + Vector3.left * (raycastOffset * 0.5f),
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
            missleObject.GetComponent<Rigidbody>().velocity += MissleSpawnPoint.transform.forward * BulletSpeed;
            missleObject.GetComponent<Rigidbody>().freezeRotation = true;
        }
    }
}