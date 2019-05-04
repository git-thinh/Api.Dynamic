--////////////////////////////////////////////////////
--// enable CLR
use master
go
sp_configure 'clr enabled', 1
GO
reconfigure
GO 

--////////////////////////////////////////////////////
--// install ASSEMBLY,PROCEDURE
USE [master]
GO
--CREATE DATABASE [clr]; 
GO
ALTER AUTHORIZATION ON DATABASE::[POST_TEST] TO [sa]; 
GO
ALTER DATABASE [POST_TEST] SET TRUSTWORTHY ON; 
GO
USE [POST_TEST]; 
GO
--////////////////////////////////////////////////////
--// DROP: ASSEMBLY,PROCEDURE ...
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fn_assPushExport_cusID]') AND type in (N'FN', N'IF',N'TF', N'FS', N'FT')) DROP FUNCTION [dbo].[fn_assPushExport_cusID]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fn_assPushExport_cusIDByPipe]') AND type in (N'FN', N'IF',N'TF', N'FS', N'FT')) DROP FUNCTION [dbo].[fn_assPushExport_cusIDByPipe]
IF EXISTS (SELECT ass.[name] FROM SYS.ASSEMBLIES ass WHERE ass.[name] = 'assPushExport') DROP ASSEMBLY assPushExport

--////////////////////////////////////////////////////
--// CREATE: ASSEMBLY,PROCEDURE ...
CREATE ASSEMBLY [assPushExport] FROM 'C:\Projects\Git\Api.Dynamic\DbStoreProcedurePipeClient\clrPushExport.dll' WITH PERMISSION_SET=EXTERNAL_ACCESS; 
GO
CREATE FUNCTION [fn_assPushExport_cusID](@host nvarchar(50), @port int, @codeExport nvarchar(MAX), @columns nvarchar(MAX), @query nvarchar(MAX)) RETURNS bit AS EXTERNAL NAME [assPushExport].[UserDefinedFunctions].[fn_assPushExport_cusID]; 
GO
CREATE FUNCTION [fn_assPushExport_cusIDByPipe](@namedPipe nvarchar(50), @codeExport nvarchar(MAX), @columns nvarchar(MAX), @query nvarchar(MAX)) RETURNS bit AS EXTERNAL NAME [assPushExport].[UserDefinedFunctions].[fn_assPushExport_cusIDByPipe]; 
GO
--////////////////////////////////////////////////////
--// TEST ...
USE [POST_TEST]; 
GO
--SELECT [dbo].[fn_assPushExport_cusID](
--'127.0.0.1', 41181
--,'EXPORT_ASSETATTRPRICEHISTORY'
--,'Id,HistoryVersionId,AssetCode,Price,Status,Created,CreatedBy,Updated,UpdatedBy'
--,'select top 10 Id,HistoryVersionId,AssetCode,Price,Status,Created,CreatedBy,Updated,UpdatedBy from pos.AssetAttrPriceHistory'
--) as [Export_Success];

--SELECT [dbo].[fn_assPushExport_cusIDByPipe](
--'PipesOfPiece'
--,'EXPORT_ASSETATTRPRICEHISTORY'
--,'Id,HistoryVersionId,AssetCode,Price,Status,Created,CreatedBy,Updated,UpdatedBy'
--,'select top 10 Id,HistoryVersionId,AssetCode,Price,Status,Created,CreatedBy,Updated,UpdatedBy from pos.AssetAttrPriceHistory'
--) as [Export_Success];