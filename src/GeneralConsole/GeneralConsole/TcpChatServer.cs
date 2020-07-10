using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using GeneralConsole;
using NetCoreServer;

namespace TcpChatServer
{
    class ChatSession : TcpSession
    {
        ConcurrentDictionary<Socket, byte[]> concurrentDictionary = new ConcurrentDictionary<Socket, byte[]>();
        public ChatSession(TcpServer server) : base(server) {}

        protected override void OnConnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} connected!");

            // Send invite message
            string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
            SendAsync(message);
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} disconnected!");
        }
        long FindPos(byte[] data, long offset, long data_size, byte[] find, long find_size)
        {
            if(data_size >= find_size)
            {
                for(var i = offset; i < data_size - find_size + 1; i++)
                {
                    if(data_size - find_size - 2 == i)
                    {
                        var n = 0;
                    }
                    var flag_size = 0;
                    for(var n = 0; n < find_size; n++)
                    {
                        if (data[i + n] == find[n])
                        {
                            flag_size++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (flag_size == find_size)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            {
                byte[] startBytes = new byte[4] { 0x89, 0x50, 0x4E, 0x47 };
                byte[] finalBytes = new byte[8] { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };

                byte[] itemValueNew = new byte[] { };
                if (concurrentDictionary.ContainsKey(this.Socket) == false)
                {
                    var startPos = FindPos(buffer, offset, size, startBytes, startBytes.Length);
                    var finalPos = FindPos(buffer, offset, size, finalBytes, finalBytes.Length);
                    if (startPos != (-1) && startPos < finalPos)
                    {
                        Array.Resize(ref itemValueNew, (int)(finalPos + finalBytes.Length - startPos));
                        System.Buffer.BlockCopy(buffer, (int)offset, itemValueNew, 0, (int)(finalPos + finalBytes.Length - startPos));
                        Program.blockingCollection.Add(itemValueNew);
                        //Program.concurrentQueue.Enqueue(itemValueNew);
                    }
                    else if (startPos != (-1))
                    {
                        Array.Resize(ref itemValueNew, (int)(size - startPos));
                        System.Buffer.BlockCopy(buffer, (int)offset, itemValueNew, 0, (int)(size - startPos));
                        concurrentDictionary.TryAdd(this.Socket, itemValueNew);
                    }
                    else
                    {
                        concurrentDictionary.TryAdd(this.Socket, itemValueNew);
                    }
                }
                else
                {
                    byte[] itemValue;
                    var finalPos = FindPos(buffer, offset, size, finalBytes, finalBytes.Length);
                    if (finalPos != (-1))
                    {
                        concurrentDictionary.TryRemove(this.Socket, out itemValue);

                        Array.Resize(ref itemValueNew, (int)(itemValue.Length + finalPos + finalBytes.Length));
                        System.Buffer.BlockCopy(itemValue, 0, itemValueNew, 0, itemValue.Length);
                        System.Buffer.BlockCopy(buffer, (int)offset, itemValueNew, itemValue.Length, (int)(finalPos + finalBytes.Length));
                        Program.blockingCollection.Add(itemValueNew);
                        //Program.concurrentQueue.Enqueue(itemValueNew);
                    }
                    else
                    {
                        bool isItemExists = concurrentDictionary.TryGetValue(this.Socket, out itemValue);
                        Array.Resize(ref itemValueNew, (int)(itemValue.Length + size));
                        System.Buffer.BlockCopy(itemValue, 0, itemValueNew, 0, itemValue.Length);
                        System.Buffer.BlockCopy(buffer, (int)offset, itemValueNew, itemValue.Length, (int)size);
                        concurrentDictionary.TryUpdate(this.Socket, itemValueNew, itemValue);
                    }
                }
                if (itemValueNew.Length != 0)
                {
                    return;
                }
            }
            string message = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine("Incoming: " + message);

            ChatMsg model;
            if (SerializeUtilities.ChatMsgDeserializer(out model, message))
            {
                Program.chatMsgCollection.Add(model);
            }

            // Multicast message to all connected sessions
            Server.Multicast(message);

            // If the buffer starts with '!' the disconnect the current session
            if (message == "!")
            {
                Disconnect();
            }
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    class ChatServer : TcpServer
    {
        public ChatServer(IPAddress address, int port) : base(address, port) {}

        protected override TcpSession CreateSession() { return new ChatSession(this); }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");
        }
    }

    class Program
    {
        public static BlockingCollection<ChatMsg> chatMsgCollection = new BlockingCollection<ChatMsg>();
        public static BlockingCollection<byte[]> blockingCollection = new BlockingCollection<byte[]>();
        public static ConcurrentQueue<byte[]> concurrentQueue = new ConcurrentQueue<byte[]>();
        public static ChatServer chatServer = null;
        public static void TcpChatMain(string[] args)
        {
            // TCP server port
            int port = 1111;
            if (args.Length > 0)
                port = int.Parse(args[0]);

            Console.WriteLine($"TCP server port: {port}");

            Console.WriteLine();

            // Create a new TCP chat server
            var server = new ChatServer(IPAddress.Any, port);

            // Start the server
            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                    continue;
                }

                // Multicast admin message to all sessions
                line = "(admin) " + line;
                server.Multicast(line);
            }

            // Stop the server
            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine("Done!");
        }
        public static void Start(string[] args)
        {
            // TCP server port
            int port = 1111;
            if (args.Length > 0)
            {
                port = int.Parse(args[0]);
            }
            Console.WriteLine($"TCP server port: {port}");

            Console.WriteLine();

            // Create a new TCP chat server
            chatServer = new ChatServer(IPAddress.Any, port);

            // Start the server
            Console.Write("Server starting...");
            chatServer.Start();
            Console.WriteLine("Done!");

            /*Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    chatServer.Restart();
                    Console.WriteLine("Done!");
                    continue;
                }

                // Multicast admin message to all sessions
                line = "(admin) " + line;
                chatServer.Multicast(line);
            }*/
        }
        public static void Stop()
        {
            // Stop the server
            Console.Write("Server stopping...");
            chatServer.Stop();
            Console.WriteLine("Done!");
        }
    }
}
