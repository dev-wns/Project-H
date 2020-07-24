using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Message
{
    public string text;
    public Text textObject;
};

public class ChatManager : MonoBehaviour
{
    [SerializeField]
    public List<Message> messageList = new List<Message>();
    public static List<string> textList = new List<string>();
    public GameObject chatPanel;
    public GameObject textObject;
    public InputField inputField;
    private static int maxMessage = 25;

    public void Update()
    {
        if ( textList.Count == 0 ) return;

        MakeMessage( textList[0] );
        textList.Remove( textList[0] );
    }

    public static void AddPacket( string text )
    {
        textList.Add( text );
    }

    public void MakeMessage( string text )
    {
        if ( messageList.Count >= maxMessage )
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
    }

    public void SendMessageToServer()
    {
        Packet msg = Packet.Create( ( short )PROTOCOL.CHAT_MSG_REQ );
        msg.Push( inputField.text );
        Server.GetServer().Send( msg );
        inputField.text = "";
    }
}
