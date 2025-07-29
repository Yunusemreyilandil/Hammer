using System;
using NaughtyCharacter;
using UnityEngine;

public enum ERotationBehavior
{
    OrientRotationToMovement,  
    UseControlRotation        
}

[System.Serializable]
public class RotationSettings
{
    [Header("Control Rotation")]
    public float MinPitchAngle = -45.0f;     public float MaxPitchAngle = 75.0f;  

    [Header("Character Orientation")]
    public ERotationBehavior RotationBehavior = ERotationBehavior.OrientRotationToMovement;

    public float MinRotationSpeed = 600.0f;  
    public float MaxRotationSpeed = 1200.0f; 
}

[System.Serializable]
public class MovementSettings
{
    public float Acceleration = 25.0f;     
    public float Decceleration = 25.0f;     
    public float MaxHorizontalSpeed = 8.0f;  
    public float JumpSpeed = 10.0f;         
    public float JumpAbortSpeed = 10.0f;     
}

[System.Serializable]
public class GravitySettings
{
    public float Gravity = 20.0f;            
    public float GroundedGravity = 5.0f;     
    public float MaxFallSpeed = 40.0f;       
}

[System.Serializable]
public class GroundSettings
{
    public LayerMask GroundLayers;          
    public float SphereCastRadius = 0.35f;   
    public float SphereCastDistance = 0.15f; 
}

public class Character : MonoBehaviour
{
    public Controller Controller;
    public MovementSettings MovementSettings;
    public GravitySettings GravitySettings;
    public RotationSettings RotationSettings;
    public GroundSettings GroundSettings;
    public float forceMagnitude; 

    private CharacterController _characterController;

    private float _targetHorizontalSpeed;
    private float _horizontalSpeed;
    private float _verticalSpeed;
    private bool _justWalkedOffALedge;

    private Vector2 _controlRotation;
    private Vector3 _movementInput;
    private Vector3 _lastMovementInput;
    private bool _hasMovementInput;
    private bool _jumpInput;

    public Vector3 Velocity => _characterController.velocity;
    public Vector3 HorizontalVelocity => _characterController.velocity.SetY(0.0f);
    public Vector3 VerticalVelocity => _characterController.velocity.Multiply(0.0f, 1.0f, 0.0f);
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        Controller.Init();             
        Controller.Character = this; 

        _characterController = GetComponent<CharacterController>();
    }

    private void Update() => Controller.OnCharacterUpdate();

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var rigidBody = hit.collider.attachedRigidbody;

        if (rigidBody == null) return;

        var forceDirection = hit.gameObject.transform.position - transform.position;
        forceDirection.y = 0;
        forceDirection.Normalize();

        rigidBody.AddForceAtPosition(forceDirection * forceMagnitude, transform.position, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        Tick(Time.deltaTime); 
        Controller.OnCharacterFixedUpdate();
    }

    private void Tick(float deltaTime)
    {
        UpdateHorizontalSpeed(deltaTime);  
        UpdateVerticalSpeed(deltaTime);   

        var movement = _horizontalSpeed * GetMovementInput() + _verticalSpeed * Vector3.up;
        _characterController.Move(movement * deltaTime); 

        OrientToTargetRotation(movement.SetY(0.0f), deltaTime);

        UpdateGrounded(); 
    }

    public void SetMovementInput(Vector3 movementInput)
    {
        var hasMovementInput = movementInput.sqrMagnitude > 0.0f;

        if (_hasMovementInput && !hasMovementInput)
        {
            _lastMovementInput = _movementInput;
        }

        _movementInput = movementInput;
        _hasMovementInput = hasMovementInput;
    }

    private Vector3 GetMovementInput()
    {
        var movementInput = _hasMovementInput ? _movementInput : _lastMovementInput;
        if (!(movementInput.sqrMagnitude > 1f)) return movementInput;
        movementInput.Normalize();

        return movementInput;
    }

    public void SetJumpInput(bool jumpInput) => _jumpInput = jumpInput;

    public Vector2 GetControlRotation() => _controlRotation;

    public void SetControlRotation(Vector2 controlRotation)
    {
        var pitchAngle = controlRotation.x;
        pitchAngle %= 360.0f;
        pitchAngle = Mathf.Clamp(pitchAngle, RotationSettings.MinPitchAngle, RotationSettings.MaxPitchAngle);

        var yawAngle = controlRotation.y;
        yawAngle %= 360.0f;

        _controlRotation = new Vector2(pitchAngle, yawAngle);
    }

    private bool CheckGrounded()
    {
        var spherePosition = transform.position;
        spherePosition.y = transform.position.y + GroundSettings.SphereCastRadius - GroundSettings.SphereCastDistance;
        var isGrounded = Physics.CheckSphere(spherePosition, GroundSettings.SphereCastRadius,
            GroundSettings.GroundLayers, QueryTriggerInteraction.Ignore);

        return isGrounded;
    }

    private bool CheckGroundedRaycast() => Physics.Raycast(transform.position, Vector3.down, out var hit, Mathf.Infinity);

    private void UpdateGrounded()
    {
        _justWalkedOffALedge = false;

        var isGrounded = CheckGrounded();
        if (IsGrounded && !isGrounded && !_jumpInput)
        {
            _justWalkedOffALedge = true; 
        }

        IsGrounded = isGrounded;
    }

    private void UpdateHorizontalSpeed(float deltaTime)
    {
        var movementInput = _movementInput;
        if (movementInput.sqrMagnitude > 1.0f)
        {
            movementInput.Normalize();
        }

        _targetHorizontalSpeed = movementInput.magnitude * MovementSettings.MaxHorizontalSpeed;
        var acceleration = _hasMovementInput ? MovementSettings.Acceleration : MovementSettings.Decceleration;

        _horizontalSpeed = Mathf.MoveTowards(_horizontalSpeed, _targetHorizontalSpeed, acceleration * deltaTime);
    }

    private void UpdateVerticalSpeed(float deltaTime)
    {
        if (IsGrounded)
        {
            _verticalSpeed = -GravitySettings.GroundedGravity;

            if (_jumpInput)
            {
                _verticalSpeed = MovementSettings.JumpSpeed;
            }
        }
        else
        {
            if (!_jumpInput && _verticalSpeed > 0.0f)
            {
                _verticalSpeed = Mathf.MoveTowards(_verticalSpeed, -GravitySettings.MaxFallSpeed,
                    MovementSettings.JumpAbortSpeed * deltaTime);
            }
            else if (_justWalkedOffALedge)
            {
                _verticalSpeed = 0.0f;
            }

            _verticalSpeed = Mathf.MoveTowards(_verticalSpeed, -GravitySettings.MaxFallSpeed,
                GravitySettings.Gravity * deltaTime);
        }
    }

    private void OrientToTargetRotation(Vector3 horizontalMovement, float deltaTime)
    {
        if (RotationSettings.RotationBehavior == ERotationBehavior.OrientRotationToMovement && horizontalMovement.sqrMagnitude > 0.0f)
        {
            var rotationSpeed = Mathf.Lerp(
                RotationSettings.MaxRotationSpeed, RotationSettings.MinRotationSpeed, _horizontalSpeed / _targetHorizontalSpeed);

            var targetRotation = Quaternion.LookRotation(horizontalMovement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * deltaTime);
        }
        else if (RotationSettings.RotationBehavior == ERotationBehavior.UseControlRotation)
        {
            var targetRotation = Quaternion.Euler(0.0f, _controlRotation.y, 0.0f);
            transform.rotation = targetRotation;
        }
    }
}
