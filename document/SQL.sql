CREATE DATABASE CHYAuth
GO
USE CHYAuth
GO
DROP TABLE Client
GO
CREATE TABLE Client(
ClientId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
ClientIdentifier NVARCHAR(50) NOT NULL,
ClientSecret NVARCHAR(50) NULL,
Callback NVARCHAR(4000) NULL,
Name NVARCHAR(4000) NULL,
ClientType INT NOT NULL
)
DROP TABLE ClientAuthorization
GO
CREATE TABLE ClientAuthorization(
AuthorizationId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
CreatedOnUtc DATETIME NOT NULL,
ClientId INT NOT NULL,
UserId INT NULL,
Scope NVARCHAR(max) NULL,
ExpirationDateUtc DATETIME NULL
)

CREATE TABLE Nonce(
Context NVARCHAR(500) NOT NULL PRIMARY KEY,
Code NVARCHAR(4000) NOT NULL,
Timestamp DATETIME NOT NULL
)

CREATE TABLE SymmetricCryptoKey(
Bucket NVARCHAR(500) NOT NULL PRIMARY KEY,
Handle NVARCHAR(4000) NOT NULL,
ExpiresUtc DATETIME NOT NULL,
Secret VARBINARY(8000) NOT null
)

DROP TABLE [User]
GO
CREATE TABLE [User](
UserId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
OpenIDClaimedIdentifier NVARCHAR(150) NOT NULL,
OpenIDFriendlyIdentifier NVARCHAR(150) null
)
GO

INSERT INTO dbo.Client
        ( 
          ClientIdentifier ,
          ClientSecret ,
          Callback ,
          Name ,
          ClientType
        )
VALUES  ( 
          N'sampleconsumer' , -- ClientIdentifier - nvarchar(50)
          N'samplesecret' , -- ClientSecret - nvarchar(50)
          N'' , -- Callback - nvarchar(4000)
          N'some sample client' , -- Name - nvarchar(4000)
          0  -- ClientType - int
        )

INSERT INTO dbo.Client
        ( 
          ClientIdentifier ,
          ClientSecret ,
          Callback ,
          Name ,
          ClientType
        )
VALUES  ( 
          N'sampleImplicitConsumer' , -- ClientIdentifier - nvarchar(50)
          N'' , -- ClientSecret - nvarchar(50)
          N'http://localhost:10000' , -- Callback - nvarchar(4000)
          N'Some sample client used for implicit grants(no secret)' , -- Name - nvarchar(4000)
          0  -- ClientType - int
        )