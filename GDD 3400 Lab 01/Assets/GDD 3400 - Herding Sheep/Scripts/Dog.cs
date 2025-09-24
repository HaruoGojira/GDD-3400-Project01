using UnityEngine;

namespace GDD3400.Project01
{
    /// <summary>
    /// Set up for the dog script
    /// </summary>
    public class Dog : MonoBehaviour
    {
        //sets up rigidbody
        private Rigidbody _rb;
        private Level _level;

        //set up the sheep list
        private GameObject[] _sheepList;
        private Sheep _currentTarget;

        // variables to set up the dog booleans
        private bool _isActive = true;
        public bool IsActive 
        {
            get => _isActive;
            set => _isActive = value;
        }

        // Required Variables (Do not edit!)
        private float _maxSpeed = 5f;
        private float _sightRadius = 7.5f;
        
        // Layers - Set In Project Settings
        public LayerMask _targetsLayer;
        public LayerMask _obstaclesLayer;

        // Tags - Set In Project Settings
        private string friendTag = "Friend";
        private string threatTag = "Threat";
        private string safeZoneTag = "SafeZone";

        //creating the movement settings for the dog
        private Vector3 _moveDirection;
        private float _initialSpeed;

        //This will set up the states for the dog
        enum DogState
        {
            Friend,
            Sneak,
            Threat
        }
        private DogState _currentState;

        public void Awake()
        {
            // Find the layers in the project settings
            _targetsLayer = LayerMask.GetMask("Targets");
            _obstaclesLayer = LayerMask.GetMask("Obstacles");

            // Get the rigidbody component
            _rb = GetComponent<Rigidbody>();

        }

        private void Update()
        {
            if (!_isActive) return;
            
            Perception();
            DecisionMaking();
        }

        /// <summary>
        /// This method is called to handle the perception of the dog
        /// </summary>
        #region Perception and Decision Making
        private void Perception()
        {
            // Check for nearby sheep within sight radius
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, _sightRadius, _targetsLayer);
            foreach (Collider target in targetsInViewRadius)
            {
                // Check if the target is a sheep
                if (target.CompareTag(friendTag))
                {
                    // Check if there is a clear line of sight to the sheep
                    Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
                    float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, _obstaclesLayer))
                    {
                        // The dog can see the sheep
                        Sheep sheep = target.GetComponent<Sheep>();
                        if (sheep != null && !sheep.InSafeZone)
                        {
                            _currentTarget = sheep;
                            break; // Exit the loop once a visible sheep is found
                        }
                    }
                }
            }


        }

        /// <summary>
        /// Determines the dog's behavior based on the presence and proximity of sheep,  and updates its movement
        /// direction and speed accordingly.
        /// </summary>
        /// <remarks>This method prioritizes targeting the closest sheep that is not in a safe zone.  If
        /// no valid target is found, the dog will wander in a random direction at reduced speed.</remarks>
        private void DecisionMaking()
        {
            //This if statement gets the dog to move towards the sheep if it is not in a safe zone
            if (_currentTarget != null)
            {
                //increases initial speed to max speed when chasing sheep towards safe zone
                Vector3 _currentDirection = (_currentTarget.transform.position - transform.position).normalized;
                _moveDirection = _currentDirection;
                _initialSpeed = _maxSpeed;
            }
            //else the dog will just wander around
            else
            {
               if (_moveDirection == Vector3.zero)
                {
                    //pick a random direction to move in
                    //-45, 45 degrees so that he doesn't go backwards
                    float randomAngle = Random.Range(-45f, 45f);
                    _moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
                    //move at half speed when wandering
                    _initialSpeed = _maxSpeed / 2; 
                }
            }

            //Switch cases for the dog states
            switch(_currentState)
            {
                case DogState.Friend:
                    break;
                case DogState.Sneak:
                    break;
                case DogState.Threat:
                    break;
                default:
                    break;
            }


        }
        #endregion

        /// <summary>
        /// Make sure to use FixedUpdate for movement with physics based Rigidbody
        /// You can optionally use FixedDeltaTime for movement calculations, but it is not required since fixedupdate is called at a fixed rate
        /// </summary>
        private void FixedUpdate()
        {
            if (!_isActive) return;
           
            // Sets up movement and moves the dog based on the current state
            Vector3 _movement = _moveDirection * _initialSpeed * Time.fixedDeltaTime;

            //make sure dog stays on the ground and doesn't fly off
            _movement.y = 0;
            _rb.MovePosition(_rb.position + _movement);

            // Rotate the dog to face the movement direction
            if (_movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
                _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, 360 * Time.fixedDeltaTime);
            }
        }
    }
}

///Psuedocode for dog script
///
