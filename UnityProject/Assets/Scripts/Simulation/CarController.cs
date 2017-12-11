/// Author: Samuel Arzt
/// Date: March 2017

#region Includes
using UnityEngine;
#endregion

/// <summary>
/// Class representing a controlling container for a 2D physical simulation
/// of a car with 5 front facing sensors, detecting the distance to obstacles.
/// </summary>
public class CarController : MonoBehaviour
{
    #region Members
    #region IDGenerator
    // Used for unique ID generation
    private static int idGenerator = 0;
    /// <summary>
    /// Returns the next unique id in the sequence.
    /// </summary>
    private static int NextID
    {
        get { return idGenerator++; }
    }
    #endregion

    // Maximum delay in seconds between the collection of two checkpoints until this car dies.
    /// <summary>
    /// 在此车死亡之前，两个检查点之间的最大延迟时间。
    /// </summary>
    private const float MAX_CHECKPOINT_DELAY = 7;

    /// <summary>
    /// 这款车的潜在人工智能
    /// The underlying AI agent of this car.
    /// </summary>
    public Agent Agent
    {
        get;
        set;
    }

    public float CurrentCompletionReward
    {
        get { return Agent.Genotype.Evaluation; }
        set { Agent.Genotype.Evaluation = value; }
    }

    /// <summary>
    /// Whether this car is controllable by user input (keyboard).
    /// </summary>
    public bool UseUserInput = false;

    /// <summary>
    /// The movement component of this car.
    /// </summary>
    public CarMovement Movement
    {
        get;
        private set;
    }

    /// <summary>
    /// The current inputs for controlling the CarMovement component.
    /// </summary>
    public double[] CurrentControlInputs
    {
        get { return Movement.CurrentInputs; }
    }

    /// <summary>
    /// The cached SpriteRenderer of this car.
    /// </summary>
    public SpriteRenderer SpriteRenderer
    {
        get;
        private set;
    }

    private Sensor[] sensors;
    /// <summary>
    /// 距离上一次检查点的时间
    /// </summary>
    private float timeSinceLastCheckpoint;
    #endregion

    #region Constructors
    void Awake()
    {
        //Cache components
        Movement = GetComponent<CarMovement>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        sensors = GetComponentsInChildren<Sensor>();
    }
    void Start()
    {
        Movement.HitWall += Die;

        //Set name to be unique
        this.name = "Car (" + NextID + ")";
    }
    #endregion

    #region Methods
    /// <summary>
    /// Restarts this car, making it movable again.
    /// </summary>
    public void Restart()
    {
        Movement.enabled = true;
        timeSinceLastCheckpoint = 0;
        //显示所有的传感器
        foreach (Sensor s in sensors)
            s.Show();
        //重置智能
        Agent.Reset();
        //启用
        this.enabled = true;
    }

    // Unity method for normal update
    void Update()
    {
        timeSinceLastCheckpoint += Time.deltaTime;
    }

    // Unity method for physics update
    void FixedUpdate()
    {
        //Get control inputs from Agent
        //从智能从获取控制输入
        if (!UseUserInput)
        {
            //Get readings from sensors
            //获取传感器的参数,5个传感器的长度
            double[] sensorOutput = new double[sensors.Length];
            for (int i = 0; i < sensors.Length; i++)
                sensorOutput[i] = sensors[i].Output;
            //神经网络处理，输出新的控制
            double[] controlInputs = Agent.FNN.ProcessInputs(sensorOutput);
            //车辆制动
            Movement.SetInputs(controlInputs);
        }
        //如果在一定时间内没有再更新检查点，则设置车辆死亡
        if (timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
        {
            Die();
        }
    }

    // Makes this car die (making it unmovable and stops the Agent from calculating the controls for the car).
    private void Die()
    {
        //禁用该车
        this.enabled = false;
        //停止移动
        Movement.Stop();
        //禁用移动组件
        Movement.enabled = false;
        //隐藏所有的传感器
        foreach (Sensor s in sensors)
            s.Hide();
        //通知智能停止
        Agent.Kill();
    }

    public void CheckpointCaptured()
    {
        timeSinceLastCheckpoint = 0;
    }
    #endregion
}
