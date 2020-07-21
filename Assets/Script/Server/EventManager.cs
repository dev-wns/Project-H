using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum NETWORK_EVENT : byte
{
    CONNECTED,

    DISCONNECTED,

    END
}

// 네트워크 라이브러리에서 발생하는 이벤트들을 관리
public class EventManager
{
    object csEvent;

    // 네트워크 엔진에서 발생된 이벤트들을 보관해놓는 큐
    Queue<NETWORK_EVENT> networkEvents;

    // 서버에서 받은 패킷을 보관해놓는 큐
    Queue<Packet> networkMessageEvents;

    public EventManager()
    {
        this.networkEvents = new Queue<NETWORK_EVENT>();
        this.networkMessageEvents = new Queue<Packet>();
        this.csEvent = new object();
    }

    public bool hasEvent()
    {
        lock ( this.csEvent )
        {
            return this.networkEvents.Count > 0;
        }
    }

    public void EnqueueNetworkEvent( NETWORK_EVENT type )
    {
        lock( this.csEvent )
        {
            this.networkEvents.Enqueue( type );
        }
    }

    public NETWORK_EVENT DequeueNetworkEvent()
    {
        lock( this.csEvent )
        {
            return this.networkEvents.Dequeue();
        }
    }

    public bool hasMessage()
    {
        lock( this.csEvent )
        {
            return this.networkMessageEvents.Count > 0;
        }
    }

    public void EnqueueNetworkMessage( Packet buffer )
    {
        lock( this.csEvent )
        {
            this.networkMessageEvents.Enqueue( buffer );
        }
    }

    public Packet DequeueNetworkMessage()
    {
        lock( this.csEvent )
        {
            return this.networkMessageEvents.Dequeue();
        }
    }
}
