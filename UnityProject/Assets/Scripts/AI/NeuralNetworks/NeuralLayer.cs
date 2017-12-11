/// Author: Samuel Arzt
/// Date: March 2017

#region Includes
using System;
#endregion

/// <summary>
/// 类表示一个完全连接的前馈神经网络的一层。
/// Class representing a single layer of a fully connected feedforward neural network.
/// </summary>
public class NeuralLayer
{
    #region Members
    private static Random randomizer = new Random();

    /// <summary>
    /// 代表一个人工神经元的激活功能。
    /// Delegate representing the activation function of an artificial neuron.
    /// </summary>
    /// <param name="xValue">The input value of the function.</param>
    /// <returns>The calculated output value of the function.</returns>
    public delegate double ActivationFunction(double xValue);

    /// <summary>
    /// 这一层的神经元所使用的激活功能。
    /// The activation function used by the neurons of this layer.
    /// </summary>
    /// <remarks>The default activation function is the sigmoid function (see <see cref="MathHelper.SigmoidFunction(double)"/>).</remarks>
    public ActivationFunction NeuronActivationFunction = MathHelper.SigmoidFunction;

    /// <summary>
    /// 这一层的神经元数量。
    /// The amount of neurons in this layer.
    /// </summary>
    public uint NeuronCount
    {
        get;
        private set;
    }

    /// <summary>
    /// 这一层的神经元数量与之相连。下一层的神经元数量。
    /// The amount of neurons this layer is connected to, i.e., the amount of neurons of the next layer.
    /// </summary>
    public uint OutputCount
    {
        get;
        private set;
    }

    /// <summary>
    /// 这一层连接到下一层的权重。
    /// The weights of the connections of this layer to the next layer.
    /// E.g., weight [i, j] is the weight of the connection from the i-th weight
    /// of this layer to the j-th weight of the next layer.
    /// </summary>
    public double[,] Weights
    {
        get;
        private set;
    }
    #endregion

    #region Constructors
    /// <summary>
    /// 在给定数量的节点和与下一层的节点数量的连接的情况下，初始化一个完全连接的前馈神经网络的新神经层。
    /// Initialises a new neural layer for a fully connected feedforward neural network with given 
    /// amount of node and with connections to the given amount of nodes of the next layer.
    /// </summary>
    /// <param name="nodeCount">The amount of nodes in this layer.</param>
    /// <param name="outputCount">The amount of nodes in the next layer.</param>
    /// <remarks>All weights of the connections from this layer to the next are initialised with the default double value.</remarks>
    public NeuralLayer(uint nodeCount, uint outputCount)
    {
        this.NeuronCount = nodeCount;//神经元计数
        this.OutputCount = outputCount;//输出数
        //初始化权重矩阵
        Weights = new double[nodeCount + 1, outputCount]; // + 1 for bias node
    }
    #endregion

    #region Methods
    /// <summary>
    /// 将此层的权重设置为给定的值。
    /// Sets the weights of this layer to the given values.
    /// </summary>
    /// <param name="weights">
    /// 设置从这一层到下一层的连接的权重值。
    /// The values to set the weights of the connections from this layer to the next to.
    /// </param>
    /// <remarks>
    /// The values are ordered in neuron order. E.g., in a layer with two neurons with a next layer of three neurons 
    /// the values [0-2] are the weights from neuron 0 of this layer to neurons 0-2 of the next layer respectively and 
    /// the values [3-5] are the weights from neuron 1 of this layer to neurons 0-2 of the next layer respectively.
    /// </remarks>
    public void SetWeights(double[] weights)
    {
        //Check arguments
        if (weights.Length != this.Weights.Length)
            throw new ArgumentException("Input weights do not match layer weight count.");

        // Copy weights from given value array
        int k = 0;
        for (int i = 0; i < this.Weights.GetLength(0); i++)
            for (int j = 0; j < this.Weights.GetLength(1); j++)
                this.Weights[i, j] = weights[k++];
    }

    /// <summary>
    /// 使用当前权重对下一层进行处理。
    /// Processes the given inputs using the current weights to the next layer.
    /// </summary>
    /// <param name="inputs">The inputs to be processed.</param>
    /// <returns>The calculated outputs.</returns>
    public double[] ProcessInputs(double[] inputs)
    {
        //Check arguments
        if (inputs.Length != NeuronCount)
            throw new ArgumentException("Given xValues do not match layer input count.");

        //Calculate sum for each neuron from weighted inputs and bias
        //计算每个神经元的加权输入和偏差
        double[] sums = new double[OutputCount];
        //Add bias (always on) neuron to inputs
        //添加偏倚(总是在)神经元输入
        double[] biasedInputs = new double[NeuronCount + 1];
        inputs.CopyTo(biasedInputs, 0);
        biasedInputs[inputs.Length] = 1.0;

        for (int j = 0; j < this.Weights.GetLength(1); j++)
            for (int i = 0; i < this.Weights.GetLength(0); i++)
                sums[j] += biasedInputs[i] * Weights[i, j];

        //Apply activation function to sum, if set
        //将激活函数应用于sum，如果设置
        if (NeuronActivationFunction != null)
        {
            for (int i = 0; i < sums.Length; i++)
                sums[i] = NeuronActivationFunction(sums[i]);
        }

        return sums;
    }

    /// <summary>
    /// 复制这个神经层，包括它的重量。
    /// Copies this NeuralLayer including its weights.
    /// </summary>
    /// <returns>A deep copy of this NeuralLayer</returns>
    public NeuralLayer DeepCopy()
    {
        //Copy weights
        double[,] copiedWeights = new double[this.Weights.GetLength(0), this.Weights.GetLength(1)];

        for (int x = 0; x < this.Weights.GetLength(0); x++)
            for (int y = 0; y < this.Weights.GetLength(1); y++)
                copiedWeights[x, y] = this.Weights[x, y];

        //Create copy
        NeuralLayer newLayer = new NeuralLayer(this.NeuronCount, this.OutputCount);
        newLayer.Weights = copiedWeights;
        newLayer.NeuronActivationFunction = this.NeuronActivationFunction;

        return newLayer;
    }

    /// <summary>
    /// 将连接的权重设置为在给定范围内的随机值。
    /// Sets the weights of the connection from this layer to the next to random values in given range.
    /// </summary>
    /// <param name="minValue">The minimum value a weight may be set to.</param>
    /// <param name="maxValue">The maximum value a weight may be set to.</param>
    public void SetRandomWeights(double minValue, double maxValue)
    {
        double range = Math.Abs(minValue - maxValue);
        for (int i = 0; i < Weights.GetLength(0); i++)
            for (int j = 0; j < Weights.GetLength(1); j++)
                Weights[i, j] = minValue + (randomizer.NextDouble() * range); //random double between minValue and maxValue
    }

    /// <summary>
    /// 返回表示该层的连接权重的字符串。
    /// Returns a string representing this layer's connection weights.
    /// </summary>
    public override string ToString()
    {
        string output = "";

        for (int x = 0; x < Weights.GetLength(0); x++)
        {
            for (int y = 0; y < Weights.GetLength(1); y++)
                output += "[" + x + "," + y + "]: " + Weights[x, y];

            output += "\n";
        }

        return output;
    }
    #endregion
}
