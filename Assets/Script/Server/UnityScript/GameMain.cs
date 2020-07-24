using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    string inputText;
    List<string> receivedTexts;
    NetworkManager networkManager;

    Vector2 currentScrollpos = new Vector2();

    private void Awake()
    {
        this.inputText = "";
        this.receivedTexts = new List<string>();
        this.networkManager = GameObject.Find( "NetworkManager" ).GetComponent<NetworkManager>(); ;
    }

    public void OnReceiveChatMessage( string text )
    {
        this.receivedTexts.Add( text );
        this.currentScrollpos.y = float.PositiveInfinity;
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        currentScrollpos = GUILayout.BeginScrollView( 
            currentScrollpos,
            GUILayout.MaxWidth( Screen.width ),
            GUILayout.MinWidth( Screen.width ),
            GUILayout.MaxHeight( Screen.height - 100 ),
            GUILayout.MinHeight( Screen.height - 100 ) );

        foreach(string text in this.receivedTexts )
        {
            GUILayout.BeginHorizontal();
            GUI.skin.label.wordWrap = true;
            GUILayout.Label( text );
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Input
        GUILayout.BeginHorizontal();
        this.inputText = GUILayout.TextField( this.inputText,
                         GUILayout.MaxWidth( Screen.width - 100 ),
                         GUILayout.MinWidth( Screen.width - 100 ),
                         GUILayout.MaxHeight( 50 ), GUILayout.MinHeight( 50 ) );
        if ( GUILayout.Button("Send", 
            GUILayout.MaxWidth(100), GUILayout.MinWidth(100), 
            GUILayout.MaxHeight( 50 ), GUILayout.MinHeight( 50 ) ) )
            {
                Packet msg = Packet.Create( ( short )PROTOCOL.CHAT_MSG_REQ );
                msg.Push( this.inputText );
                this.networkManager.Send( msg );
                this.inputText = "";
            }
        GUILayout.EndHorizontal();
    }
}

