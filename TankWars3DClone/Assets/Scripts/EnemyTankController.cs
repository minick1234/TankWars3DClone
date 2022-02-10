using UnityEngine;

namespace DefaultNamespace
{
    public class EnemyTankController
    {

        [SerializeField] private int enemyHealth = 3;
        [SerializeField] private float enemyMovementSpeed = 10f;
        [SerializeField] private float enemyTurretRotationSpeed = 20f;

        //this needs to be implemented.
        [SerializeField] private float enemyRateOfFire = 2f;

        [SerializeField] private GameObject enemyTankExplosion; 
        
        [SerializeField] private GameObject enemyTurretObject;
        
        
        private void Start()
        {
            
        }
        
        private void Update()
        {
            
        }


        private void MoveEnemyTank()
        {
            
        }

        private void MoveEnemyTurretTowardPlayer()
        {
            
        }
        
        
        
        
        
        
        
    }
}