using extOSC;
using UnityEngine;

public class SendOSC : MonoBehaviour
{
  public string ipAddress = "127.0.0.1";
  public int port = 8000;

  private OSCTransmitter transmitter;

  void Start()
  {
    transmitter = gameObject.AddComponent<OSCTransmitter>();
    transmitter.RemoteHost = ipAddress;
    transmitter.RemotePort = port;
  }

  void Update()
  {
    var message = new OSCMessage("This is Unity");
    message.AddValue(OSCValue.Float(transform.position.x));
    message.AddValue(OSCValue.Float(transform.position.y));
    message.AddValue(OSCValue.Float(transform.position.z));

    transmitter.Send(message);
  }
}
