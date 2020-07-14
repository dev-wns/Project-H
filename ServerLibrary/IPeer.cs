using System.Collections.Generic;


// 서버와 클라이언트에서 공통으로 사용하는 세션 객체
// - 서버 -
// * 하나의 클라이언트 객체를 나타낸다.
// * 이 인터페이스를 구현한 객체를 NetworkService클래스의 callbackSessionCreated 호출 시 생성하여 리턴시켜 준다.
// * 객체를 풀링할지 여부는 사용자가 원하는대로 구현한다.

// - 클라이언트 -
// * 접속한 서버 객체를 나타낸다.

// 이 클래스의 모든 메서드는 thread unsafe하므로 공유자원에 접근할 때는 동기화 처리가 필요합니다.
public interface IPeer
{
    // 소켓버퍼로부터 데이터를 수신하여 패킷하나를 완성했을 때 호출된다.
    // 호출 흐름 : .Net Socket ReceiveAsync -> UserToken.OnReceive -> Peer.OnMessage

    // 패킷 순서에 대해서 ( TCP )
    // 이 메서드는 .Net Socket의 스레드풀에 의해 작동되어 호출되므로 어느 스레드에서 호출될지 알 수 없다.
    // 하지만 하나의 Peer객체에 대해서는 이 메서드가 완료된 이후 다음 패킷이 들어오도록 구현되어 있으므로
    // 클라이언트가 보낸 패킷 순서는 보장이 된다.

    // 주의할 점
    // 이 메서드에서 다른 Peer객체를 참조하거나 공유자원에 접근할 때는 동기화 처리를 해줘야 한다.
    // 메서드가 리턴되면 buffer는 비워지며 다음 패킷을 담을 준비를 하기 때문에 메서드를 리턴하기 전에 사용할 데이터를 모두 빼내야 한다.
    // buffer참조를 다른 객체에 넘긴 후 메서드가 리턴된 이후에 사용하게 해서는 안된다. 이런 경우에는 참조대신 복사하여 처리해야 한다.

    // Socket버퍼로부터 복사된 UserToken의 버퍼를 참조한다.
    void OnMessage( Const<byte[]> buffer );

    // 원격 연결이 끊겼을 때 호출된다.
    // 이 메서드가 호출된 이후부터는 데이터 전송이 불가능하다.
    void OnRemoved();
    void Send( Packet msg );
    void DisConnect();
    void ProcessUserOperation( Packet msg );
}
