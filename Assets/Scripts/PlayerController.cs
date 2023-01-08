// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

using NeverAlone;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace NeverAlone
{
    public enum PlayerForce
    {
        BURST = 0, // Added directly to the players movement speed, to be controlled by the standard deceleration
        DECAY // An external velocity that decays over time, applied additively to the rigidbody's velocity
    }

    public enum PlayerState
    {
        NONE = 0,
        IDLE,
        RUN,
        AIRBORNE,
        DASH
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        #region Inspector members

        public SpriteRenderer sprite;
        public Transform cameraFocalPoint;

        public Rigidbody2D rigidbody;
        public CapsuleCollider2D standingCollider;

        public PlayerPhysicsData playerPhysicsData;

        #endregion

        #region Input variables

        private bool hasControl = true;

        private Vector2 playerInput = Vector2.zero;
        private Vector2 lastPlayerInput = new(1, 0);
        private Vector2 playerInputDown = Vector2.zero; // Only true on the first frame of key down

        private bool jumpKey = false;
        private bool jumpToConsume = false;
        private bool dashToConsume = false;
        private bool glideKey = false;
        private bool grappleKey = false;
        private bool grappleToConsume = false;

        #endregion

        #region Physics variables

        public Vector2 velocity = Vector2.zero;
        private Vector2 externalVelocity = Vector2.zero;

        // Jump
        private bool canEndJumpEarly = false;
        private bool endedJumpEarly = false;

        private bool jumpBufferUsable = false;
        private bool jumpCoyoteUsable = false;
        private float jumpBufferTimer = 0;
        private float jumpCoyoteTimer = 0;

        private int airJumpsRemaining = 0;

        // Dash
        private bool dashing = false;
        private bool canDash = false;
        private bool dashBufferUsable = false;
        private bool dashCoyoteUsable = false;

        private Vector2 dashVelocity = Vector2.zero;

        private float dashTimer = 0;
        private float dashCooldownTimer = 0;
        private float dashBufferTimer = 0;
        private float dashCoyoteTimer = 0;

        #endregion

        #region Collision variables

        private CapsuleCollider2D activeCollider;

        public bool onGround = false;
        private Vector2 groundNormal = Vector2.zero;
        private Vector2 ceilingNormal = Vector2.zero;

        public bool onWall = false;
        private int wallDirection = 0;
        private Vector2 wallNormal = Vector2.zero;

        private bool detectTriggers = false;

        #endregion

        #region State machine

        public PlayerState playerState = PlayerState.NONE;
        public int facingDirection = 1;

        #endregion

        #region Event actions

        public event Action<bool, float> onGrounded; // Velocity upon hitting ground
        public event Action onJump;
        public event Action onAirJump;
        public event Action<bool> onDash;

        #endregion

        #region External

        public void applyVelocity(Vector2 vel, PlayerForce forceType)
        {
            if (forceType == PlayerForce.BURST) velocity += vel;
            else externalVelocity += vel;
        }

        public void setVelocity(Vector2 vel, PlayerForce velocityType)
        {
            if (velocityType == PlayerForce.BURST) velocity = vel;
            else externalVelocity = vel;
        }

        public void toggleControl(bool control) { hasControl = control; }

        #endregion

        private void Awake()
        {
            // Initialize members
            rigidbody = GetComponent<Rigidbody2D>();
            playerInput = Vector2.zero;
            detectTriggers = Physics2D.queriesHitTriggers;
            Physics2D.queriesStartInColliders = false;
            activeCollider = standingCollider;
            playerState = PlayerState.IDLE;
        }

        private void Update()
        {
            handleInput();
        }

        private void FixedUpdate()
        {
            // Increment timers
            jumpBufferTimer += Time.fixedDeltaTime;
            jumpCoyoteTimer += Time.fixedDeltaTime;
            dashTimer += Time.fixedDeltaTime;
            dashCooldownTimer += Time.fixedDeltaTime;
            dashBufferTimer += Time.fixedDeltaTime;
            dashCoyoteTimer += Time.fixedDeltaTime;

            handlePhysics();
            handleCollisions();

            // Check if player has control
            if (hasControl)
            {
                // Handle movement machanics
                handleJump();
                handleDash();
            }

            move();

            handleStateMachine();

            // Reset input
            playerInputDown = Vector2.zero;
        }

        #region Input

        private void handleInput()
        {
            // Reset inputs at start of frame
            playerInput = Vector2.zero;

            // Check if game is paused
            if (GameManager.gameState == GameState.PAUSED) return;

            // Horizontal input
            if (InputManager.instance.getKey("left")) playerInput.x -= 1;
            if (InputManager.instance.getKey("right")) playerInput.x += 1;
            if (InputManager.instance.getKeyDown("left")) playerInputDown.x -= 1;
            if (InputManager.instance.getKeyDown("right")) playerInputDown.x += 1;
            // Set last horizontal input
            if (playerInput.x != 0) lastPlayerInput.x = playerInput.x;

            // Vertical input
            if (InputManager.instance.getKey("up")) playerInput.y += 1;
            if (InputManager.instance.getKey("down")) playerInput.y -= 1;
            if (InputManager.instance.getKeyDown("up")) playerInputDown.y += 1;
            if (InputManager.instance.getKeyDown("down")) playerInputDown.y -= 1;
            // Set last vertical input
            if (playerInput.y != 0) lastPlayerInput.y = playerInput.y;

            // Jump
            jumpKey = InputManager.instance.getKey("jump");
            if (InputManager.instance.getKeyDown("jump"))
            {
                jumpToConsume = true;
                jumpBufferTimer = 0;
            }

            // Dash
            if (InputManager.instance.getKeyDown("dash"))
            {
                dashToConsume = true;
                dashBufferTimer = 0;
            }

            // Set player's facing direction to last horizontal input
            facingDirection = lastPlayerInput.x >= 0 ? 1 : -1;
        }

        #endregion

        #region Physics

        private void handlePhysics()
        {
            // Check that the player is not dashing
            //    Dashes have constant velocity so we don't need to manage physics
            if (dashing) return;

            #region Vertical physics

            // Airborne
            if (!onGround)
            {
                float airborneAcceleration = playerPhysicsData.fallAcceleration;

                // Check if player ended jump early
                if (endedJumpEarly && velocity.y > 0) airborneAcceleration *= playerPhysicsData.jumpEndEarlyGravityModifier;

                // Accelerate towards maxFallSpeed using airborneAcceleration
                velocity.y = Mathf.MoveTowards(velocity.y, -playerPhysicsData.maxFallSpeed, airborneAcceleration * Time.fixedDeltaTime);
            }

            #endregion

            #region Horizontal physics

            // Player input is in the opposite direction of current velocity
            if (playerInput.x != 0 && velocity.x != 0 && Mathf.Sign(playerInput.x) != Mathf.Sign(velocity.x))
            {
                // Instantly reset velocity
                velocity.x = 0;
            }
            // Deceleration
            else if (playerInput.x == 0 )
            {
                var deceleration = onGround ? playerPhysicsData.groundDeceleration : playerPhysicsData.airDeceleration;

                // Decelerate towards 0
                velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            // Regular Horizontal Movement
            else
            {
                // Accelerate towards max speed
                // Take into account control loss multipliers
                velocity.x = Mathf.MoveTowards(velocity.x, playerInput.x * playerPhysicsData.maxRunSpeed, playerPhysicsData.acceleration * Time.fixedDeltaTime);

                // Reset x velocity when on wall
                if (onWall) velocity.x = 0;
            }

            #endregion
        }

        #endregion

        #region Collisions

        private void handleCollisions()
        {
            Physics2D.queriesHitTriggers = false;

            RaycastHit2D[] groundHits = new RaycastHit2D[2];
            RaycastHit2D[] ceilingHits = new RaycastHit2D[2];
            RaycastHit2D[] wallHits = new RaycastHit2D[2];
            int groundHitCount;
            int ceilingHitCount;
            int wallHitCount;
            bool ceilingCollision = false;

            #region Vertical collisions

            // Raycast to check for vertical collisions
            Physics2D.queriesHitTriggers = false;
            groundHitCount = Physics2D.CapsuleCastNonAlloc(activeCollider.bounds.center, activeCollider.size, activeCollider.direction, 0, Vector2.down, groundHits, playerPhysicsData.raycastDistance, playerPhysicsData.terrainLayer);
            ceilingHitCount = Physics2D.CapsuleCastNonAlloc(activeCollider.bounds.center, activeCollider.size, activeCollider.direction, 0, Vector2.up, ceilingHits, playerPhysicsData.raycastDistance, playerPhysicsData.terrainLayer);
            Physics2D.queriesHitTriggers = detectTriggers;

            // Get normals
            groundNormal = getRaycastNormal(Vector2.down);
            ceilingNormal = getRaycastNormal(Vector2.up);
            float groundAngle = Vector2.Angle(groundNormal, Vector2.up);

            // Enter ground
            if (!onGround && groundHitCount > 0 && groundAngle <= playerPhysicsData.maxWalkAngle)
            {
                onGround = true;
                resetJump();
                resetDash();

                // Invoke event action
                onGrounded?.Invoke(true, Mathf.Abs(velocity.y));
            }
            // Leave ground
            else if (onGround && (groundHitCount == 0 || groundAngle > playerPhysicsData.maxWalkAngle))
            {
                onGround = false;

                // Start coyote timer
                jumpCoyoteTimer = 0;
                dashCoyoteTimer = 0;

                // Invoke event action
                onGrounded?.Invoke(false, 0);
            }
            // On ground
            else if (onGround && groundHitCount > 0 && groundAngle <= playerPhysicsData.maxWalkAngle)
            {
                // Handle slopes
                if (groundNormal != Vector2.zero) // Make sure ground normal exists
                {
                    if (!Mathf.Approximately(Math.Abs(groundNormal.y), 1f))
                    {
                        // Change y velocity to match ground slope
                        float groundSlope = -groundNormal.x / groundNormal.y;
                        velocity.y = velocity.x * groundSlope;

                        // Give the player a constant velocity so that they stick to sloped ground
                        if (velocity.x != 0) velocity.y += playerPhysicsData.groundingForce;
                    }
                }
            }

            // Enter ceiling
            if (ceilingHitCount > 0 && Math.Abs(ceilingNormal.y) > Math.Abs(ceilingNormal.x))
            {
                // Prevent sticking to ceiling if we did an air jump after receiving external velocity w/ PlayerForce.Decay
                externalVelocity.y = Mathf.Min(0f, externalVelocity.y);
                velocity.y = Mathf.Min(0, velocity.y);

                // Set ceiling collision flag to true
                ceilingCollision = true;
            }

            #endregion
        }

        private Vector2 getRaycastNormal(Vector2 castDirection)
        {
            Physics2D.queriesHitTriggers = false;
            var hit = Physics2D.CapsuleCast(activeCollider.bounds.center, activeCollider.size, activeCollider.direction, 0, castDirection, playerPhysicsData.raycastDistance * 2, playerPhysicsData.terrainLayer);
            Physics2D.queriesHitTriggers = detectTriggers;

            if (!hit.collider) return Vector2.zero;

            return hit.normal; // Defaults to Vector2.zero if nothing was hit
        }

        private bool checkPositionClear(Vector2 position)
        {
            Physics2D.queriesHitTriggers = false;
            var hit = Physics2D.OverlapCapsule(position + activeCollider.offset, activeCollider.size - new Vector2(0.1f, 0.1f), activeCollider.direction, 0, playerPhysicsData.terrainLayer);
            Physics2D.queriesHitTriggers = detectTriggers;

            return !hit;
        }

        #endregion

        #region Jump

        private void handleJump()
        {
            bool canUseJumpBuffer = jumpBufferUsable && jumpBufferTimer < playerPhysicsData.jumpBufferTime;
            bool canUseCoyote = jumpCoyoteUsable && jumpCoyoteTimer < playerPhysicsData.coyoteTime;

            // Detect early jump end
            if (!endedJumpEarly && !onGround && !onWall && !jumpKey && velocity.y > 0 && canEndJumpEarly)
            {
                endedJumpEarly = true;
                canEndJumpEarly = false;
            }

            // Check for jump input
            if (!jumpToConsume && !canUseJumpBuffer) return;

            if (onGround || canUseCoyote) normalJump();
            else if (airJumpsRemaining > 0) airJump();

            jumpToConsume = false; // Always consume the flag
        }

        private void normalJump()
        {
            // Reset jump flags
            endedJumpEarly = false;
            canEndJumpEarly = true;
            jumpBufferUsable = false;
            jumpCoyoteUsable = false;

            // Apply jump velocity
            velocity.y = playerPhysicsData.jumpStrength;

            // Invoke event action
            onJump?.Invoke();
        }

        private void airJump()
        {
            // Reset jump flags
            endedJumpEarly = false;
            canEndJumpEarly = true;
            airJumpsRemaining--;

            // Apply air jump velocity
            velocity.y = playerPhysicsData.jumpStrength;
            externalVelocity.y = 0; // Air jump cancels out vertical external forces

            // Invoke event action
            onAirJump?.Invoke();
        }

        private void resetJump()
        {
            // Reset jump flags
            endedJumpEarly = false;
            canEndJumpEarly = false;
            jumpBufferUsable = true;
            if (onGround) jumpCoyoteUsable = true;

            // Reset number of air jumps
            airJumpsRemaining = playerPhysicsData.airJumps;
        }

        #endregion

        #region Dash

        private void handleDash()
        {
            bool canUseDashBuffer = dashBufferUsable && dashBufferTimer < playerPhysicsData.dashBufferTime;
            bool canUseDashCoyote = dashCoyoteUsable && dashCoyoteTimer < playerPhysicsData.dashCoyoteTime;

            // Check for conditions to initiate dash:
            //    Not currently dashing
            //    Player dash input detected or buffered
            //    Can dash or use dash coyote
            //    Dash cooldown elapsed
            if (!dashing && (dashToConsume || canUseDashBuffer) && (canDash || canUseDashCoyote) && dashCooldownTimer > playerPhysicsData.dashCooldownTime)
            {
                // Set dash velocity
                if (onWall) dashVelocity = playerPhysicsData.dashVelocity * new Vector2(-wallDirection, 0);
                else dashVelocity = playerPhysicsData.dashVelocity * new Vector2(lastPlayerInput.x, 0);

                // Set dash flags
                dashing = true;
                if (!onGround && !onWall)
                {
                    if (!canUseDashCoyote)
                    {
                        canDash = false;
                        dashBufferUsable = false;
                    }
                    dashCoyoteUsable = false;
                }

                // Start dash timer
                dashTimer = 0;

                // Remove external velocity
                externalVelocity = Vector2.zero;

                // Invoke event action
                onDash?.Invoke(true);
            }

            // Handle the dash itself
            if (dashing)
            {
                // Maintain dash velocity
                velocity = dashVelocity;

                // Check if dash time has been reached
                if (dashTimer >= playerPhysicsData.dashTime)
                {
                    endDash();

                    // Start dash cooldown timer
                    dashCooldownTimer = 0;

                    // Set player velocity at end of dash
                    velocity.x *= playerPhysicsData.dashEndHorizontalMultiplier;
                    velocity.y = Mathf.Min(0, velocity.y);
                }
            }

            // Reset dash to consume flag regardless
            dashToConsume = false;
        }

        private void endDash()
        {
            if (dashing)
            {
                // Reset dashing flag
                dashing = false;

                // Invoke event action
                onDash?.Invoke(false);
            }
        }

        private void resetDash()
        {
            // Reset dash
            canDash = true;
            if (onGround) dashBufferUsable = true; // Don't allow dash buffer on wall
            dashCoyoteUsable = true;
        }

        #endregion
        
        private void move()
        {
            // Check if player has control
            if (!hasControl) return;

            // Apply velocity to rigidbody
            rigidbody.velocity = velocity + externalVelocity;

            // Decay external velocity
            //externalVelocity = Vector2.MoveTowards(externalVelocity, Vector2.zero, playerPhysicsData.externalVelocityDecay * Time.fixedDeltaTime);
        }

        #region State machine

        private void handleStateMachine()
        {
            // Call corresponding state function
            switch (playerState)
            {
                case PlayerState.NONE: break;
                case PlayerState.IDLE:
                    idleState();
                    break;
                case PlayerState.RUN:
                    runState();
                    break;
                case PlayerState.AIRBORNE:
                    airborneState();
                    break;
                case PlayerState.DASH:
                    dashState();
                    break;
                default: break;
            }
        }

        private void idleState()
        {
            sprite.color = Color.blue;

            // Switch states
            if (dashing) playerState = PlayerState.DASH;
            else if (!onGround) playerState = PlayerState.AIRBORNE;
            else if (playerInput.x != 0) playerState = PlayerState.RUN;
        }

        private void runState()
        {
            sprite.color = Color.red;

            // Switch states
            if (dashing) playerState = PlayerState.DASH;
            else if (!onGround) playerState = PlayerState.AIRBORNE;
            else if (velocity.x == 0) playerState = PlayerState.IDLE;
        }

        private void airborneState()
        {
            sprite.color = Color.yellow;

            // Switch states
            if (dashing) playerState = PlayerState.DASH;
            else if (onGround && velocity.x == 0) playerState = PlayerState.IDLE;
            else if (onGround) playerState = PlayerState.RUN;
        }

        private void dashState()
        {
            sprite.color = Color.green;

            // Switch states
            if (!dashing && onGround) playerState = PlayerState.RUN;
            else if (!dashing) playerState = PlayerState.AIRBORNE;
        }

        #endregion
    }
}