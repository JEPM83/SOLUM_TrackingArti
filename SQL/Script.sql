USE [SOLUM_EXPRESS]
GO
insert into "@EX_PROCESO" (Code,Name,Type,State) values ('SAP_PER_TRACKING','ENVIO DE DATOS DE INTERFACES TRACKING A TABLAS PERIFERICAS',0,0)
insert into "@EX_MODELO" (Code,Name,SQL,Columnas,State) values ('M3_TRACKING','Modelo consulta de tracking para cliente ARTI','BZ_CONEP_TRACKING_ARTI @A_CLIENTE @A_FECINI @A_FECFIN @A_PEDIDO',23,0)
insert into "@EX_RUTA" (Code,U_EX_CLIENTE,U_EX_PROCESO,U_EX_MODELO,Environment,Server,Port,Route,Sftp,Historic,UserSap,Password,Email,Subject,Attached,Zip,State,LogErr)
values (144,'20100310288','SAP_PER_TRACKING',NULL,0,NULL,NULL,'C:\FTP Clientes\Arti\IN\TRACKING\TEST\',0,'C:\FTP Clientes\Arti\IN\TRACKING\TEST\HIS\',NULL,NULL,0,NULL,0,0,0,'C:\FTP Clientes\Arti\IN\TRACKING\TEST\LOG\')

insert into "@EX_FILE" (Code,U_EX_RUTA,Prefix,Extent,Separator,[Order],State,Destino) values(180,144,'SAP_TRACK_','TXT','	',1,0,'SAP_HOST_TRACKING')

create table SAP_HOST_TRACKING
(Code int identity(1,1) not null,
Sociedad nvarchar(100) not null,
HostGroupId varchar(25) not null,
U_EX_CLIENTE nvarchar(30) not null,
Interno nvarchar(25) not null,
FecInterno nvarchar(25) not null,
Pedido nvarchar(25) not null,
FecPed nvarchar(25) null,
FecApruCred nvarchar(25) null,
FecApruDscto nvarchar(25) null,
FecApruExhib nvarchar(25) null,
Fact_BV nvarchar(25) null,
FecFact_BV nvarchar(25) null
)

create table SAP_TRACKING(
Code int identity(1,1) not null,
Sociedad nvarchar(100) not null,
U_EX_CLIENTE nvarchar(30) not null,
Interno nvarchar(25) not null,
FecInterno nvarchar(25) not null,
Pedido nvarchar(25) not null,
FecPed nvarchar(25) null,
FecApruCred nvarchar(25) null,
FecApruDscto nvarchar(25) null,
FecApruExhib nvarchar(25) null,
Fact_BV nvarchar(25) null,
FecFact_BV nvarchar(25) null,
Actualizaciones int not null,
Fecha_creacion datetime not null,
Fecha_actualizacion datetime null
)

IF EXISTS (SELECT * FROM sysobjects WHERE name='BZ_CONEP_TRACKING_ARTI') 
BEGIN
	drop procedure [dbo].BZ_CONEP_TRACKING_ARTI
END
go   
create procedure [dbo].BZ_CONEP_TRACKING_ARTI (
	@HostGroupId varchar(25)
)      
as    
begin transaction transaction_tracking
insert SAP_TRACKING (Sociedad,U_EX_CLIENTE,Interno,FecInterno,Pedido,FecPed,FecApruCred,FecApruDscto,FecApruExhib,Fact_BV,FecFact_BV,Actualizaciones,Fecha_creacion)
select t0.Sociedad,'C'+ t0.U_EX_CLIENTE,t0.Interno,t0.FecInterno,t0.Pedido,t0.FecPed,t0.FecApruCred,t0.FecApruDscto,t0.FecApruExhib,t0.Fact_BV,t0.FecFact_BV,0,GETDATE() from SAP_HOST_TRACKING t0
left join SAP_TRACKING T1 on t0.Interno = T1.Interno and t0.Pedido = T1.Pedido
where HostGroupId = @HostGroupId and (T1.Interno is null and T1.Pedido is null)

update T1 set T1.FecPed = T0.FecPed,T1.FecApruCred = T0.FecApruCred,T1.FecApruDscto = T0.FecApruDscto,T1.FecApruExhib = T0.FecApruExhib,T1.Fact_BV = T0.Fact_BV, T1.FecFact_BV = T0.FecFact_BV,
T1.Actualizaciones = T1.Actualizaciones + 1,T1.Fecha_actualizacion = GETDATE()
from SAP_HOST_TRACKING T0 inner join SAP_TRACKING T1 ON (t0.Interno = T1.Interno and t0.Pedido = T1.Pedido)
commit transaction transaction_tracking
go

USE [SBO_SOLUM_BC]
GO
--exec [dbo].[BZ_CONEP_TRACKING_ARTI] 'C20100310288', '20230520','20230531',''
IF EXISTS (SELECT * FROM sysobjects WHERE name='BZ_CONEP_TRACKING_ARTI') 
BEGIN
	drop procedure [dbo].[BZ_CONEP_TRACKING_ARTI]
END
go  
CREATE procedure [dbo].[BZ_CONEP_TRACKING_ARTI]
@A_CLIENTE nvarchar(30),@A_FECINI nvarchar(10),@A_FECFIN nvarchar(10),@A_PEDIDO nvarchar(30)
as
select '','N°PEDIDO ORIGINAL','PEDIDO ORIGINAL','APROB. CREDITO','APROB. DESCUENTO','APROB. EXHIBIDOR','PEDIDO','CLIENTE','FECHA CREACION','RECIBIDO','PREPARADO','DESPACHADO','ENTREGADO','LIQUIDADO','UBIGEO','TRANSP','PLACA','BULTOS','PESO','GUIA','N°FACTURA','FACTURA BOLETA','OBS.'
SELECT (select case when (sum(DelivrdQty) <> sum(Quantity)) and convert(CHAR(10),D.U_BZ_FECHA,126) <> ''  then '*' else '' END from RDR1 where RDR1.DocEntry = R.DocEntry) AS 'ESTADO',
T4.Pedido,T4.FecInterno,T4.FecApruCred,T4.FecApruDscto,T4.FecApruExhib,
R.NumAtCard AS PEDIDO,R.U_BZ_CODCLI + '-' + r.U_BZ_CLIENTE AS 'Cliente',T4.FecPed,
convert(CHAR(10),R.DocDate,126) + ' ' + dbo.GetTime(R.DocTime,0) 'RECIBIDO',
CASE when R.u_EstadoPacking = 4 then R.U_BZ_CONTAB else '' end 'PREPARADO',
convert(CHAR(10),N.U_BZ_FFINAL,126) AS 'ENVIADO',
convert(CHAR(10),N.U_BZ_TCKFEEN,126) + ' ' + dbo.GetTime(N.U_BZ_TCKHOEN,0) 'ENTREGADO',
convert(CHAR(10),N.U_BZ_TCKFELQ,126) + ' ' + dbo.GetTime(N.U_BZ_TCKHOLQ,0) 'LIQUIDADO',
R.Address2 AS 'UBIGEO',N.U_BZ_TRANSPORTISTA 'TRANSP',N.U_BZ_PLACA 'PLACA',N.U_BZ_BULTOS 'BULTOS',N.U_BZ_PESO_KG 'PESO',N.U_SYP_MDSD + '-' + N.U_SYP_MDCD 'GUIA',T4.Fact_BV,T4.FecFact_BV,R.U_BZ_EVENTUALIDADES 'OBS.'
FROM ORDR R LEFT JOIN ODLN N ON R.CardCode = N.CardCode AND R.NumAtCard = N.NumAtCard LEFT JOIN [@BZ_CARGA] C ON R.U_BZ_CARGA = C.Code LEFT JOIN [@BZ_DETCARGA] D ON D.Code = C.Code
LEFT JOIN SOLUM_EXPRESS.dbo.SAP_TRACKING T4 ON T4.U_EX_CLIENTE COLLATE DATABASE_DEFAULT = R.CardCode and T4.Interno COLLATE DATABASE_DEFAULT = R.NumAtCard
WHERE R.CardCode = @A_CLIENTE AND R.CANCELED <> 'Y' AND RIGHT(r.NumAtCard,9) <> 'CANCELADO' AND isnull(N.CANCELED,'') NOT IN ('Y','C') AND R.NumAtCard like
	case  
		when @A_PEDIDO = '' OR ISNULL(@A_PEDIDO,'') = '' then  '%%' else @A_PEDIDO 
	end
AND R.DocDate BETWEEN (isnull(@A_FECINI,convert(CHAR(10), dateadd(day,-31,getdate()),126))) AND isnull(@A_FECFIN,convert(CHAR(10),getdate(),126)) order by 4 asc
SELECT '','A','A','A','A','A','A','A','A','A','A','A','A','A','A','A','A','N','N','A','A','A','A'
SELECT '','R','R','R','R','R','R','R','R','R','R','R','R','R','R','','','','','','','',''
GO

