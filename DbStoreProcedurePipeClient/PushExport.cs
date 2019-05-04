using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

public partial class UserDefinedFunctions
{
    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlBoolean fn_assPushExport_cusIDByPipe(String namedPipe, String codeExport, String columns, String query)
    {
        var client = new NamedPipeClientStream(namedPipe.ToString());
        client.Connect();
        StreamWriter writer = new StreamWriter(client);

        writer.WriteLine("#BEGIN=" + codeExport.ToString());
        writer.WriteLine("#COLUMN=" + columns);
        writer.Flush();

        int count = 0;
        bool hasPush = false;
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            connection.Open();
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    string[] rowData = new string[reader.FieldCount + 1];
                    while (reader.Read())
                    {
                        count++;
                        rowData[0] = count.ToString();
                        for (int i = 0; i < reader.FieldCount; i++)
                            rowData[i + 1] = reader[i].ToString();

                        writer.WriteLine(String.Join("|", rowData));
                        writer.Flush();

                        if (hasPush == false) hasPush = true;
                    }
                }
            }
        }


        writer.WriteLine("#TOTAL=" + count.ToString());
        writer.WriteLine("#END=" + codeExport.ToString());
        writer.Flush();

        client.Close();

        return new SqlBoolean(true);
    }


    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlBoolean fn_assPushExport_cusID(String host, Int32 port, String codeExport, String columns, String query)
    {
        IPAddress destination = Dns.GetHostAddresses(host)[0];
        IPEndPoint endPoint = new IPEndPoint(destination, port);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SendTo(Encoding.ASCII.GetBytes("#BEGIN=" + codeExport.ToString()), endPoint);
        socket.SendTo(Encoding.ASCII.GetBytes("#COLUMN=" + columns), endPoint);

        int count = 0;
        byte[] buffer;
        using (SqlConnection connection = new SqlConnection("context connection=true"))
        {
            connection.Open();
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    string[] rowData = new string[reader.FieldCount + 1];
                    while (reader.Read())
                    {
                        count++;
                        rowData[0] = count.ToString();
                        for (int i = 0; i < reader.FieldCount; i++)
                            rowData[i + 1] = reader[i].ToString();

                        buffer = Encoding.UTF8.GetBytes(String.Join("|", rowData));
                        socket.SendTo(buffer, endPoint);
                    }
                }
            }
        }

        socket.SendTo(Encoding.ASCII.GetBytes("#TOTAL=" + count.ToString()), endPoint);
        socket.SendTo(Encoding.ASCII.GetBytes("#END=" + codeExport.ToString()), endPoint);
        socket.Close();

        //IPHostEntry host = Dns.GetHostEntry("8.8.8.8");
        return new SqlBoolean(true);
    }
}