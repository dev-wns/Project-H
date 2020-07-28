using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ChatMessage
{
    public string text;
    public Text textObject;
}

public class GameMain : MonoBehaviour
{
    NetworkManager networkManager;

    public InputField inputMessage;
    [SerializeField]
    public List<Message> messageList = new List<Message>();
    public List<string> textList = new List<string>();
    // 
    public GameObject chatPanel;
    // 복사할 텍스트 오브젝트 객체
    public GameObject textObject;

    private int maxMessageCount = 25;

    private void Awake()
    {
        this.networkManager = GameObject.Find( "NetworkManager" ).GetComponent<NetworkManager>();
    }

    private void Update()
    {
        if ( textList.Count > 0 )
        {
            MakeMessage( textList[0] );
            textList.Remove( textList[0] );
        }
    }

    public void MakeMessage( string text )
    {
        // 최대 수치를 넘어가면 하나의 메세지를 지웁니다.
        if ( messageList.Count >= maxMessageCount )
        {
            Destroy( messageList[0].textObject.gameObject );
            messageList.Remove( messageList[0] );
        }

        // 새로운 메세지 생성
        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate( textObject, chatPanel.transform );
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text;

        messageList.Add( newMessage );

        // 생성완료된 메세지를 리스트에서 지웁니다.
        textList.Remove( text );
    }

    public void SendChatMessageToServer()
    {
        Packet msg = Packet.Create( ( short )PROTOCOL.CHAT_MSG_REQ );
        msg.Push( inputMessage.text );
        networkManager.Send( msg );
        inputMessage.text = "";
    }
}

