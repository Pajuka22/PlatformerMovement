using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    //walking
    public Rigidbody2D RB;
    [SerializeField] private float walkSpeed = 10;
    [Range(1, 10)]
    [SerializeField] private float acceleration = 1;//how quickly you accelerate when changing direction or starting movement
    //grounding and jumping
    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private List<float> jumpHeight = new List<float>();//list of heights of jumps. Allows for as many jumps as you want
    private List<float> jumpSpeed = new List<float>();//list of jumpspeeds filled in when the game starts with a program. Guarantees constant jump height
    [SerializeField] private bool variableJump = false;//can the player vary the height of jumps by releasing the jump button?
    [System.NonSerialized] public bool bGrounded;//grounded? public because things might need to check if player is grounded
    private Transform Ground;//
    private Transform Ceiling;//Ground and ceiling checks
    private float GroundRadius = 0.2f;//radius for which it will become grounded
    private int Jumps;//number of jumps the player currently has left.
    //water and gravity
    private int inWater = 0;
    private float initGrav;//initial gravity has to be stored somewhere
    [Range(1, 2)]
    [SerializeField] private float fallingMult = 1;//how much faster you fall after reaching peak height or releasing jump button (if variableJump)
    [Range(0, 1)]
    [SerializeField] private float airFriction = 0;
    [SerializeField] private float waterGravMult = 0.5f;
    [SerializeField] private float waterFriction = 0.5f;
    [SerializeField] private LayerMask WhatIsWater;

    GameObject Obj;

    // Use this for initialization
    void Start()
    {
        jumpSpeed.Clear();
        jumpSpeed.TrimExcess();
        initGrav = RB.gravityScale;
        setJumpSpeed(0, jumpHeight.Count);
        //fill in jumpspeed list with all the values needed.
        Jumps = jumpSpeed.Count;
        //just storing initial values

    }
    public void setJumpSpeed(int first, int last)
    {
        for (int i = first; i < last; i++)
        {
            if (jumpSpeed.Count < i + 1)
            {
                jumpSpeed.Add(Mathf.Sqrt(2 * initGrav * jumpHeight[i]));
            }
        }
    }
    private void Awake()
    {
        Ground = transform.Find("Ground");
        Ceiling = transform.Find("Ceiling");
        Obj = GameObject.Find("/Player/Sprite");
        //find these
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        if(RB.velocity.x == 0 ? false : RB.velocity.x/Obj.transform.right.x > 0)//if it's moving and it's moving in the direction it's facing. should never mess up unless you got some paper mario shit going on
        {
            Obj.transform.Rotate(0, 180, 0);
        }
        bGrounded = false;
        if (Jumps == jumpSpeed.Count)
        {
            Jumps--;
        }
        //basically says that number of jumps is jumpSpeed.Count - 1 so that if the player runs off a platform, they don't get the grounded jump
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Ground.position, GroundRadius, WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != gameObject)
            {
                bGrounded = true;
                Jumps = jumpSpeed.Count;
            }
        }
    }
    public void Move(float speed, bool jump)
    {
        speed *= walkSpeed;
        //speed is horizontal axis, so multiply by speed. Desired speed, not absolute.
        if (speed != 0)
        {
            if (RB.velocity.x > speed)
            {
                speed = RB.velocity.x - acceleration;
            }
            if (RB.velocity.x < speed)
            {
                speed = RB.velocity.x + acceleration;
            }
        }
        else
        {
            if (RB.velocity.x > 0)
            {
                speed = RB.velocity.x > acceleration ? RB.velocity.x - acceleration : 0;
            }
            if (RB.velocity.x < 0)
            {
                speed = Mathf.Abs(RB.velocity.x) > acceleration ? RB.velocity.x + acceleration : 0;
            }
        }
        //acceleration stuff. Makes sure it stops when done accelerating, and accelerates toward desired speed.
        if (!bGrounded && (RB.velocity.y < 0 || (variableJump && !Input.GetButton("Jump"))))
        {
            RB.gravityScale = fallingMult * initGrav * (inWater > 0 ? waterGravMult : 1);
        }
        //if it's in the air and it's either falling or the jump is variable and the player has released jump, fall faster
        else
        {
            RB.gravityScale = initGrav * (inWater > 0 ? waterGravMult : 1);
        }
        //if it's grounded or moving upward or the jump isn't variable or the player hasn't released jump, set gravityScale back to initial value
        RB.velocity = new Vector2(speed, RB.velocity.y);
        //set horizontal velocity to speed.
        if (jump && Jumps > 0)
        {
            RB.velocity = new Vector2(RB.velocity.x, jumpSpeed[jumpSpeed.Count - Jumps]);
            Jumps--;
            //if the player tried to jump and you can jump (Jumps counts jumps left) set the velocity to the jumpsSpeed's value at the jump the player's currently on and lose a jump.
        }
        RB.drag = inWater > 0 ? waterFriction : airFriction;
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject.name);
        if ((WhatIsWater.value & 1 << col.gameObject.layer) == 1 << col.gameObject.layer)
        {
            inWater++;
        }
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if ((WhatIsWater.value & 1 << col.gameObject.layer) == 1 << col.gameObject.layer)
        {
            inWater--;
        }//check if it's leaving the water.
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
    }
    private void OnCollisionExit2D(Collision2D col)
    {
        //Physics2D.IgnoreCollision(col.collider, col.otherCollider, false);
    }
}