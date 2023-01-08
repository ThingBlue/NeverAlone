using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeverAlone
{
    [CreateAssetMenu]
    public class PlayerPhysicsData : ScriptableObject
    {
        #region Layers

        [Header("LAYERS")]
        [Tooltip("Layer containing player")]
        public LayerMask playerLayer;

        [Tooltip("Layer containing terrain")]
        public LayerMask terrainLayer;

        #endregion

        #region Movement

        [Header("MOVEMENT")]
        [Tooltip("Maximum horizontal velocity")]
        public float maxRunSpeed = 10;

        [Tooltip("Rate of horizontal velocity gain")]
        public float acceleration = 100;

        [Tooltip("Rate of horizontal velocity loss while on ground")]
        public float groundDeceleration = 200;

        [Tooltip("Rate of horizontal velocity loss while airborne")]
        public float airDeceleration = 100;

        [Tooltip("A constant downward velocity applied while on ground")]
        public float groundingForce = -2;

        #endregion

        #region Jump

        [Header("JUMP")]
        [Tooltip("Number of air jumps")]
        public int airJumps = 1;

        [Tooltip("Vertical velocity applied instantly upon jumping")]
        public float jumpStrength = 18;

        [Tooltip("Maximum downwards vertical velocity")]
        public float maxFallSpeed = 40;

        [Tooltip("Rate of downwards vertical velocity gain from gravity")]
        public float fallAcceleration = 50;

        [Tooltip("Downwards velocity applied upon ending a jump early")]
        public float jumpEndEarlyGravityModifier = 5;

        [Tooltip("Amount of time that a jump is buffered")]
        public float jumpBufferTime = 0.1f;

        [Tooltip("Amount of time where jump is still usable after leaving ground")]
        public float coyoteTime = 0.1f;

        #endregion

        #region Dash

        [Header("DASH")]
        [Tooltip("Horizontal velocity applied instantly upon dashing")]
        public float dashVelocity = 25;

        [Tooltip("Duration of dash")]
        public float dashTime = 0.2f;

        [Tooltip("Amount of time that must pass between dashes")]
        public float dashCooldownTime = 0.2f;

        [Tooltip("Percentage of horizontal velocity retained when dash has completed")]
        public float dashEndHorizontalMultiplier = 0.25f;

        [Tooltip("Amount of time a dash is buffered")]
        public float dashBufferTime = 0.1f;

        [Tooltip("Amount of time where dash is still usable after leaving ground or wall")]
        public float dashCoyoteTime = 0.1f;

        #endregion

        #region Collision

        [Header("COLLISION")]
        [Tooltip("The raycast distance for collision detection"), Range(0f, 1.0f)]
        public float raycastDistance = 0.05f;

        [Tooltip("Maximum angle of walkable ground"), Range(0f, 1.0f)]
        public float maxWalkAngle = 30;

        #endregion

    }
}
