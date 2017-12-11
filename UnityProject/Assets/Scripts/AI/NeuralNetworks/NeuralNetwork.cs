/// Author: Samuel Arzt
/// Date: March 2017

#region Includes
using System;
#endregion

/// <summary>
/// 类代表一个完全连接的前馈神经网络。
/// Class representing a fully connceted feedforward neural network.
/// </summary>
public class NeuralNetwork
{
    #region Members
    /// <summary>
    /// 这个网络的各个神经层。
    /// The individual neural layers of this network.
    /// </summary>
    public NeuralLayer[] Layers
    {
        get;
        private set;
    }

    /// <summary>
    /// 一组无符号整数，表示网络从输入到输出层的每一层的节点数。
    /// An array of unsigned integers representing the node count 
    /// of each layer of the network from input to output layer.
    /// </summary>
    public uint[] Topology
    {
        get;
        private set;
    }

    /// <summary>
    /// 网络连接的总重量。
    /// The amount of overall weights of the connections of this network.
    /// </summary>
    public int WeightCount
    {
        get;
        private set;
    }
    #endregion

    #region Constructors
    /// <summary>
    /// 在给定的拓扑结构下，初始化一个全新的全连通前馈神经网络。
    /// Initialises a new fully connected feedforward neural network with given topology.
    /// </summary>
    /// <param name="topology">An array of unsigned integers representing the node count of each layer from input to output layer.</param>
    public NeuralNetwork(params uint[] topology)
    {
        //确定一个完整神经网络的层数
        this.Topology = topology;

        //Calculate overall weight count
        //计算总权重大小（应该是将每一层的权重矩阵的数据量都加起来）
        WeightCount = 0;
        for (int i = 0; i < topology.Length - 1; i++)
            WeightCount += (int) ((topology[i] + 1) * topology[i + 1]); // + 1 for bias node（+1偏置节点）

        //Initialise layers
        //初始化各神经层
        Layers = new NeuralLayer[topology.Length - 1];
        for (int i = 0; i<Layers.Length; i++)
            Layers[i] = new NeuralLayer(topology[i], topology[i + 1]);
    }
    #endregion

    #region Methods
    /// <summary>
    /// 使用当前网络的权重处理给定的输入。
    /// Processes the given inputs using the current network's weights.
    /// </summary>
    /// <param name="inputs">The inputs to be processed.</param>
    /// <returns>The calculated outputs.</returns>
    public double[] ProcessInputs(double[] inputs)
    {
        //Check arguments
        if (inputs.Length != Layers[0].NeuronCount)
            throw new ArgumentException("Given inputs do not match network input amount.");

        //Process inputs by propagating values through all layers
        double[] outputs = inputs;
        foreach (NeuralLayer layer in Layers)
            outputs = layer.ProcessInputs(outputs);

        return outputs;
        
    }

    /// <summary>
    /// 将该网络的权重设置为给定范围内的随机值。
    /// Sets the weights of this network to random values in given range.
    /// </summary>
    /// <param name="minValue">The minimum value a weight may be set to.</param>
    /// <param name="maxValue">The maximum value a weight may be set to.</param>
    public void SetRandomWeights(double minValue, double maxValue)
    {
        if (Layers != null)
        {
            foreach (NeuralLayer layer in Layers)
                layer.SetRandomWeights(minValue, maxValue);
        }
    }

    /// <summary>
    /// 返回一个新的神经网络实例，该实例具有相同的拓扑和激活函数，但是权重设置为它们的默认值。
    /// Returns a new NeuralNetwork instance with the same topology and 
    /// activation functions, but the weights set to their default value.
    /// </summary>
    public NeuralNetwork GetTopologyCopy()
    {
        NeuralNetwork copy = new NeuralNetwork(this.Topology);
        for (int i = 0; i < Layers.Length; i++)
            copy.Layers[i].NeuronActivationFunction = this.Layers[i].NeuronActivationFunction;

        return copy;
    }

    /// <summary>
    /// 复制这个神经网络，包括它的拓扑和权重。
    /// Copies this NeuralNetwork including its topology and weights.
    /// </summary>
    /// <returns>A deep copy of this NeuralNetwork</returns>
    public NeuralNetwork DeepCopy()
    {
        NeuralNetwork newNet = new NeuralNetwork(this.Topology);
        for (int i = 0; i < this.Layers.Length; i++)
            newNet.Layers[i] = this.Layers[i].DeepCopy();

        return newNet;
    }

    /// <summary>
    /// 返回一个以层序表示这个网络的字符串。
    /// Returns a string representing this network in layer order.
    /// </summary>
    public override string ToString()
    {
        string output = "";

        for (int i = 0; i<Layers.Length; i++)
            output += "Layer " + i + ":\n" + Layers[i].ToString();

        return output;
    }
    #endregion
}
