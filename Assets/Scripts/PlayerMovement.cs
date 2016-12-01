using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    [SerializeField]
    private float _runSpeed;
    [SerializeField]
    private float _jumpHeight;
    [SerializeField]
    private float _fallSpeedMax;
    [SerializeField]
    private float _dodgeDistance;
    [SerializeField]
    private float _dodgeSpeed;
    /*/// <summary>
    /// The angle at which the small dodge jump will be performed
    /// </summary>
    [SerializeField]
    private float _dodgeAngle;*/
    [SerializeField]
    private LayerMask _standableObjectLayerMask;
    

    private Rigidbody2D _rigidBody;
    private float _mass;
    private BoxCollider2D _boxCollider;

    public float gravityY;

    private enum Direction { Left, Right}
    private Direction _playerFacingDirection;

	// Use this for initialization
	void Start () {
        
        _rigidBody = GetComponent<Rigidbody2D>();
        _mass = _rigidBody.mass;
        _boxCollider = GetComponent<BoxCollider2D>();
        _playerFacingDirection = Direction.Right;           
        
	}

    /// <summary>
    /// If set, will damp rigidbody velocity
    /// </summary>
    bool _bDampVelocity;
	// Update is called once per frame
	
    void Update()
    {
        Physics2D.gravity = new Vector2(0, -gravityY); //recalculating gravity so that if it's changed during runtime, it's effect can be seen

        ProcessKeyPress();
    }

    [SerializeField]
    private float _slidingDampStrength;
    void FixedUpdate()
    {     
        if(_bDampVelocity && !_dodged)
        {
           
            float velX = _rigidBody.velocity.x;
            if(PhysicsHelper.dampFloat(ref velX, _slidingDampStrength))
            {
                _bDampVelocity = false;
                PlayerStates.UnSet(PlayerStates.AnimationParameter.Stop);
                PlayerStates.Set(PlayerStates.AnimationParameter.Idling);

            }
            _rigidBody.velocity = new Vector2(velX, _rigidBody.velocity.y);
        }

         if (_rigidBody.velocity.y < 0) {
            float clampedFallSpeed = Mathf.Clamp(_rigidBody.velocity.y, -_fallSpeedMax, int.MaxValue);
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, clampedFallSpeed); //limit fall speed to the specified one
         }
    }

    private bool _isRunKeyPressed = false;
    /// <summary>
    /// Direction represented by the currently pressed horizontal movement key
    /// </summary>
    Direction _keyDirection = Direction.Left;
    private void ProcessKeyPress()
    {
        
        float axisValue = Input.GetAxisRaw("Horizontal"); //don't need smoothing
        if(axisValue<0) //leftwards movement
        {
            _playerFacingDirection = Direction.Left;
            _keyDirection = Direction.Left;
        }
        else if(axisValue>0)
        {
            _playerFacingDirection = Direction.Right;
            _keyDirection = Direction.Right;
        }
        if (axisValue!=0 || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            _isRunKeyPressed = true;
            PlayerStates.Set(PlayerStates.AnimationParameter.Running);
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Stop);
        }    
        else
        {
            if (_isRunKeyPressed && !_dodged) //movement key(s)was pressed till now
            {
                PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);
                PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
                PlayerStates.Set(PlayerStates.AnimationParameter.Stop);
                int directionMultiplier = 1;
                if(_playerFacingDirection == Direction.Left)
                {
                    directionMultiplier = -1;
                }
                _rigidBody.velocity = new Vector2(_runSpeed * directionMultiplier, _rigidBody.velocity.y); //set rigidbody velocity so that it can be damped
                
                _bDampVelocity = true;
            }
            _isRunKeyPressed = false;
            
        }
        
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.J) && !_dodged)
        {
            Dodge();
        }
        if (_isRunKeyPressed && !_dodged)
        {
            Vector2 runVector = new Vector3(_runSpeed * axisValue, _rigidBody.velocity.y);
            //transform.position += _runVector;
            _rigidBody.velocity = runVector;
        }
    }

    bool _dodged = false;
   // public float _dodgeYVel = 8;
    private void Dodge()
    {

        if(_dodged)
        {
            return;
        }
        PlayerStates.Set(PlayerStates.AnimationParameter.Dodging);
        

        _dodged = true;
        if(_isRunKeyPressed) //a key is pressed
        {
            _playerFacingDirection = _playerFacingDirection == Direction.Left 
                ? Direction.Right : Direction.Left; //flip the direction before dodge(and then dodge in the opposite of the flipped direction)        
        }
        int horizontalVelocityDirection = 1;
        if(_playerFacingDirection==Direction.Right)
        {
            horizontalVelocityDirection = -1; //player should dodge left
        }
        float timeOfFlight = _dodgeDistance / _dodgeSpeed;
        float dodgeYVel = -Physics.gravity.y * timeOfFlight;        
        _rigidBody.velocity = new Vector2(_dodgeSpeed * horizontalVelocityDirection, dodgeYVel);//.Set(dodgeHVel, _dodgeYVel);
    }
    private void Jump()
    {
        if (IsPlayerOnGround)
        {
            Vector2 jumpVelocityVector = new Vector2(0, PhysicsHelper.GetVelocityToReachHeight(_jumpHeight));
            _rigidBody.velocity = jumpVelocityVector;
        }
    }

    private bool IsPlayerOnGround
    {
        get
        {
            return Physics2D.Raycast(_boxCollider.bounds.center, Vector2.down, _boxCollider.bounds.extents.y + .05f, _standableObjectLayerMask);
           
        }
    }
    

    protected void OnCollisionEnter2D(Collision2D col)
    {
        if(_dodged)
        {
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Dodging);
            PlayerStates.Set(PlayerStates.AnimationParameter.Idling);
            _dodged = false;
            int directionMultiplier = (int)Mathf.Sign(_rigidBody.velocity.x);
            if(directionMultiplier == 0)
            {
                directionMultiplier = 1;
            }
            _rigidBody.velocity = new Vector2(_runSpeed * directionMultiplier, _rigidBody.velocity.y); //setting runspeed as x component so that a consistent sliding distance can be acheived after dodging and movement halts
            _bDampVelocity = true;

        }
    }

    
}
