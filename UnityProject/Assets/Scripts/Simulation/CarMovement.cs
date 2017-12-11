/// Author: Samuel Arzt
/// Date: March 2017

#region Includes
using UnityEngine;
using System.Collections;
#endregion

/// <summary>
/// Component for car movement and collision detection.
/// </summary>
public class CarMovement : MonoBehaviour
{
    #region Members
    /// <summary>
    /// Event for when the car hit a wall.
    /// </summary>
    public event System.Action HitWall;

    //Movement constants
    private const float MAX_VEL = 20f;
    private const float ACCELERATION = 8f;
    private const float VEL_FRICT = 2f;
    private const float TURN_SPEED = 100;

    private CarController controller;

    /// <summary>
    /// The current velocity of the car.
    /// </summary>
    public float Velocity
    {
        get;
        private set;
    }

    /// <summary>
    /// The current rotation of the car.
    /// </summary>
    public Quaternion Rotation
    {
        get;
        private set;
    }

    private double horizontalInput, verticalInput; //Horizontal = engine force, Vertical = turning force
    /// <summary>
    /// The current inputs for turning and engine force in this order.
    /// </summary>
    public double[] CurrentInputs
    {
        get { return new double[] { horizontalInput, verticalInput }; }
    }
    #endregion

    #region Constructors
    void Start()
    {
        controller = GetComponent<CarController>();
    }
    #endregion

    #region Methods
    // Unity method for physics updates
	void FixedUpdate ()
    {
        //Get user input if controller tells us to
        if (controller != null && controller.UseUserInput)
            CheckInput();

        //应用当前设置的输入,设置车辆的速度和旋转值
        ApplyInput();
        //应用车辆速度值和旋转值，设置到车辆上
        ApplyVelocity();
        //应用摩擦力
        ApplyFriction();
	}

    // Checks for user input
    private void CheckInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    // Applies the currently set input
    //应用当前设置的输入
    private void ApplyInput()
    {
        //Cap input 裁剪输入参数到合法值 
        if (verticalInput > 1)
            verticalInput = 1;
        else if (verticalInput < -1)
            verticalInput = -1;

        if (horizontalInput > 1)
            horizontalInput = 1;
        else if (horizontalInput < -1)
            horizontalInput = -1;

        //Car can only accelerate further if velocity is lower than engineForce * MAX_VEL
        //如果速度低于工程量，汽车只能进一步加速
        bool canAccelerate = false;
        if (verticalInput < 0)
            canAccelerate = Velocity > verticalInput * MAX_VEL;
        else if (verticalInput > 0)
            canAccelerate = Velocity < verticalInput * MAX_VEL;
        
        //Set velocity
        //设置速度参数，控制车辆最大速度
        if (canAccelerate)
        {
            Velocity += (float)verticalInput * ACCELERATION * Time.deltaTime;

            //Cap velocity
            if (Velocity > MAX_VEL)
                Velocity = MAX_VEL;
            else if (Velocity < -MAX_VEL)
                Velocity = -MAX_VEL;
        }
        
        //Set rotation
        //设置旋转
        Rotation = transform.rotation;
        Rotation *= Quaternion.AngleAxis((float)-horizontalInput * TURN_SPEED * Time.deltaTime, new Vector3(0, 0, 1));
    }

    /// <summary>
    /// Sets the engine and turning input according to the given values.
    /// </summary>
    /// <param name="input">The inputs for turning and engine force in this order.</param>
    public void SetInputs(double[] input)
    {
        //水平输入，控制旋转
        horizontalInput = input[0];
        //垂直输入，控制速度
        verticalInput = input[1];
    }

    // Applies the current velocity to the position of the car.
    //将当前的速度应用到汽车的位置。
    private void ApplyVelocity()
    {
        Vector3 direction = new Vector3(0, 1, 0);
        transform.rotation = Rotation;
        direction = Rotation * direction;

        this.transform.position += direction * Velocity * Time.deltaTime;
    }

    // Applies some friction to velocity
    private void ApplyFriction()
    {
        //如果输入的车速为0，并且当前的速度大于0，需要将速度减去设定的摩擦力
        //最终让车速稳定在0
        if (verticalInput == 0)
        {
            if (Velocity > 0)
            {
                Velocity -= VEL_FRICT * Time.deltaTime;
                if (Velocity < 0)
                    Velocity = 0;
            }
            else if (Velocity < 0)
            {
                Velocity += VEL_FRICT * Time.deltaTime;
                if (Velocity > 0)
                    Velocity = 0;            
            }
        }
    }

    // Unity method, triggered when collision was detected.
    void OnCollisionEnter2D()
    {
        if (HitWall != null)
            HitWall();
    }

    /// <summary>
    /// Stops all current movement of the car.
    /// </summary>
    public void Stop()
    {
        Velocity = 0;
        Rotation = Quaternion.AngleAxis(0, new Vector3(0, 0, 1));
    }
    #endregion
}
