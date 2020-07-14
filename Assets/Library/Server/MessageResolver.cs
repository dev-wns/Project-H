using System;
using System.Collections.Generic;

class Defines
{
    public static readonly short HEADERSIZE = 2;
}

// [header][body] ������ ���� �����͸� �Ľ��ϴ� Ŭ����
// - header : ������ ������. Defines.HEADERSIZER�� ���ǵ� Ÿ�Ը�ŭ�� ũ�⸦ ���´�.
//            2����Ʈ�� ��� Int16, 4����Ʈ�� ��� Int32�� ó���ϸ� �ȴ�.
//            ������ ũ�Ⱑ Int16.Max���� ���� �ʴ´ٸ� 2����Ʈ�� ó���ϴ� ���� ���� �� ����.
//
// - body   : �޼��� ����
public class MessageResolver
{
    public delegate void CompletedMessageACallback( Const<byte[]> buffer );

    // �޼��� ������
    int messageSize;

    // �������� ����
    byte[] messageBuffer = new byte[1024];

    // ���� �������� ������ �ε����� ����Ű�� ����.
    // ��Ŷ �ϳ��� �ϼ��� �ڿ��� 0���� �ʱ�ȭ ������� �Ѵ�.
    int currentPosition;

    // �о�;� �� ��ǥ ��ġ
    int positionToRead;

    // ���� ������
    int remainBytes;

    // ��ǥ�������� ������ ��ġ������ ����Ʈ�� ���� ���۷κ��� �����Ѵ�.
    // �����Ͱ� ���ڶ� ��� ���� ���� ����Ʈ ������ �����Ѵ�.
    // <return> �� �о����� ture, �����Ͱ� ���ڶ� ���о����� false�� �����Ѵ�.
    bool ReadUntil( byte[] buffer, ref int srcPosition, int offset, int transferred )
    {
        if ( this.currentPosition >= offset + transferred )
        {
            // ���� ������ ��ŭ �� ���� �����̹Ƿ� ���̻� ���� �����Ͱ� ����.
            return true;
        }

        // �о�;� �� ����Ʈ
        // �����Ͱ� �и��Ǿ� �� ��� ������ �о���� ���� ���༭ ������ ��ŭ �о�� �� �ֵ��� ����� �ش�.
        int copySize = this.positionToRead - this.currentPosition;

        // ���� �����Ͱ� �� ���ٸ� ������ ��ŭ�� �����Ѵ�.
        if ( this.remainBytes < copySize )
        {
            copySize = this.remainBytes;
        }

        // ���ۿ� ����
        Array.Copy( buffer, srcPosition, this.messageBuffer, this.currentPosition, copySize );

        // ���� ���� ������ �̵�.
        srcPosition += copySize;

        // Ÿ�� ���� ������ �̵�
        this.currentPosition += copySize;

        // ���� ����Ʈ ��
        this.remainBytes -= copySize;

        // ��ǥ������ ���� �������� false
        if ( this.currentPosition < this.positionToRead )
        {
            return false;
        }

        return true;
    }

    // ���� ���۷κ��� �����͸� ������ ������ ȣ��ȴ�.
    // �����Ͱ� ���� ���� ������ ��� ��Ŷ�� ����� callback�Լ��� ȣ���Ѵ�.
    // �ϳ��� ��Ŷ�� �ϼ����� ���ߴٸ� ���ۿ� ������ ���� �� ���� ������ ��ٸ���.
    public void OnReceive( byte[] buffer, int offset, int transferred, CompletedMessageACallback callback )
    {
        // �̹� receive�� �о���� �� ����Ʈ ��
        this.remainBytes = transferred;

        // ���� ������ �����ǰ�.
        // ��Ŷ�� ������ ���� �� ��� ���� ������ �������� ��� ������ �����ϴµ� �� ó���� ���� ����
        int srcPosition = offset;

        // ���� �����Ͱ� �ִٸ� ��� �ݺ��Ѵ�.
        while ( this.remainBytes > 0 )
        {
            bool completed = false;

            // �����ŭ ������ ��� ����� ���� �д´�.
            if ( this.currentPosition < Defines.HEADERSIZE )
            {
                // ��ǥ���� ���� ( ��� ��ġ���� �����ϵ��� ���� )
                this.positionToRead = Defines.HEADERSIZE;

                completed = ReadUntil( buffer, ref srcPosition, offset, transferred );
                if ( completed == false )
                {
                    // ���� �� ���о����Ƿ� ���� receive�� ��ٸ���.
                    return;
                }

                // ��� �ϳ��� ������ �о�����Ƿ� �޼��� ����� ���Ѵ�.
                this.messageSize = GetBodySize();

                // ���� ��ǥ ���� ( ��� + �޼��� ������ )
                this.positionToRead = this.messageSize + Defines.HEADERSIZE;
            }

            // �޼����� �д´�.
            completed = ReadUntil( buffer, ref srcPosition, offset, transferred );

            if ( completed == true )
            {
                // ��Ŷ �ϳ��� �ϼ� �ߴ�.

                callback( new Const<byte[]>( this.messageBuffer ) );

                ClearBuffer();
            }
        }
    }

    int GetBodySize()
    {
        // ��� Ÿ���� ����Ʈ��ŭ�� �о�� �޼��� ����� �����Ѵ�.
        Type type = Defines.HEADERSIZE.GetType();
        if ( type.Equals( typeof( Int16 ) ) == true )
        {
            return BitConverter.ToInt16( this.messageBuffer, 0 );
        }
        return BitConverter.ToInt32( this.messageBuffer, 0 );
    }

    void ClearBuffer()
    {
        Array.Clear( this.messageBuffer, 0, this.messageBuffer.Length );

        this.currentPosition = 0;
        this.messageSize = 0;
    }
}
