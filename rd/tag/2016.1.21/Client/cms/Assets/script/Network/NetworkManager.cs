using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public int GameServerPort = 9595;                      //测试Socket服务器端口
    public string GameServerAdd = "127.0.0.1";              //本地Socket服务器地址

    private int count;
    private TimerInfo timer;
	private static Queue<KeyValuePair<int, ProtocolMessage>> sEvents = new Queue<KeyValuePair<int, ProtocolMessage>>();

    void Start()
    {

    }

    public void OnInit()
    {
    }

    ///------------------------------------------------------------------------------------
	public static void AddEvent(int _event, ProtocolMessage data)
    {
		sEvents.Enqueue(new KeyValuePair<int, ProtocolMessage>(_event, data));
    }

    void Update()
    {
        if (sEvents.Count > 0)
        {
            while (sEvents.Count > 0)
            {
				KeyValuePair<int, ProtocolMessage> _event = sEvents.Dequeue();
				//
                switch (_event.Key)
                {
				case ResponseState.Connect:
                    UINetRequest.Close();
					GameEventMgr.Instance.FireEvent<int>(NetEventList.NetConnectFinished,1);
					break; 
				case ResponseState.UnConnect:
					GameEventMgr.Instance.FireEvent<int>(NetEventList.NetConnectFinished,0);
					break;
				case ResponseState.Disconnect:
					Logger.LogError("disconnect msg");
					break;
				case ResponseState.Exception:
					Logger.LogError("net Exception");
					break;
				case ResponseState.Message:
				{ 
					ProtocolMessage pmsg = _event.Value;
					//Logger.Log("receiv msg : " + pmsg.GetMessageType());
                    if (pmsg.GetMessageType() == (int)PB.sys.ERROR_CODE)
                    {
                        PB.HSErrorCode errorCode = pmsg.GetProtocolBody<PB.HSErrorCode>();

                        if (errorCode != null)
                        {
                            if (GameEventMgr.Instance.IsListenEvent(errorCode.hsCode.ToString()) == true)
                            {
                                GameEventMgr.Instance.FireEvent<ProtocolMessage>(errorCode.hsCode.ToString(), pmsg);
                            }
							//Logger.LogError("error for net request :error code =  " + errorCode.errCode);
							Logger.LogErrorFormat("error for net request :error code =  {0:X} " , errorCode.errCode);
                        }
                    }
                    else
                    {
                        GameEventMgr.Instance.FireEvent<ProtocolMessage>(pmsg.GetMessageType().ToString(), pmsg);
                    }
					
				}
					break;
                default:
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 链接请求完成
    /// </summary>
    void OnNetConnectFinished(int state)
    {
        if (1 == state)
        {
            Logger.LogWarning("OK for net");
        }
        else
        {
            Logger.LogError("Error for Net");
        }
    }

    /// <summary>
    /// 发送链接请求
    /// </summary>
    public void SendConnect()
    {
		UINetRequest.Open ();
        SocketClient.SendConnect();
    }

    /// <summary>
    /// 发送SOCKET消息
    /// </summary>
    public void SendMessage(ProtocolMessage buffer, bool showMask = true)
    {
        SocketClient.SendMessage(buffer);
        if (showMask)
        {
            UINetRequest.Open();
        }
    }

	public void SendMessage(int messageType, ProtoBuf.IExtensible builder, bool showMask = true)
	{
		ProtocolMessage pbmsg = ProtocolMessage.Create (messageType, builder);
		SendMessage (pbmsg, showMask);
	}

    /// <summary>
    /// 析构函数
    /// </summary>
    void OnDestroy()
    {
		Logger.Log("~NetworkManager was destroy");
    }
}
