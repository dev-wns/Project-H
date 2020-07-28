using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Message
{
    public string text;
    public Text textObject;
}

public class GameMain : MonoBehaviour
{
    NetworkManager networkManager;

    [SerializeField]
    public List<Message> messageList = new List<Message>();
    public List<string> textList = new List<string>();

    // 인풋 필드. 안의 Text내용을 꺼내옵니다.
    public InputField inputMessage;
    // 복사한 텍스트들의 부모가 되는 객체
    // Content Size Fitter, Vertical Layout Group 컴포넌트를 넣어논 객체로써
    // 자동으로 위아래 간격을 맞춰줍니다.
    public GameObject textContents;
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

    public void ChatEnabled( bool enable )
    {
        if ( inputMessage == null ) throw new ArgumentNullException( "The appended object cannot be null" );

        inputMessage.enabled = enable;
    }

    public void MakeMessage( string text )
    {
        if ( messageList.Count >= maxMessageCount )
        {
            Destroy( messageList[0].textObject.gameObject );
            messageList.Remove( messageList[0] );
        }

        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate( textObject, chatPanel.transform );
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text;
        messageList.Add( newMessage );

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

