using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Player : MonoBehaviour, IEventListener<LevelEvent>
{
    protected static Player _newPlayer;
    public static Player PlayerIns
    {
        get
        {
            _newPlayer = FindObjectOfType<Player>();
            return _newPlayer;
        }
    }
    /// Play
    public GameObject ContainedPlayerObject;
    public ParticleSystem[] Exhausts;
    public GameObject DeathEffect;
    public Transform CenterOfMass;
    public AudioSource RocketSound;
    public AudioClip DeathSound;
    public WheelJoint2D[] MotorWheels;
    /// Values for player movement.
    public float MotorPower = 1400f,
    FlightPower = 5f, BrakePower = -14f,
    DecelerationSpeed = 0.35f,
    AirDecelerationSpeed = 0.25f,
    BackRotateForce = 70f,
    FrontRotateForce = 350f,
    MaximumAngularVelocity = 4,
    SkyRotationSpeed = 5f;
    public float MaxSpeed = 14f, MaxFlightSpeed = 20f;
    /// Player's distance to the ground while the flightmode is off.
    public float DistanceToGround { get; protected set; }
    public float GroundDistanceTolerance = 0.1f;

    [Header("Flight-Mode Bezier Settings")]
    public Transform[] Routes;
    public float BezierFollowSpeed;
    public float TimeParameter;
    protected Vector3 BezierFollowPosition;
    protected int _routeNumber = 0;
    protected bool _canAscend = true;
    protected bool _canFly = false;

    public bool ControlsDisabled { get; protected set; } = false;
    [HideInInspector]
    public float CurrentSpeed { get; protected set; }
    [HideInInspector]
    public Vector3 CurrentDirection { get; protected set; }
    public bool MovingForward { get; protected set; } = false;
    public bool RotatingForward { get; protected set; } = false;
    public bool MovingBackward { get; protected set; } = false;
    public bool RotatingBackward { get; protected set; } = false;
    public float AngularVelocity { get; protected set; }
    public bool IsGrounded { get; protected set; }

    /// Some special data to use.
    protected Rigidbody2D _rigidBody;
    protected CapsuleCollider2D _collider;
    protected SpriteRenderer _spriteRenderer;
    protected JointMotor2D _jointMotor;
    protected Vector3 _initialPosition;
    protected Vector3 _newMovement;
    protected Animator _animator;
    protected Vector2 _flightModeBounds;
    protected bool _airTimeActive = false;
    protected bool _flightModeActive = false;
    protected bool _isMoving;
    protected bool _canRotate;
    protected float _distanceToGroundRaycastLength = 10f;
    protected float _secondaryRaycastLength = 10f;
    protected float _powerValueHolder;
    protected float _gravityValueHolder;
    protected GameObject _ground;
    protected LayerMask _collisionMask;
    protected float _awakeTime;

    /// Primary is for calculating distance to ground. Secondary is for detecting ground, slopes and slope angles.
    protected Vector3 _raycastLeftOrigin;
    protected Vector3 _raycastRightOrigin;
    protected Vector2 _secondaryRaycastOrigin;
    protected Vector2 _groundPosition;
    protected bool _groundBelow;
    protected float _slopeAngle;
    protected Vector3 _crossSlopeAngle;
    protected Vector3 _newPosition;

    /// Player rotate values for calculating gained points.
    protected float _rotation, _previousRotation;
    protected float _clockwiseValue; // Holds the values of -2 or 2, in which the values corresponds to clockwise or counter-clockwise rotations;

    /// Controls
    protected bool _rightPressed;
    protected bool _leftPressed;
    protected bool _upPressed;
    protected bool _downPressed;

    protected virtual void Awake()
    {
        _rigidBody = ContainedPlayerObject.GetComponent<Rigidbody2D>();
        _collider = ContainedPlayerObject.GetComponentInChildren<CapsuleCollider2D>();
        _animator = ContainedPlayerObject.GetComponentInChildren<Animator>();
        RocketSound = ContainedPlayerObject.GetComponent<AudioSource>();
        _spriteRenderer = ContainedPlayerObject.GetComponentInChildren<SpriteRenderer>();
        ContainedPlayerObject.transform.SetParent(null);
        this.gameObject.GetComponentNoAlloc<Follower>().Target = ContainedPlayerObject.transform;

        DistanceToGround = -1;
        _awakeTime = Time.time;
        _powerValueHolder = MotorPower;
        _gravityValueHolder = _rigidBody.gravityScale;
    }

    protected virtual void Start()
    {
        _rigidBody.centerOfMass = CenterOfMass.transform.localPosition;
        _jointMotor = MotorWheels[0].motor;

        _rotation = ContainedPlayerObject.transform.rotation.eulerAngles.z;
        _previousRotation = ContainedPlayerObject.transform.rotation.eulerAngles.z;

    }

    protected virtual void Update()
    {
        _rotation = ContainedPlayerObject.transform.rotation.eulerAngles.z;
        AngularVelocity = _rigidBody.angularVelocity;

        CheckDeathBounds();
        if (_flightModeActive && !ControlsDisabled)
        {
            float height = (_collider.bounds.max.y - _collider.bounds.min.y) * this.transform.localScale.y;
            ContainedPlayerObject.transform.position = new Vector3(
                ContainedPlayerObject.transform.position.x,
                Mathf.Clamp(
                    ContainedPlayerObject.transform.position.y,
                    8.6f + height / 2f,
                    15.6f - height / 2f
                    ),
                ContainedPlayerObject.transform.position.z
                );
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!_flightModeActive && _canAscend && _upPressed)
        {
            StartCoroutine(GetReadyToAscend(_routeNumber));
        }
        else if (_flightModeActive && !ControlsDisabled)
        {
            if (_downPressed)
            {
                StartCoroutine(Descend());
            }

            if (_canFly)
            {
                FlightModeMovement();
                _rigidBody.rotation = 0f;
                CurrentSpeed = _rigidBody.velocity.magnitude;
                CurrentDirection = _rigidBody.velocity;
            }
        }
        else if (!_flightModeActive && !ControlsDisabled)
        {
            if (IsGrounded)
            {
                NormalMovement();
            }
            else if (!IsGrounded)
            {
                Rotate();
            }

            CurrentSpeed = _rigidBody.velocity.magnitude;
            CurrentDirection = _rigidBody.velocity;

        }
        else
        {
            Debug.Log("Some error occured at player movement.");
        }

        if (CurrentDirection.x > 0f && CurrentSpeed > 0.4f)
        {
            MovingBackward = false;
            MovingForward = true;
        }
        else if (CurrentDirection.x < 0f && CurrentSpeed > 0.35f)
        {
            MovingForward = false;
            MovingBackward = true;
        }
        else
        {
            MovingForward = false;
            MovingBackward = false;
        }
    }

    protected virtual void LateUpdate()
    {
        if (!_flightModeActive)
        {
            DetectGround();
            EvaluateRotation();
        }
        //RemainAboveGround();
    }

    /// Calculates the distance between player and the ground.
    protected virtual void DetectGround()
    {
        _raycastLeftOrigin = _collider.bounds.min;
        _raycastLeftOrigin.y = _collider.bounds.center.y;
        _secondaryRaycastOrigin = _collider.bounds.center;
        _raycastRightOrigin = _collider.bounds.min;
        _raycastRightOrigin.x = _collider.bounds.max.x;
        _raycastRightOrigin.y = _collider.bounds.center.y;

        RaycastHit2D raycastLeft = RayCast(_raycastLeftOrigin, Vector2.down, _distanceToGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.green, true);

        if (raycastLeft)
        {
            DistanceToGround = raycastLeft.distance;
            _ground = raycastLeft.collider.gameObject;
        }

        RaycastHit2D raycastDown = RayCast(_secondaryRaycastOrigin, Vector2.down, _secondaryRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.blue, true);
        if (raycastDown)
        {
            if (raycastLeft)
            {
                if (raycastDown.distance < DistanceToGround)
                {
                    DistanceToGround = raycastDown.distance;
                    _ground = raycastDown.collider.gameObject;
                }

                _groundPosition = raycastDown.point;
                _groundBelow = true;
                DrawCross((Vector3)_groundPosition, 1f, Color.red);

                _slopeAngle = Vector2.Angle(raycastDown.normal, transform.up);
                _crossSlopeAngle = Vector3.Cross(transform.up, raycastDown.normal);
                if (_crossSlopeAngle.z < 0)
                {
                    _slopeAngle = -_slopeAngle;
                }

                DrawArrow((Vector3)_groundPosition, raycastDown.normal, Color.magenta);
            }
            else
            {
                DistanceToGround = raycastDown.distance;
                _ground = raycastDown.collider.gameObject;
                _groundBelow = false;
            }
        }

        RaycastHit2D raycastRight = RayCast(_raycastRightOrigin, Vector2.down, _distanceToGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.green, true);
        if (raycastRight)
        {
            if (raycastDown)
            {
                if (raycastRight.distance < DistanceToGround)
                {
                    DistanceToGround = raycastRight.distance;
                    _ground = raycastRight.collider.gameObject;
                }
            }
            else
            {
                DistanceToGround = raycastRight.distance;
                _ground = raycastRight.collider.gameObject;
            }
        }

        if (!raycastLeft && !raycastDown && !raycastRight)
        {
            DistanceToGround = -1;
            _ground = null;
        }
        IsGrounded = GroundChecker();
    }

    protected virtual void RemainAboveGround()
    {
        float height = (_collider.bounds.max.y - _collider.bounds.min.y) * this.transform.localScale.y;
        if (this.transform.position.y - height / 2f < _groundPosition.y)
        {
            _newPosition = this.transform.position;
            _newPosition.y = _groundPosition.y + height / 2f;
            this.transform.position = _newPosition;
        }
    }

    protected virtual bool GroundChecker()
    {
        if (DistanceToGround == -1)
        {
            return (false);
        }

        if (DistanceToGround - _collider.bounds.extents.y < GroundDistanceTolerance)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    protected virtual void CheckDeathBounds()
    {

    }

    protected virtual void NormalMovement()
    {
        MotorWheels[1].useMotor = false;

        if (CurrentSpeed > MaxSpeed)
        {
            MotorPower = 0;
        }
        else
        {
            MotorPower = _powerValueHolder;
        }

        if (_rightPressed && !_leftPressed)
        {
            _jointMotor.motorSpeed = Mathf.Lerp(_jointMotor.motorSpeed, -MotorPower, Time.fixedDeltaTime * 1.4f);
            _isMoving = true;
        }
        else
        {
            if (_leftPressed && !_rightPressed)
            {
                if (CurrentSpeed < -MaxSpeed)
                {
                    _jointMotor.motorSpeed = Mathf.Lerp(_jointMotor.motorSpeed, 0, Time.fixedDeltaTime * 3f);
                    _isMoving = true;
                }
                else
                {
                    _jointMotor.motorSpeed = Mathf.Lerp(_jointMotor.motorSpeed, MotorPower / 1.5f, Time.fixedDeltaTime * 1.4f);
                    _isMoving = true;
                }
            }
            else
            {
                _jointMotor.motorSpeed = Mathf.Lerp(_jointMotor.motorSpeed, 0, Time.fixedDeltaTime * DecelerationSpeed);
                _isMoving = false;
            }
        }
        MotorWheels[0].motor = _jointMotor;
    }

    protected virtual void Rotate()
    {
        _jointMotor.motorSpeed = Mathf.Lerp(_jointMotor.motorSpeed, 0, Time.fixedDeltaTime * AirDecelerationSpeed);
        MotorWheels[0].motor = _jointMotor;
        MotorWheels[1].useMotor = true;

        if (_rigidBody.angularVelocity < -MaximumAngularVelocity) { _rigidBody.angularVelocity = -MaximumAngularVelocity; }
        if (_rigidBody.angularVelocity > MaximumAngularVelocity) { _rigidBody.angularVelocity = MaximumAngularVelocity; }

        if (_rightPressed && !_leftPressed)
        {
            _rigidBody.AddTorque(Mathf.Lerp(0, BackRotateForce, SkyRotationSpeed) * Time.fixedDeltaTime);
            RotatingForward = false;
            RotatingBackward = true;
        }
        else if (_leftPressed && !_rightPressed)
        {
            _rigidBody.AddTorque(-Mathf.Lerp(0, FrontRotateForce, SkyRotationSpeed) * Time.fixedDeltaTime);
            RotatingBackward = false;
            RotatingForward = true;
        }
        else if (!_leftPressed && !_rightPressed)
        {
            if (RotatingBackward)
                _rigidBody.AddTorque(Mathf.Lerp(BackRotateForce / 2, 0, SkyRotationSpeed) * Time.fixedDeltaTime);

            else if (RotatingForward)
                _rigidBody.AddTorque(-Mathf.Lerp(FrontRotateForce / 2, 0, SkyRotationSpeed) * Time.fixedDeltaTime);


            if (_rigidBody.angularVelocity == 0f)
            {
                RotatingForward = false;
                RotatingBackward = false;
            }
        }
    }

    protected virtual void EvaluateRotation()
    {
        if (_rotation > 180f)
            _rotation -= 360f;

        if ((Mathf.Abs(_rotation) > 40f && Mathf.Abs(_previousRotation) <= 40f) ||
            (Mathf.Abs(_rotation) < 40f && Mathf.Abs(_previousRotation) >= 40f))
        {
            _clockwiseValue += Mathf.Sign(_rotation - _previousRotation);
        }

        if (Mathf.Abs(_clockwiseValue) == 2)
        {
            if (_clockwiseValue == -2f)
            {
                Debug.Log("Player flipped");
                LevelEvent.TriggerEvent(LevelEventType.PlayerFlipped);
            }
            else if (_clockwiseValue == 2f)
            {
                Debug.Log("Player backflipped");
                LevelEvent.TriggerEvent(LevelEventType.PlayerReverseFlipped);
            }

            _clockwiseValue = 0f;
        }

        _previousRotation = _rotation;
    }

    protected virtual void EvaluateAirTime()
    {
        float _counter = 0;

        if (!IsGrounded)
        {
            if (_counter == 0f)
            {
                StartCoroutine(EvaluateAirTimeHelper(1f));
            }
            else if (_counter >= 1f)
            {
                _counter++;
                Debug.Log("Combo: " + _counter);
            }
        }
        else if (IsGrounded)
        {
            _counter = 0;
            LevelEvent.TriggerEvent(LevelEventType.OnGround);
        }

        IEnumerator EvaluateAirTimeHelper(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            LevelEvent.TriggerEvent(LevelEventType.InAir);
            _counter++;
        }
    }


    /// Movement function when flightMode is on.
    protected virtual void FlightModeMovement()
    {
        Vector2 _forwardMovement = Vector3.right * FlightPower;
        Vector2 _upDownMovement = Vector3.zero;

        if (_rightPressed && !_leftPressed)
        {
            _rigidBody.DORotate(5f, 0.1f);
            _upDownMovement = Vector3.down * FlightPower * 1.5f;
        }
        else if (!_rightPressed && _leftPressed)
        {
            _rigidBody.DORotate(-5f, 0.1f);
            _upDownMovement = Vector3.up * FlightPower * 1.5f;
        }
        else if (!_rightPressed && !_leftPressed)
        {
            _rigidBody.DORotate(0, 0.1f);
            _upDownMovement = Vector3.zero;
        }

        _rigidBody.velocity = _forwardMovement + _upDownMovement;
    }

    protected IEnumerator GetReadyToAscend(int routeNum)
    {
        Routes[0].transform.SetParent(null);
        LevelEvent.TriggerEvent(LevelEventType.Ascending);
        ControlsDisabled = true;
        _canAscend = false;
        DoTheseToAscend();
        yield return _rigidBody.DORotate(65, 2f).WaitForCompletion();
        yield return StartCoroutine(Ascend(routeNum));
        TimeParameter = 0f;
        yield return _rigidBody.DORotate(0, 0.5f).WaitForCompletion();
        Routes[0].transform.SetParent(this.transform);
        Routes[0].transform.position = Vector3.zero;
        LevelEvent.TriggerEvent(LevelEventType.FlightOn);
        _canFly = true;
        ControlsDisabled = false;
    }

    protected virtual void DoTheseToAscend()
    {
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.angularVelocity = 0f;
        _jointMotor.motorSpeed = 0;
        MotorWheels[0].motor = _jointMotor;
        MotorWheels[1].useMotor = true;
        GameObject.Find("BackWheel").SetActive(false);
        GameObject.Find("FrontWheel").SetActive(false);
        _rigidBody.isKinematic = true;
    }

    protected IEnumerator Ascend(int routeNum)
    {
        Vector2 p0 = Routes[routeNum].GetChild(0).position;
        Vector2 p1 = Routes[routeNum].GetChild(1).position;
        Vector2 p2 = Routes[routeNum].GetChild(2).position;
        Vector2 p3 = Routes[routeNum].GetChild(3).position;

        yield return new WaitForSeconds(0.7f);

        while (TimeParameter < 1)
        {
            TimeParameter += Time.fixedDeltaTime * BezierFollowSpeed;
            BezierFollowPosition = Mathf.Pow(1 - TimeParameter, 3) * p0 + 3 * Mathf.Pow(1 - TimeParameter, 2) * TimeParameter * p1 + 3 * (1 - TimeParameter) * Mathf.Pow(TimeParameter, 2) * p2 + Mathf.Pow(TimeParameter, 3) * p3;
            _rigidBody.DORotate(TimeParameter < 0.7f ? 65 : 60, 0.002f).WaitForCompletion();
            _rigidBody.MovePosition(BezierFollowPosition);
            yield return null;
        }

        //TimeParameter = 0f;
        //_routeNumber += 1;

        //if (_routeNumber > Routes.Length - 1)
        //   _routeNumber = 0;
    }

    protected IEnumerator Descend()
    {
        LevelEvent.TriggerEvent(LevelEventType.Descending);
        _canFly = false;
        _rigidBody.isKinematic = false;
        GameObject.Find("BackWheel").SetActive(true);
        GameObject.Find("FrontWheel").SetActive(true);
        MotorWheels[1].useMotor = false;
        yield return new WaitForSeconds(1f);
        LevelEvent.TriggerEvent(LevelEventType.FlightOff);
        yield return new WaitForSeconds(15.0f); // FlightMode cooldown.
        _canAscend = true;
    }

    /// Death, destroy.
    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }

    public virtual void Die()
    {
        if (DeathEffect != null)
        {
            Instantiate(DeathEffect, this.transform.position, this.transform.rotation);
        }
        ContainedPlayerObject.gameObject.SetActive(false);
    }

    public virtual void DisableCollisions()
    {
        _collider.enabled = false;
    }

    public virtual void EnableCollision()
    {
        _collider.enabled = true;
    }

    public virtual void LevelStart() { }

    public virtual void LevelEnd() { }

    public virtual void RightPress()
    {
        _rightPressed = true;
    }

    public virtual void RightKeep() { }

    public virtual void RightRelease()
    {
        _rightPressed = false;
    }

    public virtual void LeftPress()
    {
        _leftPressed = true;
    }

    public virtual void LeftKeep() { }

    public virtual void LeftRelease()
    {
        _leftPressed = false;
    }

    public virtual void UpPress()
    {
        _upPressed = true;
    }

    public virtual void UpRelease()
    {
        _upPressed = false;
    }

    public virtual void DownPress()
    {
        _downPressed = true;
    }

    public virtual void DownRelease()
    {
        _downPressed = false;
    }

    public virtual void OnEvent(LevelEvent levelEvent)
    {
        switch (levelEvent.EventType)
        {
            case LevelEventType.FlightOn:
                _flightModeActive = true;
                break;
            case LevelEventType.FlightOff:
                _flightModeActive = false;
                break;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collidingObject)
    {
        CollisionEnter(collidingObject.collider.gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collidingObject)
    {
        TriggerEnter(collidingObject.gameObject);
    }

    protected virtual void OnCollisionExit2D(Collision2D collidingObject)
    {
        CollisionExit(collidingObject.collider.gameObject);
    }

    protected virtual void OnTriggerExit2D(Collider2D collidingObject)
    {
        TriggerExit(collidingObject.gameObject);
    }

    protected virtual void CollisionEnter(GameObject collidingObject)
    {

    }

    protected virtual void TriggerEnter(GameObject collidingObject)
    {

    }

    protected virtual void CollisionExit(GameObject collidingObject)
    {

    }

    protected virtual void TriggerExit(GameObject collidingObject)
    {

    }

    public virtual void OnEnable()
    {
        this.StartListeningEvent<LevelEvent>();
    }
    public virtual void OnDisable()
    {
        this.StopListeningEvent<LevelEvent>();
    }

    public static RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color, bool drawGizmo = false)
    {
        if (drawGizmo)
        {
            Debug.DrawRay(rayOriginPoint, rayDirection * rayDistance, color);
        }
        return Physics2D.Raycast(rayOriginPoint, rayDirection, rayDistance, mask);
    }

    public static void DrawCross(Vector3 spot, float crossSize, Color color)
    {
        Vector3 tempOrigin = Vector3.zero;
        Vector3 tempDirection = Vector3.zero;
        tempOrigin.x = spot.x - crossSize / 2;
        tempOrigin.y = spot.y - crossSize / 2;
        tempOrigin.z = spot.z;
        tempDirection.x = 1;
        tempDirection.y = 1;
        tempDirection.z = 0;
        Debug.DrawRay(tempOrigin, tempDirection * crossSize, color);

        tempOrigin.x = spot.x - crossSize / 2;
        tempOrigin.y = spot.y + crossSize / 2;
        tempOrigin.z = spot.z;
        tempDirection.x = 1;
        tempDirection.y = -1;
        tempDirection.z = 0;
        Debug.DrawRay(tempOrigin, tempDirection * crossSize, color);
    }

    public static void DrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 0.20f, float arrowHeadAngle = 35.0f)
    {
        Debug.DrawRay(origin, direction, color);

        DrawArrowEnd(false, origin, direction, color, arrowHeadLength, arrowHeadAngle);
    }

    private static void DrawArrowEnd(bool drawGizmos, Vector3 arrowEndPosition, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
    {
        if (direction == Vector3.zero)
        {
            return;
        }
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
        if (drawGizmos)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(arrowEndPosition + direction, right * arrowHeadLength);
            Gizmos.DrawRay(arrowEndPosition + direction, left * arrowHeadLength);
            Gizmos.DrawRay(arrowEndPosition + direction, up * arrowHeadLength);
            Gizmos.DrawRay(arrowEndPosition + direction, down * arrowHeadLength);
        }
        else
        {
            Debug.DrawRay(arrowEndPosition + direction, right * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, left * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, up * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, down * arrowHeadLength, color);
        }
    }

}
