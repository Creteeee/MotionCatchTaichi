using Mediapipe;
using UnityEngine;

public sealed class HelloWorld : MonoBehaviour
{
  private const string _ConfigText = @"
input_stream: ""in""
output_stream: ""out""
node {
  calculator: ""PassThroughCalculator""
  input_stream: ""in""
  output_stream: ""out1""
}
node {
  calculator: ""PassThroughCalculator""
  input_stream: ""out1""
  output_stream: ""out""
}
";

  private void Start()
  {
    using var graph = new CalculatorGraph(_ConfigText);
    using var poller = graph.AddOutputStreamPoller<string>("out");
    graph.StartRun();

    for (var i = 0; i < 10; i++)
    {
      graph.AddPacketToInputStream("in", Packet.CreateStringAt("Hello World!", i));
    }

    graph.CloseInputStream("in");
    var packet = new Packet<string>();

    while (poller.Next(packet))
    {
      Debug.Log(packet.Get());
    }
    graph.WaitUntilDone();
  }
}
