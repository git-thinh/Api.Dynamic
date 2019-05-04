using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.SqlServer.Server;


public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None)]
    public static SqlBoolean export_cusID(String ids)
    {
        IPAddress destination = Dns.GetHostAddresses("127.0.0.1")[0];
        IPEndPoint endPoint = new IPEndPoint(destination, 41181);
        byte[] buffer = Encoding.ASCII.GetBytes("CLR:" + Guid.NewGuid().ToString());
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SendTo(buffer, endPoint);
        socket.Close();

        //IPHostEntry host = Dns.GetHostEntry("8.8.8.8");
        return new SqlBoolean(true);
    }
}

public partial class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void SqlStoredProcedure1()
    {
        SqlPipe sqlP = SqlContext.Pipe;
        sqlP.Send("Hello World");

        int port = 41181;
        IPAddress destination = Dns.GetHostAddresses("127.0.0.1")[0];
        IPEndPoint endPoint = new IPEndPoint(destination, port);
        byte[] buffer = Encoding.ASCII.GetBytes("CLR:" + Guid.NewGuid().ToString());
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SendTo(buffer, endPoint);
        socket.Close();

        // Put your code here
    }
}

public class SayHi
{
    [SqlProcedure]
    public static void Voice(out string message)
    {
        SqlContext.Pipe.Send("Hello world! is comming from SqlContext.Pipe.Send method." + Environment.NewLine);
        message = "Hello world! is comming from out parameter.";
    }
}
