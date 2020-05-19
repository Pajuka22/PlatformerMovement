using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    float jump;
    [SerializeField] private int jumpBufferFrames = 3;
    float speed = 0;
    public MovementController controller;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            speed = -1;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            speed = Input.GetKey(KeyCode.D) ? 1 : 0;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            speed = 1;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            speed = Input.GetKey(KeyCode.A) ? -1 : 0;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jump = jumpBufferFrames;
        }
    }
    private void FixedUpdate()
    {
        controller.Move(speed, jump > 0);
        if (controller.bGrounded && jump > 0)
        {
            jump = 0;
        }
        jump--;
    }
}