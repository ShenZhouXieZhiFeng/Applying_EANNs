/// Author: Samuel Arzt
/// Date: March 2017

#region Includes
using System;
using UnityEngine;
using System.Collections.Generic;
#endregion

/// <summary>
/// Singleton class managing the current track and all cars racing on it, evaluating each individual.
/// </summary>
public class TrackManager : MonoBehaviour
{
    #region Members
    public static TrackManager Instance
    {
        get;
        private set;
    }

    // Sprites for visualising best and second best cars. To be set in Unity Editor.
    [SerializeField]
    private Sprite BestCarSprite;
    [SerializeField]
    private Sprite SecondBestSprite;
    [SerializeField]
    private Sprite NormalCarSprite;

    private Checkpoint[] checkpoints;

    /// <summary>
    /// Car used to create new cars and to set start position.
    /// </summary>
    public CarController PrototypeCar;
    // Start position for cars
    private Vector3 startPosition;
    private Quaternion startRotation;

    // Struct for storing the current cars and their position on the track.
    /// <summary>
    /// 用于存储当前车辆及其位置的结构体
    /// </summary>
    private class RaceCar
    {
        public RaceCar(CarController car = null, uint checkpointIndex = 1)
        {
            this.Car = car;
            this.CheckpointIndex = checkpointIndex;
        }
        public CarController Car;
        /// <summary>
        /// 到达的检查点的个数，用来评估这辆车的主要参数
        /// </summary>
        public uint CheckpointIndex;
    }
    private List<RaceCar> cars = new List<RaceCar>();

    /// <summary>
    /// The amount of cars currently on the track.
    /// </summary>
    public int CarCount
    {
        get { return cars.Count; }
    }

    #region Best and Second best
    private CarController bestCar = null;
    /// <summary>
    /// The current best car (furthest in the track).
    /// </summary>
    public CarController BestCar
    {
        get { return bestCar; }
        private set
        {
            if (bestCar != value)
            {
                //Update appearance
                if (BestCar != null)
                    BestCar.SpriteRenderer.sprite = NormalCarSprite;
                if (value != null)
                    value.SpriteRenderer.sprite = BestCarSprite;

                //Set previous best to be second best now
                CarController previousBest = bestCar;
                bestCar = value;
                if (BestCarChanged != null)
                    BestCarChanged(bestCar);

                SecondBestCar = previousBest;
            }
        }
    }
    /// <summary>
    /// Event for when the best car has changed.
    /// </summary>
    public event System.Action<CarController> BestCarChanged;

    private CarController secondBestCar = null;
    /// <summary>
    /// The current second best car (furthest in the track).
    /// </summary>
    public CarController SecondBestCar
    {
        get { return secondBestCar; }
        private set
        {
            if (SecondBestCar != value)
            {
                //Update appearance of car
                if (SecondBestCar != null && SecondBestCar != BestCar)
                    SecondBestCar.SpriteRenderer.sprite = NormalCarSprite;
                if (value != null)
                    value.SpriteRenderer.sprite = SecondBestSprite;

                secondBestCar = value;
                if (SecondBestCarChanged != null)
                    SecondBestCarChanged(SecondBestCar);
            }
        }
    }
    /// <summary>
    /// Event for when the second best car has changed.
    /// </summary>
    public event System.Action<CarController> SecondBestCarChanged;
    #endregion

    

    /// <summary>
    /// The length of the current track in Unity units (accumulated distance between successive checkpoints).
    /// </summary>
    public float TrackLength
    {
        get;
        private set;
    }
    #endregion

    #region Constructors
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Mulitple instance of TrackManager are not allowed in one Scene.");
            return;
        }

        Instance = this;

        //Get all checkpoints
        checkpoints = GetComponentsInChildren<Checkpoint>();

        //Set start position and hide prototype
        startPosition = PrototypeCar.transform.position;
        startRotation = PrototypeCar.transform.rotation;
        PrototypeCar.gameObject.SetActive(false);

        CalculateCheckpointPercentages();
    }

    void Start()
    {
        //Hide checkpoints
        foreach (Checkpoint check in checkpoints)
            check.IsVisible = false;
    }
    #endregion

    #region Methods
    // Unity method for updating the simulation
    void Update()
    {
        //Update reward for each enabled car on the track
        //更新每一辆车的参数
        for (int i = 0; i < cars.Count; i++)
        {
            RaceCar car = cars[i];
            if (car.Car.enabled)
            {
                //设置对这辆车的评估值
                car.Car.CurrentCompletionReward = GetCompletePerc(car.Car, ref car.CheckpointIndex);

                //Update best
                //根据车辆评估更新最优车辆
                if (BestCar == null || car.Car.CurrentCompletionReward >= BestCar.CurrentCompletionReward)
                    BestCar = car.Car;
                //更新第二名车辆
                else if (SecondBestCar == null || car.Car.CurrentCompletionReward >= SecondBestCar.CurrentCompletionReward)
                    SecondBestCar = car.Car;
            }
        }
    }

    public void SetCarAmount(int amount)
    {
        //Check arguments
        if (amount < 0) throw new ArgumentException("Amount may not be less than zero.");

        if (amount == CarCount) return;

        if (amount > cars.Count)
        {
            //Add new cars
            for (int toBeAdded = amount - cars.Count; toBeAdded > 0; toBeAdded--)
            {
                GameObject carCopy = Instantiate(PrototypeCar.gameObject);
                carCopy.transform.position = startPosition;
                carCopy.transform.rotation = startRotation;
                CarController controllerCopy = carCopy.GetComponent<CarController>();
                cars.Add(new RaceCar(controllerCopy, 1));
                carCopy.SetActive(true);
            }
        }
        else if (amount < cars.Count)
        {
            //Remove existing cars
            for (int toBeRemoved = cars.Count - amount; toBeRemoved > 0; toBeRemoved--)
            {
                RaceCar last = cars[cars.Count - 1];
                cars.RemoveAt(cars.Count - 1);

                Destroy(last.Car.gameObject);
            }
        }
    }

    /// <summary>
    /// 重新启动所有的汽车，并把它们放在轨道上。
    /// Restarts all cars and puts them at the track start.
    /// </summary>
    public void Restart()
    {
        //重置所有车辆
        foreach (RaceCar car in cars)
        {
            car.Car.transform.position = startPosition;
            car.Car.transform.rotation = startRotation;
            car.Car.Restart();
            car.CheckpointIndex = 1;
        }
        BestCar = null;
        SecondBestCar = null;
    }

    /// <summary>
    /// Returns an Enumerator for iterator through all cars currently on the track.
    /// </summary>
    public IEnumerator<CarController> GetCarEnumerator()
    {
        for (int i = 0; i < cars.Count; i++)
            yield return cars[i].Car;
    }

    /// <summary>
    /// Calculates the percentage of the complete track a checkpoint accounts for. This method will
    /// also refresh the <see cref="TrackLength"/> property.
    /// </summary>
    private void CalculateCheckpointPercentages()
    {
        checkpoints[0].AccumulatedDistance = 0; //First checkpoint is start
        //Iterate over remaining checkpoints and set distance to previous and accumulated track distance.
        for (int i = 1; i < checkpoints.Length; i++)
        {
            checkpoints[i].DistanceToPrevious = Vector2.Distance(checkpoints[i].transform.position, checkpoints[i - 1].transform.position);
            checkpoints[i].AccumulatedDistance = checkpoints[i - 1].AccumulatedDistance + checkpoints[i].DistanceToPrevious;
        }

        //Set track length to accumulated distance of last checkpoint
        TrackLength = checkpoints[checkpoints.Length - 1].AccumulatedDistance;
        
        //Calculate reward value for each checkpoint
        for (int i = 1; i < checkpoints.Length; i++)
        {
            checkpoints[i].RewardValue = (checkpoints[i].AccumulatedDistance / TrackLength) - checkpoints[i-1].AccumulatedReward;
            checkpoints[i].AccumulatedReward = checkpoints[i - 1].AccumulatedReward + checkpoints[i].RewardValue;
        }
    }

    // Calculates the completion percentage of given car with given completed last checkpoint.
    // This method will update the given checkpoint index accordingly to the current position.
    /// <summary>
    /// 计算给定车辆的完成百分比，并完成最后一个检查点。
    /// 该方法将根据当前位置更新给定的检查点索引。
    /// </summary>
    /// <param name="car"></param>
    /// <param name="curCheckpointIndex"></param>
    /// <returns></returns>
    private float GetCompletePerc(CarController car, ref uint curCheckpointIndex)
    {
        //Already all checkpoints captured
        //如果通过了所有的检查点
        if (curCheckpointIndex >= checkpoints.Length)
            return 1;

        //Calculate distance to next checkpoint
        //计算到下一个检查点的距离
        float checkPointDistance = Vector2.Distance(car.transform.position, checkpoints[curCheckpointIndex].transform.position);

        //Check if checkpoint can be captured
        //判断该检查点是否为可通过
        if (checkPointDistance <= checkpoints[curCheckpointIndex].CaptureRadius)
        {
            //当前检查点+1
            curCheckpointIndex++;
            //通过车辆检查点被更新，清空时间计时
            car.CheckpointCaptured(); //Inform car that it captured a checkpoint
            //计算总的完成情况
            return GetCompletePerc(car, ref curCheckpointIndex); //Recursively check next checkpoint
        }
        else
        {
            //Return accumulated reward of last checkpoint + reward of distance to next checkpoint
            //返回最后一个检查点的累积奖励和距离到下一个检查点的奖励
            return checkpoints[curCheckpointIndex - 1].AccumulatedReward + checkpoints[curCheckpointIndex].GetRewardValue(checkPointDistance);
        }
    }
    #endregion

}
