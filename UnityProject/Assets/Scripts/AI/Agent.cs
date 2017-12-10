/// Author: Samuel Arzt
/// Date: March 2017

#region Includes
using System;
using System.Collections.Generic;
#endregion

/// <summary>
/// 结合基因型和前馈神经网络(FNN)的类。
/// Class that combines a genotype and a feedforward neural network (FNN).
/// </summary>
public class Agent : IComparable<Agent>
{
    #region Members
    /// <summary>
    /// The underlying genotype of this agent.
    /// 这个代理的潜在基因型。
    /// </summary>
    public Genotype Genotype
    {
        get;
        private set;
    }

    /// <summary>
    /// The feedforward neural network which was constructed from the genotype of this agent.
    /// 由该代理的基因型构成的前馈神经网络。
    /// </summary>
    public NeuralNetwork FNN
    {
        get;
        private set;
    }

    private bool isAlive = false;
    /// <summary>
    /// Whether this agent is currently alive (actively participating in the simulation).
    /// 这个代理是否现在还活着(积极地参与到模拟中)。
    /// </summary>
    public bool IsAlive
    {
        get { return isAlive; }
        private set
        {
            if (isAlive != value)
            {
                isAlive = value;

                if (!isAlive && AgentDied != null)
                    AgentDied(this);
            }
        }
    }
    /// <summary>
    /// Event for when the agent died (stopped participating in the simulation).
    /// 当代理死亡时发生的事件(停止参与模拟)。
    /// </summary>
    public event Action<Agent> AgentDied;
    #endregion

    #region Constructors
    /// <summary>
    /// 从给定的基因型中启动一个新的代理，从基因型的参数构造一个新的喂禽神经网络。
    /// Initialises a new agent from given genotype, constructing a new feedfoward neural network from
    /// the parameters of the genotype.
    /// </summary>
    /// <param name="genotype">The genotpye to initialise this agent from.</param>
    /// <param name="topology">The topology of the feedforward neural network to be constructed from given genotype.</param>
    public Agent(Genotype genotype, NeuralLayer.ActivationFunction defaultActivation, params uint[] topology)
    {
        IsAlive = false;
        this.Genotype = genotype;
        FNN = new NeuralNetwork(topology);
        foreach (NeuralLayer layer in FNN.Layers)
            layer.NeuronActivationFunction = defaultActivation;

        //Check if topology is valid
        if (FNN.WeightCount != genotype.ParameterCount)
            throw new ArgumentException("The given genotype's parameter count must match the neural network topology's weight count.");

        //Construct FNN from genotype
        IEnumerator<float> parameters = genotype.GetEnumerator();
        foreach (NeuralLayer layer in FNN.Layers) //Loop over all layers
        {
            for (int i = 0; i < layer.Weights.GetLength(0); i++) //Loop over all nodes of current layer
            {
                for (int j = 0; j < layer.Weights.GetLength(1); j++) //Loop over all nodes of next layer
                {
                    layer.Weights[i,j] = parameters.Current;
                    parameters.MoveNext();
                }
            }
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Resets this agent to be alive again.
    /// </summary>
    public void Reset()
    {
        Genotype.Evaluation = 0;
        Genotype.Fitness = 0;
        IsAlive = true;
    }

    /// <summary>
    /// Kills this agent (sets IsAlive to false).
    /// </summary>
    public void Kill()
    {
        IsAlive = false;
    }

    #region IComparable
    /// <summary>
    /// 通过比较其潜在的基因型，将这个代理与另一个代理进行比较。
    /// Compares this agent to another agent, by comparing their underlying genotypes.
    /// </summary>
    /// <param name="other">The agent to compare this agent to.</param>
    /// <returns>The result of comparing the underlying genotypes of this agent and the given agent.</returns>
    public int CompareTo(Agent other)
    {
        return this.Genotype.CompareTo(other.Genotype);
    }
    #endregion
    #endregion
}

