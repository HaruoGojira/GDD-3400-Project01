using UnityEngine;

namespace GDD3400.Project01
{
    /// <summary>
    /// Set up for the dog script
    /// </summary>
    public class Dog : MonoBehaviour
    {
        //set up for an enum
        public enum DogState
        {
            Sneak,
            Friend,
            Threat
        }
        public DogState currentState;

        //sets up rigidbody
        private Rigidbody _rb;
        private Level _level;

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

        #region Perception and Decision Making
        private void Perception()
        {
            
        }

        private void DecisionMaking()
        {
            // Find the closest sheep in the scene



        }
        #endregion

        /// <summary>
        /// Make sure to use FixedUpdate for movement with physics based Rigidbody
        /// You can optionally use FixedDeltaTime for movement calculations, but it is not required since fixedupdate is called at a fixed rate
        /// </summary>
        private void FixedUpdate()
        {
            if (!_isActive) return;
           
            // Move the dog based on the current state
            Vector3 _movement = _moveDirection * _initialSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + _movement);
            //make sure dog stays on the ground
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
