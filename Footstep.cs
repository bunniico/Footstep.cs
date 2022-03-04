namespace Footstep
{
    using UnityEngine;

    [AddComponentMenu ("Entity/Audio/Footstep")]
    public class Footstep : MonoBehaviour
    {
        #region Inspector
        #region Components

        // Components
        [SerializeField]
        private CharacterController character;

        //Rigidbody
        [SerializeField]
        private Rigidbody rigidBody;

        // AudioSource to play footstep sounds from
        [SerializeField]
        private AudioSource footstepSource;

        // Array of footstep soundclips to play
        [SerializeField]
        private AudioClip[] footstepSounds;

        // Footstep Settings
        [Tooltip ("Cooldown (sec) between footsteps")]
        [SerializeField]
        [Min (0.01f)]
        private float footstepRate = 0.5f;

        [Tooltip ("Velocity threshold to play footsteps faster")]
        [SerializeField]
        [Min (0.5f)]
        private float sprintThreshold = 0.5f;

        [Tooltip ("Velocity threshold to play footsteps")]
        [SerializeField]
        [Min (0)]
        private float walkThreshold = 0.1f;

        [Tooltip ("Velocity threshold to stop playing footsteps while falling")]
        [SerializeField]
        [Min (0)]
        private float fallThreshold = 0.2f;

        #endregion Settings

        #region Debug
        
        private Debug debug;
        private Vector3 velocity;
        private bool isCharacterControlled;
        private bool isWalking;
        private bool isFalling;
        private bool isSprinting;
        private bool isPlayingSound;
        private float timeUntilNextStep;

        #endregion Debug
        #endregion Inspector

        #region Coroutines
        private Coroutine playSoundCoroutine;
        #endregion

        #region Unity

        /// <summary>
        /// Unity Awake
        /// </summary>
        private void Awake ()
        {
            GetRigidBodyOrCharacterController ();
            OnAwakeValidate ();

            void GetRigidBodyOrCharacterController ()
            {
                var hasRigidBody = TryGetComponent (out rigidBody);
                var hasCharacterController = TryGetComponent<CharacterController> (out _);

                Debug.Log ("<color=grey>Footstep:" + nameof(GetRigidBodyOrCharacterController) +"</color>");
                // Attempt to get rigidbody
                if (!hasRigidBody)
                {
                    // Attempt to get rigidbody from character controller
                    if (hasCharacterController)
                        isCharacterControlled = true;

                    if (!hasCharacterController)
                        Debug.LogError ("<color=red>Null rigidbody and character controller</color>\nAdd either a rigidbody or character controller component to " + name);
                }
            }
        }

        /// <summary>
        /// Unity Update
        /// </summary>
        private void Update ()
        {
            UpdateVelocity ();
            UpdateMovementStates ();
            PlayFootstep ();

            /// <summary>
            /// Updates velocity either using rigidBody or characterController
            /// </summary>
            void UpdateVelocity ()
            {
                // Using character controller...
                if (isCharacterControlled)
                    velocity = character.velocity;

                // Using rigidbody...
                if (!isCharacterControlled)
                    velocity = rigidBody.velocity;
            }

            /// <summary>
            /// Updates movement state machine
            /// </summary>
            void UpdateMovementStates ()
            {
                isWalking = IsWalking;
                isSprinting = IsSprinting;
                isFalling = IsFalling;
            }
        }

        /// <summary>
        /// Validation on Awake
        /// </summary>
        private void OnAwakeValidate ()
        {
            CheckForRigidBodyCharacterControllerConflict ();

            /// Check for RigidBody/Character Controller Conflict
            void CheckForRigidBodyCharacterControllerConflict ()
            {
                if (TryGetComponent<Rigidbody> (out _) && TryGetComponent<CharacterController> (out _))
                    Debug.LogWarning ("<color=orange>Conflict:RigidBodyAndCharacterController</color>\nRemove one of the conflicting components");
            }
        }

        #endregion Unity

        #region Boolean Simplifiers

        private bool IsFalling   => Mathf.Abs (velocity.y) >= fallThreshold;
        private bool IsWalking   => Mathf.Abs (velocity.x) >= walkThreshold   || Mathf.Abs (velocity.z) >= walkThreshold;
        private bool IsSprinting => Mathf.Abs (velocity.x) >= sprintThreshold && Mathf.Abs (velocity.z) >= sprintThreshold;

        #endregion Boolean Simplifiers

        #region Core

        /// <summary>
        /// Plays a random footstep audio clip using the footstep audiosource
        /// </summary>
        private void PlayFootstep ()
        {
            // Countdown until next step...
            timeUntilNextStep -= Time.deltaTime;

            if (isWalking && !isFalling && timeUntilNextStep < 0)
            {
                if (footstepSource.isPlaying)
                {
                    // This error should be impossible :(
                    Debug.LogError ("<color=red><b>:(</b></color>");
                    return;
                }

                timeUntilNextStep = footstepRate;
                footstepSource.pitch = Random.Range (0.9f, 1.1f);
                footstepSource.PlayOneShot (Clip ());
            }

            AudioClip Clip () => footstepSounds[Random.Range (0, footstepSounds.Length)];
        }

        #endregion Core
    }
}
