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

        //set up the sheep as targets
        private Sheep _sheepTarget;

        //set up the safe zone
        private bool _inSafeZone = false;
        public bool InSafeZone => _inSafeZone;

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

        /// <summary>
        /// Update is called once per frame
        /// </summary>
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
            Collider[] _targetsInViewRadius = Physics.OverlapSphere(transform.position, _sightRadius, _targetsLayer);
            foreach (Collider _target in _targetsInViewRadius)
            {
                // Check if the target is a sheep
                if (_target.CompareTag("Friend"))
                {
                    // Check if there is a clear line of sight to the sheep
                    Vector3 _directionToTarget = (_target.transform.position - transform.position).normalized;
                    float _distanceToTarget = Vector3.Distance(transform.position, _target.transform.position);
                    if (!Physics.Raycast(transform.position, _directionToTarget, _distanceToTarget, _obstaclesLayer))
                    {
                        // The dog can see the sheep and sees that it is not in a safe zone
                        Sheep sheep = _target.GetComponent<Sheep>();
                        if (sheep != null && !sheep.InSafeZone)
                        {
                            //this stops the loop once sheep is found
                            _sheepTarget = sheep;
                            break; 
                        }
                    }
                }
            }
            

        }

        /// <summary>
        /// Determines the dog's behavior based on the presence and proximity of sheep, and updates its movement
        /// </summary>
        private void DecisionMaking()
        {
            // Variables to help with the decision making
           if (_sheepTarget != null)
            {
                float _distanceToSheep = Vector3.Distance(transform.position, _sheepTarget.transform.position);
                if (_distanceToSheep <= 2f)
                {
                    _currentState = DogState.Friend;
                }
                else if (_distanceToSheep <= 5f)
                {
                    _currentState = DogState.Threat;
                }
                else
                {
                    _currentState = DogState.Sneak;
                }
            }
           else
            {
                Search();
            }

                // Determine the current state based on the presence of sheep
                switch (_currentState)
                {
                    case DogState.Friend:
                        Friend();
                        break;
                    case DogState.Sneak:
                        Sneak();
                        break;
                    case DogState.Threat:
                        Threat();
                        break;
                    default:
                        Search();
                        break;
                }

        }
        #endregion

        /// <summary>
        /// This is when the dog is in the friend state
        /// </summary>
        public void Friend()
        {
            //this will make the sheep go towards the dog
            if (_sheepTarget != null)
            {
                //The sheep will move towards the dog and then to the safe zone
                
            }
            else
            {
                Search();
            }
        }

        /// <summary>
        /// This is when the dog is in the sneak state
        /// </summary>
        public void Sneak()
        {
            //this will make the dog invisible to the sheep
            if (_sheepTarget != null)
            {
                //The sheep can't see the dog so it will go around it
                
            }
            else
            {
                Search();
            }
            
        }

        /// <summary>
        /// This is when the dog is in the threat state
        /// </summary>
        public void Threat()
        {
            //This if statement gets the dog to move towards the sheep if it is not in a safe zone
            if (_sheepTarget != null)
            {
                //increases initial speed to max speed when chasing sheep towards safe zone
                Vector3 _currentDirection = (_sheepTarget.transform.position - transform.position).normalized;
                _moveDirection = _currentDirection;
                _initialSpeed = _maxSpeed - 2.5f;

            }
            //else the dog will just wander around
            else
            {
                Search();
            }

        }

        /// <summary>
        /// Default to have the dog wander and look for sheep
        /// </summary>
        public void Search()
        {
            //Keeps the dog from spazzing when walking
            if (_moveDirection == Vector3.zero)
            {
                //pick a random direction to move in
                //Random angle so the dog doesn't just go in a straight line
                float randomAngle = Random.Range(-20f, 20f);
                _moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
                _initialSpeed = _maxSpeed;
                
            }
        }

        /// <summary>
        /// Prevents the dog from getting stuck on the walls
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            //makes sure dog doesn't get stuck in walls
            if (other.CompareTag("Wall"))
            {
                // Reverse direction upon hitting an obstacle
                _moveDirection = -_moveDirection;
            }
        }

        /// <summary>
        /// Make sure to use FixedUpdate for movement with physics based Rigidbody
        /// You can optionally use FixedDeltaTime for movement calculations, but it is not required since fixedupdate is called at a fixed rate
        /// </summary>
        private void FixedUpdate()
        {
            if (!_isActive) return;
           
            // Sets up movement and moves the dog based on the current state
            Vector3 _movement = _moveDirection * _initialSpeed * Time.fixedDeltaTime;
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

///Psuedocode for dog script/Also my thought process
/// What does the dog need to do?
/// What I aim for is to first have the dog spawn and head in a random direction.
/// This would give it a realistic start instead of just going towards the closest sheep immediately.
/// In the fixed update function, I set up movement that updates the dog's position based on its move direction and speed
/// based the FixedDeltaTime.
/// I also set up rotation so that the dog faces the direction it is moving and made sure it keeps moving
/// I then focused on the Perception and Decision Making functions
/// Perception: This is where the dog will find the sheep within its sight radius using a physics overlap sphere similar to the sheep's.
/// I also set up a raycast so if I do add obstacles, the dog won't see through them.
/// The tricky part was setting up the three states for the dog
/// The threat state was the easier one since i had to where once the dog sees a sheep, it slows down and tries to herd or push them to the safe zone
/// The friend state I tried to approach by 
/// 
