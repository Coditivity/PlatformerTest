using UnityEngine;
using System.Collections;
using UnityEditor;

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
    [SerializeField]
    private LayerMask _standableObjectLayerMask;
    

    private Rigidbody2D _rigidBody;
    private float _mass;
    private BoxCollider2D _boxCollider;

    public float gravityY;

    private enum Direction { Left, Right}
    private Direction _playerFacingDirection;

    SpriteRenderer _spriteRenderer;

    public static Vector3 PlayerPosition {
        get
        {
            return s_instance.transform.position;
        }
    }

    private static PlayerMovement s_instance;
	// Use this for initialization
	void Start () {

        s_instance = this;
        _rigidBody = GetComponent<Rigidbody2D>();
        _mass = _rigidBody.mass;
        _boxCollider = GetComponent<BoxCollider2D>();
        _playerFacingDirection = Direction.Right;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
	}

    void OnDestroy()
    {
        s_instance = null;
    }

    /// <summary>
    /// If set, will damp rigidbody velocity
    /// </summary>
    bool _bDampVelocity;
    private bool _falling = false;
    private float _prevVelocityY = 0;

    void Update()
    {
        Physics2D.gravity = new Vector2(0, -gravityY); //recalculating gravity so that if it's changed during runtime, it's effect can be seen

        ProcessKeyPress();
        if(_playerFacingDirection == Direction.Left)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * -1;
            transform.localScale = scale;
        }
        else{
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        if(_rigidBody.velocity.y < 0 && !_dodged)
        {
            PlayerStates.Set(PlayerStates.AnimationParameter.Falling);
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
            _falling = true;
            _landed = false;
        }
        
            
        

    }

    
    [SerializeField]
    private float _slidingDampStrength = 0;
    void FixedUpdate()
    {
        
        if (_prevVelocityY < 0 && _rigidBody.velocity.y == 0) //player has landed from a jump or fall
        {
            
            OnLanding();
        }
        if(_bDampVelocity && !_falling) //(!_dodged || (_dodged &&
        {
            float velX = _rigidBody.velocity.x;
            if(_dodged && _rigidBody.velocity.y <0 && _landed) //is falling after dodge-landing on an edge
            {
                //EditorApplication.isPaused = true;
               // Debug.LogError("yup");
                _dodged = false;
                PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgingInAir);
                PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgeLanding);
                PlayerStates.Set(PlayerStates.AnimationParameter.Falling);
                _falling = true;
                _landed = false;
                
                
               // _bDampVelocity = false;
            }
            if(PhysicsHelper.dampFloat(ref velX, _slidingDampStrength))
            {
                if(_dodged)
                {
                    
                    _dodged = false;
                    PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgeLanding);
                    PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgingInAir);
                }
                _bDampVelocity = false;
                //PlayerStates.UnSet(PlayerStates.AnimationParameter.Stop);
                PlayerStates.Set(PlayerStates.AnimationParameter.Idling);

            }
            _rigidBody.velocity = new Vector2(velX, _rigidBody.velocity.y);
        }

         if (_rigidBody.velocity.y < 0) {
            float clampedFallSpeed = Mathf.Clamp(_rigidBody.velocity.y, -_fallSpeedMax, int.MaxValue);
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, clampedFallSpeed); //limit fall speed to the specified one
         }
        _prevVelocityY = _rigidBody.velocity.y;
    }

    private bool _isRunKeyPressed = false;
    private bool _isCrouching = false;
    /// <summary>
    /// Direction represented by the currently pressed horizontal movement key
    /// </summary>
    Direction _keyDirection = Direction.Left;

    float _axisValueY = 0;
    float _axisValueX = 0;
    private float _prevAxisValueX = 0;
    private float _prevAxisValueY = 0;
    private void ProcessKeyPress()
    {
        _prevAxisValueX = _axisValueX;
        _prevAxisValueY = _axisValueY;
        _axisValueX = Input.GetAxisRaw("Horizontal"); //don't need smoothing
        _axisValueY = Input.GetAxisRaw("Vertical");
        if (!_dodged )
        {
            if (_axisValueX < 0) //leftwards movement
            {
                _playerFacingDirection = Direction.Left;
                _keyDirection = Direction.Left;
            }
            else if (_axisValueX > 0)
            {
                _playerFacingDirection = Direction.Right;
                _keyDirection = Direction.Right;
            }
            if (_axisValueX != 0 && _axisValueY>=0)
            {
                _isRunKeyPressed = true;
                if (IsPlayerOnGround && !_isCrouching)
                {
                    PlayerStates.Set(PlayerStates.AnimationParameter.Running);                    
                }
                PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
                // PlayerStates.UnSet(PlayerStates.AnimationParameter.Stop);
            }
            else
            {
                if (_isRunKeyPressed && !_dodged ) //movement key(s)was pressed till now
                {
                    if (!_isCrouching)
                    {
                        PlayerStates.Set(PlayerStates.AnimationParameter.Stop);
                    }
                    PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);
                    PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
                    /*int directionMultiplier = 1;
                    if (_playerFacingDirection == Direction.Left)
                    {
                        directionMultiplier = -1;
                    }

                    if (Mathf.Abs(_rigidBody.velocity.x) > 0)
                    {
                        _rigidBody.velocity = new Vector2(_runSpeed * directionMultiplier, _rigidBody.velocity.y); //set rigidbody velocity so that it can be damped
                        _bDampVelocity = true;
                    }*/
                    SetVelocityForDamping();
                }
                _isRunKeyPressed = false;
                if(IsPlayerOnGround)
                {
                    PlayerStates.Set(PlayerStates.AnimationParameter.Idling); //if on ground and no key is pressed, should be idling
                }
                

            }


            if (_axisValueY>0 && IsKeyShiftVertical) //don't jump if jump was held
            {
                Jump();
            }
        }
        if (Input.GetKeyDown(KeyCode.J) && !_dodged)
        {
            Dodge();
        }
        if(_axisValueY<0)
        {
           SetCrouch(true);
        }
        else
        {
            SetCrouch(false);
        }
        
        

        if (_isRunKeyPressed && !_dodged && !_isCrouching)
        {
            Vector2 runVector = new Vector3(_runSpeed * _axisValueX, _rigidBody.velocity.y);
            //transform.position += _runVector; 
            _rigidBody.velocity = runVector;
        }
    }

    private void SetCrouch(bool crouch)
    {
        if(!IsPlayerOnGround)
        {
            _isCrouching = false; //can't crouch when not on ground
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Crouching);
            return; 
        }
        if (crouch)
        {
            _isCrouching = true;            
            PlayerStates.Set(PlayerStates.AnimationParameter.Crouching);
        }
        else 
        {
            _isCrouching = false;
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Crouching);
        }
             
    }

    bool _dodged = false;
   // public float _dodgeYVel = 8;
    private void Dodge()
    {

        if(_dodged || !IsPlayerOnGround)
        {
            return;
        }
        _landed = false;
        PlayerStates.Set(PlayerStates.AnimationParameter.DodgingInAir);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
        PlayerStates.Set(PlayerStates.AnimationParameter.Dodge);

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
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);            
            PlayerStates.Set(PlayerStates.AnimationParameter.Jump);
            Vector2 jumpVelocityVector = new Vector2(0, PhysicsHelper.GetVelocityToReachHeight(_jumpHeight));
            _rigidBody.velocity = jumpVelocityVector;
        }
    }

    private bool IsPlayerOnGround
    {
        get
        {
            //return Physics2D.Raycast(_boxCollider.bounds.center, Vector2.down, _boxCollider.bounds.extents.y + .05f, _standableObjectLayerMask);
            return _rigidBody.velocity.y == 0;
           
        }
    }

    private void SetVelocityForDamping()
    {
        int directionMultiplier = 1;
        if (_playerFacingDirection == Direction.Left)
        {
            directionMultiplier = -1;
        }

        if (Mathf.Abs(_rigidBody.velocity.x) > 0)
        {
            _rigidBody.velocity = new Vector2(_runSpeed * directionMultiplier, _rigidBody.velocity.y); //set rigidbody velocity so that it can be damped
            _bDampVelocity = true;
        }
    }

    private void SetVelocityForDampingForDodge()
    {
        /*int directionMultiplier = (int)Mathf.Sign(_rigidBody.velocity.x);
        if (directionMultiplier == 0)
        {
            directionMultiplier = 1;
        }*/
        int directionMultiplier = -1;
        if (_playerFacingDirection == Direction.Left)
        {
            directionMultiplier = 1;
        }
        _rigidBody.velocity = new Vector2(_runSpeed * directionMultiplier, _rigidBody.velocity.y); //setting runspeed as x component so that a consistent sliding distance can be acheived after dodging and movement halts
        _bDampVelocity = true;
    }

    
    bool _landed = false;
    private void OnLanding()
    {
        _landed = true;
        if (_dodged)
        {
            PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgingInAir);
            PlayerStates.Set(PlayerStates.AnimationParameter.DodgeLanding);
            //_bDampDodge = true;            
            SetVelocityForDampingForDodge();
        }
        _falling = false;
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Falling);
        PlayerStates.ResetTrigger(PlayerStates.AnimationParameter.Jump);
    }
    private const string TagGround = "Ground";
    //private bool _bDampDodge = false;
    protected void OnCollisionEnter2D(Collision2D col)
    {
                

    }


    /// <summary>
    /// Check if the vertical key was down or up
    /// </summary>
    private bool IsKeyShiftVertical
    {
        get {
            return _prevAxisValueY != _axisValueY;
        }
    }
    /// <summary>
    /// Check if the horizontal key was down or up
    /// </summary>
    private bool IsKeyShiftHorizontal
    {
        get
        {
            return _prevAxisValueX != _axisValueX;
        }
    }
}
