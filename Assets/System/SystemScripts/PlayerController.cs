using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[SelectionBase]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Space]
    [Header("Movement")]
    [Tooltip("The player's movement physics when on the ground.")]
    public PlayerMovementValues groundMovementValues;
    [Tooltip("The player's movement physics when in the air.")]
    public PlayerMovementValues airMovementValues;
    [Tooltip("The player's movement physics when sliding on a wall.")]
    public PlayerMovementValues wallMovementValues;
    [Tooltip("Whether or not the player can 'swim', i.e. jump while in water.")]
    public bool canSwim = true;
    [Tooltip("How fast the camera rotates around the player using player input.")]
    public float lookSpeed = 150;
    [HideInInspector]
    public bool invertYAxis = false;
    [Tooltip("What level of slope is no longer considered \"ground\", on a scale of 0-1, with 1 being flat ground, 0 being a vertical wall, and 0.5 being a 45° slope.")]
    public float groundSlopeThreshold = 0.3f;
    [Tooltip("What level of slope is considered a \"wall\", on a scale of 0-1, with 1 being flat ground, 0 being a vertical wall, and 0.5 being a 45° slope.")]
    public float wallSlopeThreshold = 0.9f;

    [Space]
    [Header("Jumping")]
    [Tooltip("The default stats for performing a jump.")]
    public PlayerJumpValues jumpValues;
    [Tooltip("How many seconds the player can be in the air after running off a ledge, and still jump.")]
    public float coyoteTime = 0.5f;
    [Tooltip("How many seconds the player can press the jump button before they touch the ground, and have it still count as a jump.")]
    public float jumpQueueTime = 0.1f;
    [Tooltip("The maximum upwards speed a player can achieve.")]
    public float maxJumpSpeed = 100f;

    [Space]
    [Header("Double Jumping")]
    [Tooltip("The number of jumps the player can perform while already in the air.")]
    public int doubleJumps = 1;
    [Tooltip("The physics of each subsequent double jump the player does. If the player can perform more double jumps than this list has, the player will continuously use the last value in the list.")]
    public PlayerJumpValues[] doubleJumpValues = new PlayerJumpValues[1];

    [Space]
    [Header("Wall Jumping")]
    [Tooltip("Whether or not the player can jump while sliding down walls.")]
    public bool allowWallJump = true;
    [Tooltip("Whether or not wall jumping shoudl replenish the player's wall jumps.")]
    public bool resetDoubleJumpsOnWall = true;
    [Tooltip("The stats for performing a jump while sliding on a wall.")]
    public PlayerWallJumpValues wallJumpValues;

    [Space]
    [Header("Crouching")]
    [Tooltip("Whether or not the player can crouch.")]
    public bool allowCrouch = true;
    [Tooltip("How high the player's collider is when crouching. This is used to slide under objects.")]
    public float crouchColliderHeight = 0.25f;

    // Camera
    private Camera camera;

    // Movement
    private PlayerMovementValues currentMovementState;
    private bool isInWater = false;

    // Jump
    private PlayerJumpValues currentJumpValues;
    private bool isJumping = false;
    private bool jumpQueued = false;
    private float lastJumpQueueTime = 0;
    private float lastJumpTime = -1;
    private float lastJumpDuration = -1;
    private float jumpStartedThreshold = 0.1f;
    private int jumpsSinceGroundTouch = 0;
    private int currentSequenceJump = -1;
    private float sequenceJumpMinimumtime = 0.135f; // Prevents short hops from triggering sequence jump
    private Transform currentSpawnPoint;
    private Transform startSpawnPoint;
    private float currentGravityScale = 0;

    // Crouching
    private bool isCrouching = false;
    private Vector3 originalColliderSize = Vector3.one;
    private Vector3 originalColliderOffset = Vector3.zero;

    // Looking
    private float rotationX = 30;
    private float rotationY = 0;

    [Space]
    [Header("Miscellaneous")]
    [Tooltip("The global y coordinates that will cause the player to respawn if they fall below.")]
    public float deathHeight = -301f;
    [Tooltip("The sound that plays when the player jumps.")]
    public AudioClip jumpSound;
    [Tooltip("The sound that plays when the player hits a wall.")]
    public AudioClip bumpSound;
    private float bumpSoundVolume = 0.2f;
    private float bumpMinimumImpulse = 7f;
    [Tooltip("The sound that plays when the player dies.")]
    public AudioClip deathSound;

    // System Variables
    [Space]
    public PlayerSystemVariables systemVariables;
    private bool controlEnabled = true;
    private bool isOnGround = false;
    private bool jumpedOffGround = false;
    private float lastOnGroundTime = 0;
    private float lastLandTime = 0;
    private bool onWall = false;
    private Vector3 wallDirection = Vector3.zero;
    private List<Collision> allWallCollisions = new List<Collision>();
    private Vector3 moveDirection = Vector3.zero;
    private Animator[] animators;
    private float lookStickSensitivityModifier = 0.1f;

    // Component References
    private Rigidbody rigidbody;
    private BoxCollider collider;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance != null)
        {
            Debug.Log("WARNING: More than one player detected!");
        }
        instance = this;

        camera = FindObjectOfType<Camera>();

        rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        
        collider = GetComponent<BoxCollider>();
        audioSource = GetComponentInChildren<AudioSource>();

        currentMovementState = groundMovementValues;

        originalColliderSize = collider.size;
        originalColliderOffset = collider.center;

        startSpawnPoint = new GameObject().transform;
        startSpawnPoint.position = transform.position;
        startSpawnPoint.gameObject.name = "InitialSpawnPoint";
        currentSpawnPoint = startSpawnPoint;

        animators = GetComponentsInChildren<Animator>(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        ReassesCollisionDirection();
        DetermineMovementState();
        CheckWaterState();

        if (controlEnabled && (isCrouching && !isInWater))
        {
            SetFriction(currentMovementState.crouchFriction);
        }
        else if (controlEnabled && !IsTryingToStop(moveDirection))
        {
            SetFriction(currentMovementState.moveFriction);
            if (moveDirection != Vector3.zero) transform.forward = moveDirection;
            rigidbody.AddForce(new Vector3(moveDirection.x * currentMovementState.moveAcceleration, 0, moveDirection.z * currentMovementState.moveAcceleration));
        }
        else
        {
            SetFriction(currentMovementState.stopFriction);
        }

        if ((IsOnGround() || (resetDoubleJumpsOnWall && allowWallJump && IsOnWall())) && (Time.time - lastJumpTime) > jumpStartedThreshold)
        {
            jumpsSinceGroundTouch = 0;
        }

        if (controlEnabled && jumpQueued)
        {
            if (CanJump() && (Time.time - lastJumpQueueTime) < jumpQueueTime)
            {
                jumpQueued = false;
                DetermineJumpValues();
                PerformJump();
            }
        }

        DetermineGravityScale();
        ApplyGravity();

        rigidbody.linearVelocity = new Vector3(
            Mathf.Clamp(rigidbody.linearVelocity.x, -currentMovementState.maxSpeed, currentMovementState.maxSpeed),
            Mathf.Clamp(rigidbody.linearVelocity.y, -currentMovementState.maxFallSpeed, maxJumpSpeed),
            Mathf.Clamp(rigidbody.linearVelocity.z, -currentMovementState.maxSpeed, currentMovementState.maxSpeed)
        );

        SetAnimatorStates();

        if (transform.position.y < deathHeight) Respawn();
    }
    
    void Update()
    {
        Vector2 inputDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        moveDirection = new Vector3(inputDirection.x, 0, inputDirection.y);
        moveDirection = Quaternion.Euler(0, camera.transform.eulerAngles.y, 0) * moveDirection;

        if(Input.GetButtonDown("Jump"))
        {
            JumpButtonPressed();
        }
        else if(Input.GetButtonUp("Jump"))
        {
            JumpButtonReleased();
        }

        CheckIfCrouching();

        DetermineCameraRotation();
    }

    private void SetFriction(float aFriction)
    {
        PhysicsMaterial material = collider.sharedMaterial;
        material.dynamicFriction = aFriction;
        material.staticFriction = aFriction;
        collider.sharedMaterial = material;
        if(!isInWater) rigidbody.linearDamping = currentMovementState.airDrag;
    }

    private void DetermineCameraRotation()
    {
        if(CameraRotate.instance != null)
        {
            Vector2 lookInput = new Vector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            );

            lookInput += new Vector2(
                Input.GetAxisRaw("Look X"),
                Input.GetAxisRaw("Look Y")
            ) * lookStickSensitivityModifier;
        
        
            rotationX += lookInput.y * Time.deltaTime * lookSpeed * (invertYAxis ? -1f : 1f);
            rotationX = Mathf.Clamp(rotationX, -90, 90);
            rotationY -= lookInput.x * Time.deltaTime * lookSpeed ;
            CameraRotate.instance.SetPlayerCameraAngle(Quaternion.Euler(rotationX, rotationY, 0));
        }
    }


    // --------- Movement States ---------

    private void DetermineMovementState()
    {
        if (IsOnGround())
        {
            currentMovementState = groundMovementValues;
            isJumping = false;
        }
        else if (IsOnWall())
        {
            currentMovementState = wallMovementValues;
            isJumping = false;
        }
        else
        {
            currentMovementState = airMovementValues;
        }
    }

    private bool IsOnGround()
    {
        return isOnGround || (canSwim && isInWater);
    }

    private bool IsOnGroundWithCoyoteTime()
    {
        return IsOnGround() || ((jumpsSinceGroundTouch <= 0) && (Time.time - lastOnGroundTime) < coyoteTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        CheckCollisionImpulse(other);
    }

    private void ReassesCollisionDirection()
    {
        Vector3 colliderCenter = collider.transform.TransformPoint(collider.center);
        Vector3 colliderHalfExtents = GetPositiveVector(collider.transform.TransformVector(collider.size * 0.5f));
        CheckWallStates(colliderHalfExtents);
        CheckGroundState(colliderHalfExtents);
    }

    private void CheckGroundState(Vector3 colliderHalfExtents)
    {
        bool onGroundLastFrame = isOnGround;
        
        isOnGround = false;
        foreach(RaycastHit each in Physics.BoxCastAll(transform.position, colliderHalfExtents * 0.9f, Vector3.down, Quaternion.identity, 1f, systemVariables.groundMask, QueryTriggerInteraction.Ignore))
        {
            if(each.normal.y > groundSlopeThreshold)
            {
                // Debug.Log("Ground hit: " + each.transform.gameObject.name + ", " + each.normal);
                isOnGround = true;
            }
        }

        if (onGroundLastFrame && !isOnGround)
        {
            lastOnGroundTime = Time.time;
        }

        if (!onGroundLastFrame && isOnGround)
        {
            lastLandTime = Time.time;
            if (systemVariables.landParticle != null) systemVariables.landParticle.Play();
            jumpedOffGround = false;
        }
    }

    private void CheckWallStates(Vector3 colliderHalfExtents)
    {
        bool wasOnWall = onWall;

        onWall = false;
        wallDirection = Vector3.zero;
        foreach(RaycastHit each in Physics.BoxCastAll(transform.position, colliderHalfExtents * 0.99f, moveDirection, Quaternion.identity, 1f, systemVariables.groundMask, QueryTriggerInteraction.Ignore))
        {
            if(Mathf.Abs(each.normal.z) > wallSlopeThreshold || Mathf.Abs(each.normal.x) > wallSlopeThreshold)
            {
                // Debug.Log("Wall hit: " + each.transform.gameObject.name + ", " + each.normal);
                onWall = true;
                wallDirection += each.normal;
            }
        }
    }
    
    private Vector3 GetPositiveVector(Vector3 input)
    {
        return new Vector3(
            Mathf.Abs(input.x),
            Mathf.Abs(input.y),
            Mathf.Abs(input.z)
        );
    }

    private void CheckWaterState()
    {
        bool wasOnWall = onWall;
        Physics.queriesHitTriggers = false;
        Vector3 worldCenter = collider.transform.TransformPoint(collider.center);
        Vector3 worldHalfExtents = GetPositiveVector(collider.transform.TransformVector(collider.size * 0.5f));
        Collider[] colliders = Physics.OverlapBox(worldCenter, worldHalfExtents, collider.transform.rotation, systemVariables.groundMask,QueryTriggerInteraction.Collide);
        
        isInWater = false;
        foreach(Collider each in colliders)
        {
            if (each.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                isInWater = true;
            }
        }
    }

    private bool IsOnWall()
    {
        return onWall;
    }

    private bool IsTryingToStop(Vector3 moveDirection)
    {
        if (!IsOnWall() && moveDirection == Vector3.zero) return true;
        if (!IsOnGround() && onWall && moveDirection != Vector3.zero && Vector3.Angle(moveDirection, -wallDirection) <= 45) return true;
        return false;
    }



    private void DetermineGravityScale()
    {
        if (isCrouching)
        {
            SetGravity(currentMovementState.crouchGravity);
        }
        else if (isJumping && Mathf.Abs(rigidbody.linearVelocity.y) < currentJumpValues.airHangThreshold && (Time.time - lastJumpTime) > jumpStartedThreshold)
        {
            SetGravity(currentJumpValues.airHangGravity);
        }
        else if (isJumping && rigidbody.linearVelocity.y > 0)
        {
            SetGravity(currentJumpValues.jumpGravity);
        }
        else
        {
            SetGravity(currentMovementState.fallGravity);
        }
    }

    private void SetGravity(float aGravity)
    {
        currentGravityScale = aGravity;
    }

    private void ApplyGravity()
    {
        rigidbody.AddForce(Vector3.down * currentGravityScale, ForceMode.Acceleration);
    }

    void OnCollisionStay(Collision other)
    {
        CheckCollisionImpulse(other);
    }

    private void CheckCollisionImpulse(Collision other)
    {
        ContactPoint[] contacts = new ContactPoint[other.contactCount];
		other.GetContacts(contacts);
		float totalImpulse = 0;
		foreach (ContactPoint contact in contacts) {
			totalImpulse += contact.impulse.magnitude;
		}
        if(totalImpulse > bumpMinimumImpulse)
        {
            // Debug.Log(totalImpulse);
            PlayBumpSound();
        }
    }


    // --------- Jumping ---------

    private bool CanJump()
    {
        return IsOnGroundWithCoyoteTime()
            || (IsOnWall() && allowWallJump)
            || (jumpsSinceGroundTouch <= GetAvailableJumps() && !isInWater);
    }

    private int GetAvailableJumps()
    {
        if (jumpedOffGround)
        {
            return doubleJumps;
        }
        else
        {
            return doubleJumps - 1;
        }
    }

    private void DetermineJumpValues()
    {
        if (!IsOnGround() && IsOnWall())
        {
            currentJumpValues = wallJumpValues;
        }
        else if (IsOnGroundWithCoyoteTime()) // Ground Sequence Jumps
        {
            currentJumpValues = jumpValues;
        }
        else // Air Multi Jumps
        {
            currentJumpValues = doubleJumpValues[Mathf.Clamp(jumpsSinceGroundTouch - 1, 0, doubleJumpValues.Length - 1)];
        }
    }

    private void PerformJump()
    {
        SetGravity(currentJumpValues.jumpGravity);
        rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x,Mathf.Max(0, rigidbody.linearVelocity.y),rigidbody.linearVelocity.z);
        rigidbody.AddForce(new Vector3(0, currentJumpValues.jumpForce, 0));
        if (currentJumpValues is PlayerWallJumpValues)
        {
            PlayerWallJumpValues wallJumpValues = currentJumpValues as PlayerWallJumpValues;
            if (IsOnWall() && !IsOnGround())
            {
                rigidbody.AddForce(wallDirection * wallJumpValues.horizontalJumpForce);
            }
        }
        if (IsOnGroundWithCoyoteTime()) jumpedOffGround = true;
        isJumping = true;
        lastJumpTime = Time.time;
        jumpsSinceGroundTouch++;
        SetAnimatorTrigger("jump");
        PlaySfx(jumpSound,
            Random.Range(1f - systemVariables.randomJumpSoundPitchFluctuation, 1f + systemVariables.randomJumpSoundPitchFluctuation)
                 + (jumpsSinceGroundTouch * systemVariables.jumpSoundSequencePitchIncrease)
            );
    }

    private void QueueJump()
    {
        jumpQueued = true;
        lastJumpQueueTime = Time.time;
    }


    // --------- Input ---------

    public void JumpButtonPressed()
    {
        if (controlEnabled)
        {
            QueueJump();
        }
    }

    public void JumpButtonReleased()
    {
        isJumping = false;
        SetGravity(currentMovementState.fallGravity);
        lastJumpDuration = Time.time - lastJumpTime;
        ResetAnimatorTrigger("jump");
    }

    public void CheckIfCrouching()
    {
        bool crouchPressed = Input.GetButton("Crouch");
        if (controlEnabled && allowCrouch && !isCrouching && crouchPressed)
        {
            isCrouching = true;
            collider.size = new Vector2(originalColliderSize.x, crouchColliderHeight);
            collider.center = new Vector3(originalColliderOffset.x, -(crouchColliderHeight / 2), originalColliderOffset.z);
            if (systemVariables.slideParticle != null && !systemVariables.slideParticle.isPlaying)
            {
                systemVariables.slideParticle.Play();
            }
        }
        else if (isCrouching && (!crouchPressed || !controlEnabled || !allowCrouch))
        {
            isCrouching = false;
            collider.size = originalColliderSize;
            collider.center = originalColliderOffset;
            if (systemVariables.slideParticle != null && systemVariables.slideParticle.isPlaying)
            {
                systemVariables.slideParticle.Stop();
            }
        }
    }

    // --------- Spawning ---------

    public void Respawn()
    {
        if (systemVariables.deathParticlePrefab != null)
        {
            GameObject.Instantiate(systemVariables.deathParticlePrefab, transform.position, Quaternion.identity);
        }
        StartCoroutine((ResetCameraCoroutine()));
        PlaySfx(deathSound, 1);

        if (currentSpawnPoint != null)
        {
            transform.position = currentSpawnPoint.position;
        }
        else if (startSpawnPoint != null)
        {
            transform.position = startSpawnPoint.position;
        }
    }

    private IEnumerator ResetCameraCoroutine()
    {
        CameraFollow.instance.SetCameraTarget(null);
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.isKinematic = true;
        EnablePlayerControl(false);
        if (systemVariables.spritesParent != null) systemVariables.spritesParent.SetActive(false);
        yield return new WaitForSeconds(systemVariables.waitToResetCameraAfterDeath);
        CameraFollow.instance.FocusCameraToPlayer();
        if (systemVariables.spritesParent != null) systemVariables.spritesParent.SetActive(true);
        EnablePlayerControl(true);
        rigidbody.isKinematic = false;
    }

    public void SetSpawnPoint(Transform aPoint)
    {
        currentSpawnPoint = aPoint;
    }


    // --------- Animation ---------

    private void SetAnimatorStates()
    {
        SetAnimatorBool("onGround", IsOnGround());
        SetAnimatorBool("falling", !IsOnGround() && IsFalling());
        SetAnimatorBool("crouching", isCrouching);
        // SetAnimatorBool("onWall", onWall);
        SetAnimatorFloat("VerticalSpeed", moveDirection.y != 0 ? (GetComponent<Rigidbody>().linearVelocity.y/currentMovementState.maxSpeed) : 0);
        SetAnimatorFloat("HorizontalSpeed", moveDirection.x != 0 ? (GetComponent<Rigidbody>().linearVelocity.x/currentMovementState.maxSpeed) : 0);
        SetAnimatorInt("AirJumps", jumpsSinceGroundTouch);
    }

    private void SetAnimatorTrigger(string aTrigger)
    {
        foreach (Animator each in animators)
        {
            if (each != null && each.gameObject.activeInHierarchy) each.SetTrigger(aTrigger);
        }
    }

    private void ResetAnimatorTrigger(string aTrigger)
    {
        foreach (Animator each in animators)
        {
            if (each != null && each.gameObject.activeInHierarchy) each.ResetTrigger(aTrigger);
        }
    }

    private void SetAnimatorBool(string aBoolName, bool aBoolValue)
    {
        foreach (Animator each in animators)
        {
            if (each != null && each.gameObject.activeInHierarchy) each.SetBool(aBoolName, aBoolValue);
        }
    }

    private void SetAnimatorFloat(string aName, float aValue)
    {
        foreach (Animator each in animators)
        {
            if (each != null && each.gameObject.activeInHierarchy) each.SetFloat(aName, aValue);
        }
    }

    private void SetAnimatorInt(string aName, int aValue)
    {
        foreach (Animator each in animators)
        {
            if (each != null && each.gameObject.activeInHierarchy) each.SetInteger(aName, aValue);
        }
    }

    public bool IsFalling()
    {
        return rigidbody.linearVelocity.y < 0;
    }

    // --------- Audio ---------

    private void PlaySfx(AudioClip clip, float pitch)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip, systemVariables.sfxVolume);
        }
    }

    private void PlayBumpSound()
    {
        if(audioSource != null && bumpSound != null)
        {
            audioSource.pitch = 1;
            audioSource.PlayOneShot(bumpSound, systemVariables.sfxVolume * bumpSoundVolume);
        }
    }

    // --------- External Manipulation ---------

    public void EnablePlayerControl(bool enable)
    {
        controlEnabled = enable;
        if (!controlEnabled)
        {
            isCrouching = false;
            isJumping = false;
            jumpQueued = false;
        }
    }

    public void SetDoubleJumpCount(int amount)
    {
        doubleJumps = amount;
    }

    public void AdjustDoubleJumpCount(int amount)
    {
        doubleJumps += amount;
    }

    public void EnableCrouch(bool enable)
    {
        allowCrouch = enable;
    }

    public void EnableWallJump(bool enable)
    {
        allowWallJump = enable;
    }

    public void EnableWallsResetDoubleJumps(bool enable)
    {
        resetDoubleJumpsOnWall = enable;
    }

    public void EnableSwim(bool enable)
    {
        canSwim = enable;
    }

    public void RefreshDoubleJumps()
    {
        jumpsSinceGroundTouch = 0;
    }

    void OnDrawGizmos()
    {
        // Draw line at deathHeight
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawLine(new Vector3(-5000, deathHeight, 0), new Vector3(5000, deathHeight, 0));
    }
}

[System.Serializable]
public class PlayerMovementValues
{
    [Space]
    [Header("Moving")]
    [Tooltip("How quickly the player accelerates.")]
    public float moveAcceleration = 40f;
    [Tooltip("How much friction the player experiences when actively moving left/right.")]
    public float moveFriction = 0f;
    [Tooltip("How much friction the player experiences when not actively moving.")]
    public float stopFriction = 3f;
    [Tooltip("How much air drag the player experiences.")]
    public float airDrag = 0.4f;
    [Tooltip("The maximum left/right speed the player can achieve.")]
    public float maxSpeed = 16f;

    [Space]
    [Header("Falling")]
    [Tooltip("How much gravity the player experiences.")]
    public float fallGravity = 3f;
    [Tooltip("The maximum downwards speed the player can achieve.")]
    public float maxFallSpeed = 50f;

    [Space]
    [Header("Crouching")]
    [Tooltip("The friction the player experiences when crouching. Use a low number to slide.")]
    public float crouchFriction = 0.01f;
    [Tooltip("The gravity the player experiences when crouching. Use a high number to fast fall while holding down.")]
    public float crouchGravity = 12f;
}

[System.Serializable]
public class PlayerJumpValues
{
    [Tooltip("The amount of upwards force the player experiences when jumping.")]
    public float jumpForce = 600f;
    [Tooltip("The amount of gravity the player experiences when jumping.")]
    public float jumpGravity = 1.5f;
    [Tooltip("A number to quantify how long the player will 'hang' at the apex of their jump (not specifically seconds).")]
    public float airHangThreshold = 0.35f;
    [Tooltip("The amount of gravity the player experiences when at the apex of their jump. A low number here will allow the player to 'hang' in the air.")]
    public float airHangGravity = 1.7f;
}

[System.Serializable]
public class PlayerWallJumpValues : PlayerJumpValues
{
    [Tooltip("The amount of outwards force the player experiences when jumping while sliding on a wall. Use a high number to encourage left/right wall jumping, or a low number to encourage wall climbing.")]
    public float horizontalJumpForce = 500f;
}

[System.Serializable]
public class PlayerSystemVariables
{
    public LayerMask groundMask;
    public GameObject spritesParent;
    public SpriteRenderer[] flipSprites;
    public float waitToResetCameraAfterDeath = 2;
    public float sfxVolume = 1;
    public float randomJumpSoundPitchFluctuation = 0.05f;
    public float jumpSoundSequencePitchIncrease = 0.1f;

    [Space]
    [Header("Particles")]
    public ParticleSystem landParticle;
    public ParticleSystem leftWallHitParticle;
    public ParticleSystem rightWallHitParticle;
    public ParticleSystem slideParticle;
    public ParticleSystem leftWallSlideParticle;
    public ParticleSystem rightWallSlideParticle;
    public GameObject deathParticlePrefab;
}