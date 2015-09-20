using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour {

    Animator anim;
    CapsuleCollider col;
    float groundMultiplier = 100f;
    float airMultiplier = 0.75f;
    float jumpForce = 20f;

    float maxspeed = 1f;
    int direction;
    Rigidbody rb;
    bool grounded = true;

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
        float mult = groundMultiplier;
        if (!grounded)
        {
            mult = groundMultiplier;
        }

        float h = Input.GetAxis("Horizontal") * mult;				// setup h variable as our horizontal input axis
        float v = Input.GetAxis("Vertical");	// setup v variables as our vertical input axis

        Vector3 currentVel = rb.velocity;
        Vector3 locoForce = new Vector3(h, 0f, 0f);

        //Debug.Log(currentVel);

        rb.AddForce(locoForce, ForceMode.Force);

        if (Mathf.Abs(currentVel.x) > maxspeed)
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

        direction = 1;
        if (h < 0)
        {
            h = -h;
            direction = -1;
        }

        if (Input.GetButtonDown("Jump"))
        {
            anim.SetBool("Jump", true);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        anim.SetFloat("Speed", Mathf.Abs(currentVel.x));							// set our animator's float parameter 'Speed' equal to the vertical input axis				
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
}
