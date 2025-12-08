using System;
using extOSC;
using UnityEngine;

public class SendOSC : Singleton<SendOSC>
{
  public string ipAddress = "127.0.0.1";
  public int port = 8000;
  public OSCTransmitter transmitter;

  void Start()
  {
    transmitter = gameObject.AddComponent<OSCTransmitter>();
    transmitter.RemoteHost = ipAddress;
    transmitter.RemotePort = port;
  }

  void Update()
  {
    var message = new OSCMessage("/unity");//必须有的格式
    // message.AddValue(OSCValue.Float(transform.position.x));
    // message.AddValue(OSCValue.Float(transform.position.y));
    // message.AddValue(OSCValue.Float(transform.position.z));
    //message.AddValue(OSCValue.String("HelloWorld"));
    //transmitter.Send(message);
  }

  public void SendOSCMessage(String str)
  {
    var message = new OSCMessage("/unity");
    message.AddValue(OSCValue.String(str));
    transmitter.Send(message);
    
  }
}
