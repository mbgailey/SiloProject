using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour {

    Animator anim;
    CapsuleCollider col;
    public float animSpeed = 1.5f;				// a public setting for overall animator animation speed
    float groundMultiplier = 100f;
    float airMultiplier = 75f;
    float waterMultiplier = 50f;
    float jumpForce = 30f;
    

    float maxspeed = 1f;
    float maxWaterSpeed = 0.75f;
    float rotateSpeed = 360f;
    float waterRotateSpeed = 180f;

    //int direction;
    Rigidbody rb;
    float globalWaterElevation = -2.1f;
    bool grounded = false;
    bool submerged = false;
    bool swimming = false;
    bool jump = false;
    bool direction = true;
    float charHeight = 0.35f;

    public LayerMask terrainLayer;

    private AnimatorStateInfo currentBaseState;			// a reference to the current state of the animator, used for base layer
    private AnimatorStateInfo layer2CurrentState;	// a reference to the current state of the animator, used for layer 2

	void Start ()
	{
		// initialising reference variables
		anim = gameObject.GetComponent<Animator>();					  
		col = gameObject.GetComponent<CapsuleCollider>();
        rb = gameObject.GetComponent<Rigidbody>();
	}

    void FixedUpdate()
    {
        Vector3 globalPos = rb.position;

        anim.speed = animSpeed;								// set the speed of our animator to the public variable 'animSpeed'

        //Check whether player is grounded
        grounded = IsGrounded();
        
        float mult = groundMultiplier;
        if (!grounded)
        {
            mult = groundMultiplier;
        }

        if (!swimming && globalPos.y < globalWaterElevation)
        {
            Debug.Log("SWIMMING");
            swimming = true;
            rb.useGravity = false;
            rb.drag = 10f;
        }
        else if (swimming && globalPos.y > globalWaterElevation)
        {
            Debug.Log("NOT SWIMMING");
            swimming = false;
            rb.useGravity = true;
            rb.drag = 0f;
        }

        //Debug.Log("Grounded: " + grounded);
        //Debug.Log("Swimming: " + swimming);

        if (swimming)
        {
            mult = waterMultiplier;
        }

        float h = Input.GetAxis("Horizontal");				// setup h variable as our horizontal input axis
        float v = Input.GetAxis("Vertical");	// setup v variables as our vertical input axis

        Vector3 moveDir = new Vector3(h, v, 0f).normalized;

        Vector3 currentVel = rb.velocity;

        Vector3 locoForce;

        if (!swimming)
        {
            locoForce = new Vector3(h * mult, 0f, 0f);
        }
        else
        {
            locoForce = new Vector3(h * mult, v * mult, 0f);
        }
        

        //Debug.Log(currentVel);

        rb.AddForce(locoForce, ForceMode.Force);

        if (!swimming && Mathf.Abs(currentVel.x) > maxspeed)
        {
            Vector3 vel = currentVel;

            if (vel.x > 0)
            {
                vel.x = maxspeed;
            }
            else
            {
                vel.x = -maxspeed;
            }
            
            rb.velocity = vel;
        }

        else if (swimming && Mathf.Abs(currentVel.x) > maxWaterSpeed)
        {
            Vector3 vel = currentVel;

            if (vel.x > 0)
            {
                vel.x = maxWaterSpeed;
            }
            else
            {
                vel.x = -maxWaterSpeed;
            }

            rb.velocity = vel;
        }

        Quaternion q;

        if (swimming && (moveDir.sqrMagnitude > 0))
        {
            q = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, waterRotateSpeed * Time.deltaTime);
        }

        else if (h < 0)
        {

            q = Quaternion.LookRotation(-Vector3.right);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, rotateSpeed * Time.deltaTime);
        }
        else
        {
            q = Quaternion.LookRotation(Vector3.right);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, rotateSpeed * Time.deltaTime);
        }

        

        if (Input.GetButtonDown("Jump") && grounded || Input.GetButtonDown("Jump") && swimming)
        {
            anim.SetBool("Jump", true);
            rb.AddForce(Vector3.up * jumpForce + Vector3.right * jumpForce * 0.95f, ForceMode.Impulse);
            jump = true;
        }
        else
            jump = false;

        anim.SetFloat("Speed", Mathf.Abs(currentVel.x));							
        anim.SetBool("Grounded", grounded);
        anim.SetBool("Swimming", swimming);
        anim.SetBool("Jump", jump);					
        //anim.SetFloat("Direction", h); 						// set our animator's float parameter 'Direction' equal to the horizontal input axis		
        //anim.speed = animSpeed;								// set the speed of our animator to the public variable 'animSpeed'
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);	// set our currentState variable to the current state of the Base Layer (0) of animation

        if (anim.layerCount == 2)
            layer2CurrentState = anim.GetCurrentAnimatorStateInfo(1);	// set our layer2CurrentState variable to the current state of the second Layer (1) of animation

        


        // STANDARD JUMPING

        // if we are currently in a state called Locomotion, then allow Jump input (Space) to set the Jump bool parameter in the Animator to true
        //if (currentBaseState.fullPathHash == locoState)
        //{

        //}

    }

    bool IsGrounded()
    {
        //RaycastHit hit;
        //Vector3 p1 = transform.position + charCtrl.center;
        //float distanceToObstacle = 0;

        Ray ray = new Ray(transform.position, -Vector3.up);
        float radius = 0.15f;
        float maxDist = charHeight / 2 + 0.02f - radius;
        Debug.DrawRay(transform.position, -Vector3.up);

        // Cast a sphere wrapping character controller 10 meters forward
        // to see if it is about to hit anything.
        //if (Physics.SphereCast(p1, charCtrl.height / 2, transform.forward, out hit, 10))
        //{
        //    distanceToObstacle = hit.distance;
        //}

        //return Physics.SphereCast(ray, radius, maxDist, terrainLayer);
        return Physics.Raycast(ray, maxDist, terrainLayer);
    }

}
