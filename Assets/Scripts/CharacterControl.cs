using UnityEngine;
using System.Collections;
using Prime31;


public class CharacterControl : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
    public float swimSpeed = 5f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

    float jumpCooldownTime = 0.15f;
    bool jumpEligible = true;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;
    private float normalizedVerticalSpeed = 0;

	private CharacterController3D _controller;
	private Animator _animator;
	private RaycastHit _lastControllerColliderHit;
    [HideInInspector]
	public Vector3 _velocity;

    private bool grounded = false;
    private bool running = false;
    private bool jumping = false;
    private bool swimming = false;
    private bool surfaced = false;
    private int direction = 1;

    float rotateSpeed = 420f;
    float waterRotateSpeed = 180f;

    float zPosLimit = 0.1f;

    //int direction;
    Rigidbody rb;
    float waterElevation;
    float swimmingBuffer = 0.1f;

    TideController tideController;
    PlayerLungs lungsController;

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController3D>();
        rb = gameObject.GetComponent<Rigidbody>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;

        Vector3 globalPos = rb.position;
        tideController = GameObject.FindGameObjectWithTag("GameController").GetComponent<TideController>();
        lungsController = this.GetComponent<PlayerLungs>();
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f )
			return;

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider col )
	{
		//Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider col )
	{
		//Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion

    IEnumerator StartJumpCooldownTimer()
    {
        //Debug.Log("StartTimer");
        yield return new WaitForSeconds(jumpCooldownTime);
        //Debug.Log("Eligible");
        jumpEligible = true;
    }

	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
        Vector3 globalPos = rb.position;
        waterElevation = tideController.globalWaterElevation;

        if (_controller.isGrounded)
        {
            _velocity.y = 0;
            
        }

        if (jumping && (swimming || surfaced || _controller.isGrounded))
        {
            jumping = false;
            StartCoroutine(StartJumpCooldownTimer());
        }

        float h = Input.GetAxis("Horizontal");				// setup h variable as our horizontal input axis
        float v = Input.GetAxis("Vertical");	// setup v variables as our vertical input axis
        bool j = Input.GetButtonDown("Jump");

		if( h > 0f )
		{
			normalizedHorizontalSpeed = 1;
            direction = 1;
			//if( transform.localScale.x < 0f )
				//transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

            if (_controller.isGrounded)
            {
                running = true;
                
            }
                
		}
		else if( h < 0f )
		{
			normalizedHorizontalSpeed = -1;
            direction = -1;
			//if( transform.localScale.x > 0f )
				//transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

            if (_controller.isGrounded)
            {
                running = true;
            }
		}
		else
		{
			normalizedHorizontalSpeed = 0;
            running = false;
            if (_controller.isGrounded)
            {
                grounded = true;
                jumping = false;
            }
		}

        //Apply water physics
        if (!swimming && globalPos.y < waterElevation - swimmingBuffer)
        {
            //Debug.Log("SWIMMING");
            swimming = true;
            jumping = false;
            //lungsController.submerged = true;
        }
        else if (swimming && globalPos.y > waterElevation - swimmingBuffer)
        {
            //Debug.Log("NOT SWIMMING");
            swimming = false;
            jumping = false;
            //lungsController.submerged = false;
        }

        if (globalPos.y > waterElevation - swimmingBuffer * 2f && globalPos.y < waterElevation + swimmingBuffer) //Surfaced
        {
            //Debug.Log("Surfaced");
            //jumpEligible = true;
            surfaced = true;
            //lungsController.submerged = false;
        }
        else
        {
            surfaced = false;
        }

        if (swimming)
        {
            if (!surfaced)
            {
                lungsController.submerged = true;
            }
            else
            {
                lungsController.submerged = false;
            }
        }
        else
        {
            lungsController.submerged = false;
        }


        if (swimming)
        {
            //_velocity.y -= _velocity.y * 0.5f * Time.deltaTime;  //Apply damping to slow any current velocity (ie. from falling)
            _velocity.y *= Mathf.Pow(0.1f, Time.deltaTime);
        }
        if (v > 0f)
        {
            normalizedVerticalSpeed = 1;
        }
        else if (v < 0f)
        {
            normalizedVerticalSpeed = -1;
        }

        else
        {
            normalizedVerticalSpeed = 0;
        }


		//Add some float if jump button is held down while in the air
        if (!_controller.isGrounded && Input.GetButton("Jump") && jumping == true)
        {
            _velocity.y += 0.6f * -gravity * Time.deltaTime;

        }

        // we can only jump whilst grounded or swimming
        if ((_controller.isGrounded && j && jumpEligible) || (surfaced && j && jumpEligible))
		{
			_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
            jumping = true;
            jumpEligible = false;
            running = false;
		}

        

        Vector3 moveDir = new Vector3(h, v, 0f).normalized;

        Quaternion q;

        if (swimming && (moveDir.sqrMagnitude > 0f))
        {
            q = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, waterRotateSpeed * Time.deltaTime);
        }

        else if (h < 0f)
        {

            q = Quaternion.LookRotation(-Vector3.right);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, rotateSpeed * Time.deltaTime);
        }
        else if (h > 0f)
        {
            q = Quaternion.LookRotation(Vector3.right);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, rotateSpeed * Time.deltaTime);
        }


		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
        if (!swimming)
        {
            _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor);
        }
        else
        {
            _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * swimSpeed, Time.deltaTime * smoothedMovementFactor);
        }

		// apply gravity before moving        
        if (!swimming)
        {
            _velocity.y += gravity * Time.deltaTime;    //Apply gravity
        }
        else
        {
            _velocity.y = Mathf.Lerp(_velocity.y, normalizedVerticalSpeed * swimSpeed, Time.deltaTime * smoothedMovementFactor);
        }

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets uf jump down through one way platforms
        //if( _controller.isGrounded && Input.GetKey( KeyCode.DownArrow ) )
        //{
        //    _velocity.y *= 3f;
        //    _controller.ignoreOneWayPlatformsThisFrame = true;
        //}

		_controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;

        _animator.SetFloat("HorizSpeed", Mathf.Abs(_velocity.x));
        _animator.SetBool("Grounded", grounded);
        _animator.SetBool("Running", running);
        _animator.SetBool("Swimming", swimming);
        _animator.SetBool("Jumping", jumping);
        _animator.SetInteger("Direction", direction);
	}

}
