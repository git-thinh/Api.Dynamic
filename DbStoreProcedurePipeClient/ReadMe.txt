You must create the assembly with external access permission set. The assembly must also be signed or the database set as trustworthy and the database owner granted the external access assembly permission. The permission and assert code above is not required.

E.g.

using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.Net;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction(DataAccess=DataAccessKind.None)]
    public static SqlString getname(String ipAddress)
    {
        IPHostEntry host = Dns.GetHostEntry(ipAddress);
        return new SqlString(host.HostName);
    }
}
Save the code to C:\resolve\resolve.cs and compile it...

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /t:library /out:C:\resolve\resolve.dll C:\resolve\resolve.cs
Copy the .dll to the server if not a local instance. Then create the objects...

USE [master]
GO
CREATE DATABASE [clr]; 
GO
ALTER AUTHORIZATION ON DATABASE::[clr] TO [sa]; 
GO
ALTER DATABASE [clr] SET TRUSTWORTHY ON; 
GO
USE [clr]; 
GO
CREATE ASSEMBLY [assResolve] FROM 'C:\resolve\resolve.dll' 
WITH PERMISSION_SET=EXTERNAL_ACCESS; 
GO
CREATE FUNCTION [fnGetHostName](@ipAddress nvarchar(255)) RETURNS nvarchar(255) 
AS EXTERNAL NAME [assResolve].[UserDefinedFunctions].[getname]; 
GO
SELECT [dbo].[fnGetHostName](N'8.8.8.8') as [host]; 
GO
USE [master]