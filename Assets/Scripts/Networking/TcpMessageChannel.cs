using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace PlayerHostGame.Networking
{
    public sealed class TcpMessageChannel : IDisposable
    {
        private const int LengthPrefixBytes = 4;

        private readonly TcpClient tcpClient;
        private readonly NetworkStream stream;
        private readonly ConcurrentQueue<NetworkMessage> incomingMessages = new ConcurrentQueue<NetworkMessage>();
        private readonly object writeLock = new object();
        private int closeRequested;
        private long lastReceiveMilliseconds;
        private volatile bool isOpen;
        private volatile string closeReason;

        public bool IsOpen
        {
            get { return isOpen; }
        }

        public string CloseReason
        {
            get { return closeReason; }
        }

        public long MillisecondsSinceLastReceive
        {
            get { return NetworkClock.NowMilliseconds - Interlocked.Read(ref lastReceiveMilliseconds); }
        }

        public TcpMessageChannel(TcpClient client)
        {
            tcpClient = client;
            tcpClient.NoDelay = true;
            tcpClient.SendTimeout = NetworkConfig.SendTimeoutMs;
            stream = tcpClient.GetStream();
            lastReceiveMilliseconds = NetworkClock.NowMilliseconds;
            isOpen = true;

            Thread readThread = new Thread(ReadLoop) { IsBackground = true };
            readThread.Start();
        }

        public bool TryDequeue(out NetworkMessage message)
        {
            return incomingMessages.TryDequeue(out message);
        }

        public bool Send(NetworkMessage message)
        {
            if (!isOpen)
            {
                return false;
            }

            byte[] payload = message.ToBytes();
            if (payload.Length > NetworkConfig.MaxMessageBytes)
            {
                return false;
            }

            byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);
            try
            {
                lock (writeLock)
                {
                    stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                    stream.Write(payload, 0, payload.Length);
                }

                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is ObjectDisposedException)
            {
                Close(exception.Message);
                return false;
            }
        }

        public void Close(string reason = null)
        {
            if (Interlocked.Exchange(ref closeRequested, 1) == 1)
            {
                return;
            }

            if (closeReason == null)
            {
                closeReason = reason ?? "Closed locally";
            }

            isOpen = false;
            tcpClient.Close();
        }

        public void Dispose()
        {
            Close();
        }

        private void ReadLoop()
        {
            byte[] lengthBuffer = new byte[LengthPrefixBytes];
            try
            {
                while (isOpen)
                {
                    if (!TryReadExactly(lengthBuffer, LengthPrefixBytes))
                    {
                        break;
                    }

                    int payloadLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (payloadLength <= 0 || payloadLength > NetworkConfig.MaxMessageBytes)
                    {
                        closeReason = "Invalid message length";
                        break;
                    }

                    byte[] payload = new byte[payloadLength];
                    if (!TryReadExactly(payload, payloadLength))
                    {
                        break;
                    }

                    if (!NetworkMessage.TryParse(payload, out NetworkMessage message))
                    {
                        closeReason = "Malformed message";
                        break;
                    }

                    Interlocked.Exchange(ref lastReceiveMilliseconds, NetworkClock.NowMilliseconds);
                    incomingMessages.Enqueue(message);
                }
            }
            catch (IOException exception)
            {
                closeReason = exception.Message;
            }
            catch (ObjectDisposedException)
            {
                closeReason = "Connection closed locally";
            }
            finally
            {
                isOpen = false;
                if (closeReason == null)
                {
                    closeReason = "Remote side closed the connection";
                }
            }
        }

        private bool TryReadExactly(byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int bytesRead = stream.Read(buffer, offset, count - offset);
                if (bytesRead == 0)
                {
                    return false;
                }

                offset += bytesRead;
            }

            return true;
        }
    }
}
