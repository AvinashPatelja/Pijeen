-- Users Table
CREATE TABLE [dbo].[Users] (
    [UserId] INT IDENTITY(1,1) PRIMARY KEY,
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [Password] NVARCHAR(255) NOT NULL,
    [Email] NVARCHAR(150),
    [PhoneNumber] NVARCHAR(20),
    [UserType] NVARCHAR(50) NOT NULL, -- 'Farmer', 'PanchayathMember', 'Admin'
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Devices Table (Field Controller, Gate Controller, Master Controller)
CREATE TABLE [dbo].[Devices] (
    [DeviceId] INT IDENTITY(1,1) PRIMARY KEY,
    [IMEI] NVARCHAR(50) NOT NULL UNIQUE,
    [DeviceType] NVARCHAR(10) NOT NULL, -- 'FC' (Field Controller), 'GC' (Gate Controller), 'MC' (Master Controller)
    [UserId] INT NOT NULL,
    [DeviceName] NVARCHAR(100),
    [Location] NVARCHAR(255),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId])
);

-- DeviceLive Table (Latest device status)
CREATE TABLE [dbo].[DeviceLive] (
    [DeviceLiveId] INT IDENTITY(1,1) PRIMARY KEY,
    [DeviceId] INT NOT NULL,
    [IMEI] NVARCHAR(50) NOT NULL,
    [DeviceType] NVARCHAR(10) NOT NULL,
    [Status] NVARCHAR(20), -- 'ON', 'OFF'
    [Voltage] FLOAT,
    [Ampere] FLOAT,
    [RSSI] INT, -- Signal strength
    [FaultReason] NVARCHAR(255),
    [Valve] INT DEFAULT 0, -- For FC: 0,0 by default
    [LastUpdatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([DeviceId]) REFERENCES [dbo].[Devices]([DeviceId]) ON DELETE CASCADE,
    UNIQUE ([IMEI])
);

-- AuditLog Table (History of all device status changes)
CREATE TABLE [dbo].[AuditLog] (
    [AuditLogId] INT IDENTITY(1,1) PRIMARY KEY,
    [DeviceId] INT NOT NULL,
    [IMEI] NVARCHAR(50) NOT NULL,
    [DeviceType] NVARCHAR(10) NOT NULL,
    [Status] NVARCHAR(20),
    [Voltage] FLOAT,
    [Ampere] FLOAT,
    [RSSI] INT,
    [FaultReason] NVARCHAR(255),
    [Valve] INT,
    [ActionType] NVARCHAR(50), -- 'UserCommand', 'HardwareUpdate', 'System'
    [ActionBy] INT, -- UserId who triggered the action
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([DeviceId]) REFERENCES [dbo].[Devices]([DeviceId]) ON DELETE CASCADE,
    FOREIGN KEY ([ActionBy]) REFERENCES [dbo].[Users]([UserId])
);

-- Indexes for better query performance
CREATE INDEX [IX_Users_Username] ON [dbo].[Users]([Username]);
CREATE INDEX [IX_Devices_UserId] ON [dbo].[Devices]([UserId]);
CREATE INDEX [IX_Devices_IMEI] ON [dbo].[Devices]([IMEI]);
CREATE INDEX [IX_DeviceLive_IMEI] ON [dbo].[DeviceLive]([IMEI]);
CREATE INDEX [IX_DeviceLive_DeviceId] ON [dbo].[DeviceLive]([DeviceId]);
CREATE INDEX [IX_AuditLog_DeviceId] ON [dbo].[AuditLog]([DeviceId]);
CREATE INDEX [IX_AuditLog_CreatedAt] ON [dbo].[AuditLog]([CreatedAt]);
