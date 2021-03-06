﻿#define DEVELOPMENT
using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    [Tooltip("The speed at which the player moves horizontally when running")]
    [SerializeField]
    private float _runSpeed;
    [SerializeField]
    [Tooltip("The max height player can jump")]
    private float _jumpHeight;
    [SerializeField]
    [Tooltip("The maximum downwards speed attainable by player when falling")]
    private float _fallSpeedMax;
    [SerializeField]
    [Tooltip("Horizontal distance travelled by player when dodging. You may have to adjust 'dodge speed' after adjusting this.")]
    private float _dodgeDistance;
    [SerializeField]
    [Tooltip("How fast the player should cover the dodge distance")]
    private float _dodgeSpeed;
    [SerializeField]
    [Tooltip("Duration for which player will slide for after stopping or dodging")]
    private float _slidingTime = 1;
    [SerializeField]
    [Tooltip("How far the player will slide for after stopping or dodging")]
    private float _slidingDistance = .4f;
    [SerializeField]
    [Tooltip("Player will be ready for the next action only after dodgeCooldown and sliding time (See 'sliding time') is reached")]
    private float _dodgeCoolDownTime = 1f;
    [SerializeField]
    [Tooltip("Layers representing standable object in game, such as ground")]
    private LayerMask _standableObjectLayerMask;

    private float _slidingDampStrength = 0;
    private Rigidbody2D _rigidBody;
    private float _mass;
    private BoxCollider2D _boxCollider;

    
    private float _dodgeTimeProgress = 0;
    [Tooltip("Gravity used to calculate falling acceleration, velocity etc.")]
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

    public static BoxCollider2D PlayerCollider
    {
        get
        {
            return s_instance._boxCollider;
        }
        set
        {
            s_instance._boxCollider = value;
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
        _slidingDampStrength = -2 * (_slidingDistance - _runSpeed * _slidingTime) / (_slidingTime * _slidingTime);// S=ut+1/2at^2
        float timeOfFlight = _dodgeDistance / _dodgeSpeed;
        float dodgeYVel = -Physics.gravity.y * timeOfFlight;
        _dodgeAnimToFallOverrideDistance = dodgeYVel * timeOfFlight + .5f * Physics.gravity.y * timeOfFlight * timeOfFlight + .01f;


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

    
    private float _dodgeAnimToFallOverrideDistance = 1;
    [SerializeField]
    [Tooltip("If enabled, will play falling animation if falls down when doing a dodge at cliff edge")] 
    private bool _playFallingAnimationWhenJumpingOffCliff = true;
    void Update()
    {
#if DEVELOPMENT
        _slidingDampStrength = -2 * (_slidingDistance - _runSpeed * _slidingTime) / (_slidingTime * _slidingTime);// S=ut+1/2at^2 //recalculating sliding damp strength so that it can be tweaked in the editro at runtime
        Physics2D.gravity = new Vector2(0, -gravityY); //recalculating gravity so that if it's changed during runtime, it's effect can be seen
        float timeOfFlight = _dodgeDistance / _dodgeSpeed;
        float dodgeYVel = -Physics.gravity.y * timeOfFlight;
        _dodgeAnimToFallOverrideDistance = dodgeYVel * timeOfFlight + .5f * Physics.gravity.y * timeOfFlight * timeOfFlight + .01f;
#endif

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
        if(_rigidBody.velocity.y < 0 )
        {
            
            if (!_dodged)
            {
                PlayerAnimationHandler.OnFall();                
                _falling = true;
                _landed = false;
            }
           

            else if(PhysicsHelper.GetObjectToGroundDistance(_boxCollider, _standableObjectLayerMask) > _dodgeAnimToFallOverrideDistance)
            {
                if (_playFallingAnimationWhenJumpingOffCliff) {
                    PlayerAnimationHandler.OnFallDodge();                    
                    _falling = true;
                    _landed = false;
                    _dodged = false;
                    SetVelocityForDampingForDodge();
                }
            }
          
        }
       /* else if(_rigidBody.velocity.y > 0)
        {
            _jumping = true;
        }*/
   
        
            
        if(_dodged)
        {
            _dodgeTimeProgress += Time.deltaTime;
            if (_dodgeTimeProgress >= _dodgeCoolDownTime && _rigidBody.velocity.x ==0 )
            {
                _dodged = false;
                PlayerAnimationHandler.OnDodgeEnd();                
            }
        }

        

    }

    
    
    void FixedUpdate()
    {
        
        if (_prevVelocityY < 0 && _rigidBody.velocity.y >= 0) //player has landed from a jump or fall
        {
            
            OnLanding();
        }
        if(_bDampVelocity && !_falling) //(!_dodged || (_dodged &&
        {
            float velX = _rigidBody.velocity.x;
            if(_dodged && _rigidBody.velocity.y <0 && _landed) //is falling after dodge-landing on an edge
            {
                //EditorApplication.isPaused = true;
                _dodged = false;
                PlayerAnimationHandler.OnDodgeEndLandFall();                
                _falling = true;
                _landed = false;
                
                
                
               // _bDampVelocity = false;
            }
            if(PhysicsHelper.dampFloat(ref velX, _slidingDampStrength))
            {
                if(_dodged)
                {
                    if (_dodgeTimeProgress >= _dodgeCoolDownTime) {
                        _dodged = false;
                        PlayerAnimationHandler.OnDodgeEndDamp();
                        
                    }
                }
                _bDampVelocity = false;
                //PlayerStates.UnSet(PlayerStates.AnimationParameter.Stop);
                PlayerAnimationHandler.OnDamp();
                

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
                    PlayerAnimationHandler.OnRun();
                                      
                }
                PlayerAnimationHandler.OnUnIdle();
                
                // PlayerStates.UnSet(PlayerStates.AnimationParameter.Stop);
            }
            else
            {
                if (_isRunKeyPressed && !_dodged ) //movement key(s)was pressed till now
                {
                    
                    PlayerAnimationHandler.OnNoMoveKeysPressed();
                   
                    if (PhysicsHelper.CollidingWithSomethingOnEitherSide(_boxCollider, _standableObjectLayerMask))
                    {
                        PlayerStates.Set(PlayerStates.AnimationParameter.DirectIdle);
                    }
                    else 
                    {
                        if (!_isCrouching)
                        {
                            PlayerAnimationHandler.OnStop();

                        }
                        SetVelocityForDamping();
                    }
                }
                _isRunKeyPressed = false;
                if(IsPlayerOnGround)
                {
                    PlayerAnimationHandler.OnIdling();            //if on ground and no key is pressed, should be idling
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
        _dodgeTimeProgress = 0;
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

    int jumpTrigsetcount = 0;
    bool _jumping = false;
    private void Jump()
    {
        if (IsPlayerOnGround && _landed)
        {
            PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);            
            PlayerStates.Set(PlayerStates.AnimationParameter.Jump);
            PlayerStates.Set(PlayerStates.AnimationParameter.Jumping);
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

        if (Mathf.Abs(_rigidBody.velocity.x) > 0  )
        {
            //Debug.LogError("setting for damp");
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
        //Debug.LogError("setting vel for damp");
        _bDampVelocity = true;
    }

    
    bool _landed = false;
    int fallUsetCount = 0;
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
      /*  else if (!_isRunKeyPressed)
        {
            SetVelocityForDamping();
        }*/
        _falling = false;
        
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Falling);
        PlayerStates.ResetTrigger(PlayerStates.AnimationParameter.Jump);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Jumping);

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
