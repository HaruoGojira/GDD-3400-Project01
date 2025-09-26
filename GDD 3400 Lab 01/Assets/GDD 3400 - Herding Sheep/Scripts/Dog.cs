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

        //set up the safe zone booleans like the sheep
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

        /// <summary>
        /// This method is called when the script instance is being loaded
        /// </summary>
        public void Awake()
        {
            // Find the layers in the project settings
            _targetsLayer = LayerMask.GetMask("Targets");
            _obstaclesLayer = LayerMask.GetMask("Obstacles");

            // Get the rigidbody component
            _rb = GetComponent<Rigidbody>();

        }

        /// <summary>
        /// This method initializes the dog with a reference to the level and an index for naming
        /// </summary>
        /// <param name="level"></param>
        /// <param name="index"></param>
        public void Initialize(Level level, int index)
        {
            this.name = $"Dog {index}";

            this._level = level;

        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        public void Start()
        {
            // helps to assign the level for the dog
            if (_level == null)
            {
                _level = Level.Instance;
            }
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
                if (_target.CompareTag(friendTag))
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

        #region Decision Making
        /// <summary>
        /// Determines the dog's behavior based on the presence and proximity of sheep, and updates its movement
        /// </summary>
        private void DecisionMaking()
        {
            // Variables to help with the decision making
           if (_sheepTarget != null)
            {
                // Calculates distance and decides the state
                float _distanceToSheep = Vector3.Distance(transform.position, _sheepTarget.transform.position);
                if (_distanceToSheep <= 1.5f)
                {
                    _currentState = DogState.Friend;
                }
                else if (_distanceToSheep <= 6f)
                {
                    _currentState = DogState.Threat;
                }
                else if (_distanceToSheep == _sightRadius)
                {
                    _currentState = DogState.Sneak;
                }
            }
           else
            {
                Search();
            }

            //Switch statement to determine the dog state
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
            //set the dog tag to friend
            gameObject.tag = friendTag;
            
            if (_sheepTarget != null)
            {
                // Dog moves towards safe zone once sheep are close enough
                GameObject _safeZone = GameObject.FindGameObjectWithTag(safeZoneTag);
                if (_safeZone != null)
                {
                    // Calculate distance to safe zone
                    float _distanceToSafeZone = Vector3.Distance(transform.position, _safeZone.transform.position);
                    // Move towards safe zone
                    if (_distanceToSafeZone == 0.1f)
                    {
                        Vector3 _directionToSafe = (_safeZone.transform.position - transform.position).normalized;
                        _moveDirection = _directionToSafe;
                        _initialSpeed = _maxSpeed;
                    }
                    //keeps dog from getting stuck
                    else
                    {
                        Search();
                    }
                }

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
                 //dog moves to sheep slowly
                 Vector3 _directionToSheep = (_sheepTarget.transform.position - transform.position).normalized;
                 _moveDirection = _directionToSheep;
                 _initialSpeed = _maxSpeed - 2f;
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
            //set the dog tag to threat
            gameObject.tag = threatTag;
            //Dog begins to herd the sheep
            if (_sheepTarget != null)
            {
                // Dog finds the safe zone
                GameObject _safezone = GameObject.FindGameObjectWithTag(safeZoneTag);

                if (_safezone != null)
                {
                    // Makes the sheep move towards the safe zone
                    Vector3 _directionToSafe = (_safezone.transform.position - _sheepTarget.transform.position).normalized;

                    // Dog stays behind the sheep to herd them
                    Vector3 _stayBehind = _sheepTarget.transform.position - _directionToSafe * 3f;

                    //adjust initial speed to max speed and applies the movement
                    Vector3 _directionBehind = (_stayBehind - transform.position).normalized;
                    _moveDirection = _directionBehind;
                    _initialSpeed = _maxSpeed - 2.4f;
                }
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
                float randomAngle = Random.Range(0f, 360);
                _moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
                _initialSpeed = _maxSpeed;
                
            }
        }
        #endregion

        /// <summary>
        /// Keeps dog from going into the walls
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            // Check if the dog has entered the safe zone
            if (collision.gameObject.CompareTag("Wall"))
            {
                //turns the dog around
                _moveDirection = -_moveDirection;

                //fix the rotation after it reverses
                Quaternion _targetRotation = Quaternion.LookRotation(_moveDirection);
                _rb.rotation = Quaternion.RotateTowards(_rb.rotation, _targetRotation, 360 * Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Make sure to use FixedUpdate for movement with physics based Rigidbody
        /// You can optionally use FixedDeltaTime for movement calculations, but it is not required since fixedupdate is called at a fixed rate
        /// </summary>
        private void FixedUpdate()
        {
            if (!_isActive) return;

            // Sets up movement for the dog based on FixedDeltaTime
            Vector3 _movement = _moveDirection * _initialSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + _movement);
            _movement.y = 0;

            // Rotate the dog to face the movement direction
            if (_movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
                _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, 360 * Time.fixedDeltaTime);
            }
        }
    }
}

///Psuedocode for dog script/Also my thought process:
///
/// What does the dog need to do?
/// The way I approached this was to first try and see what the sheep and level scripts were doing.
/// Now I didn't look at every single thing they were doing, but I did get the main idea of how they were handled.
/// The sheep script was what I used to implement some of the dog script.
/// I borrwed some of the set up variables and Even labeled the sheep object as _sheepTarget to access the script for the dog
/// I even borrowed the Initialize method and made my own variables for the dog.
/// I created the dog states as enums for my future functions and made my own variables similar to the sheeps
/// 
/// How does the dog move?
/// So I first focused on the FixedUpdate method and stuck with making a movement variable that moves the dog based on FixedDeltaTime.
/// Thankfully Visual Studio provided a solution that helped with both securing the rigidbody component and rotation of the dog.
/// I created a collision method and messed with the playground boundries to make sure the dog doesn't get stuck on the walls
/// 
/// How does the dog states work?
/// This was by far the hardest challenge for me. To make it easier for myself, I seperated the perception and decision into regions to find them easier
/// The perception method was provided by the Visual Studio solution and I modified it to fit my needs.
/// Considering both methods were in the update method, it made it easier to provide the code
/// The decision making I used a switch statement and decided to make it switch based on distance
/// If the dog was farther, it would sneak, to the sheep, if it was closer, it would threaten the sheep, and if it was very close, it would friend the sheep
/// Using the tags was the easiest way to keep track of them
/// I made each state its own method to keep things organized
/// I even had a default search method that keeps the dog wandering around if no sheep are found.
/// The friend method I tried to manage by having the dog turn on the friend tag and make it move towards the safe zone
/// The sneak method I tried to have the dog move slower for a sneaking effect
/// The threat method I had the dog move towards the sheep but stay a bit behind to simulate herding.
/// The threat still needed work, because it would still get too close even after I tried to fix it.
/// The search method was the default method that would make sure the dog didn't stop moving
/// 
/// 
/// 
/// 
/// 
/// Sources: Some of the code was provided by Visual Studio suggestions (Perception method and FixedUpdate method)
///          Some was inspired by the sheep script (Initialize method and some variables)
///          GDT Solutions: https://gamedevtraum.com/en/game-and-app-development-with-unity/unity-tutorials-and-solutions/unity-tutorial-make-objects-chase-follow-in-unity-smooth-lag-free-movement/
///          Stack Overflow: https://stackoverflow.com/questions/59022682/using-unity-scripting-enemy-ai-follow-player
///          ChatGPT: Used to help with creating the dog states since this was the main challenge.
///          (Prompts: "in unity how can I approach making an object invisible to another npc type object, 
///          It will act like a sneak mode and I already have a sneak function set up, so how can i approach this.")
///          ("in unity in c#, what are some ways i can have an object move towards another object. 
///          And once that happens, both go to a certain area or point in a script. I also want to make it stay some distance behind
///          object but still move towards them, and also make sure they don't run into each other.")
///          
///          
/// 



