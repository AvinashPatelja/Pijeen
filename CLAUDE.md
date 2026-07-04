# Pijeen - Hardware Control System

## Project Overview
Pijeen is a responsive web application for communicating with hardware devices (Field Controller, Gate Controller, Master Controller) using MQTT protocol. It provides real-time monitoring and control of IoT devices through an intuitive web interface.

## Tech Stack

### Backend
- **Framework**: .NET Core 8
- **Database**: Microsoft SQL Server (AWS RDS)
- **Authentication**: JWT (JSON Web Tokens)
- **MQTT**: MQTTnet library
- **ORM**: Entity Framework Core

### Frontend
- **Framework**: React 18
- **Styling**: Tailwind CSS
- **Routing**: React Router v6
- **HTTP Client**: Axios
- **Language**: TypeScript

### Infrastructure
- **MQTT Broker**: broker.emqx.io:1883
- **Database**: Server=workorder.cobqxubuodey.us-east-1.rds.amazonaws.com,1433;Database=DH_Automation

## Project Structure

```
Pijeen/
├── api/                          # .NET Core API
│   ├── Models/                   # Data models (User, Device, DeviceLive, AuditLog)
│   ├── Controllers/              # API endpoints (Auth, Devices, etc.)
│   ├── Services/                 # Business logic (AuthService, MQTT Service)
│   ├── Data/                     # EF Core DbContext
│   ├── Utilities/                # Helper classes (JwtTokenGenerator)
│   ├── Pijeen.API.csproj
│   └── Program.cs               # Configuration and startup
│
├── client/                       # React Frontend
│   ├── public/                   # Static files
│   ├── src/
│   │   ├── pages/               # Page components (Login, Register, Dashboard)
│   │   ├── components/          # Reusable components (PrivateRoute)
│   │   ├── services/            # API service (axios client)
│   │   ├── App.tsx              # Main app with routing
│   │   └── index.tsx
│   ├── package.json
│   └── tailwind.config.js
│
├── database/                     # Database scripts
│   └── 01_CreateTables.sql      # Initial schema
│
├── Images/                       # Project assets (logos, device images)
└── CLAUDE.md                     # This file

```

## Database Schema

### Tables
1. **Users** - User authentication and profile
   - UserId, Username, Password (hashed), Email, PhoneNumber, UserType, IsActive, CreatedAt, UpdatedAt

2. **Devices** - Registered IoT devices
   - DeviceId, IMEI (unique), DeviceType (FC/GC/MC), UserId, DeviceName, Location, IsActive, CreatedAt, UpdatedAt

3. **DeviceLive** - Latest device status (one record per device)
   - DeviceLiveId, DeviceId, IMEI, DeviceType, Status, Voltage, Ampere, RSSI, FaultReason, Valve, LastUpdatedAt

4. **AuditLog** - Historical log of all device status changes
   - AuditLogId, DeviceId, IMEI, DeviceType, Status, Voltage, Ampere, RSSI, FaultReason, Valve, ActionType, ActionBy, CreatedAt

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login with credentials

### Response Format
```json
{
  "success": true,
  "message": "string",
  "token": "jwt_token_here",
  "user": {
    "userId": 1,
    "username": "farmer1",
    "email": "farmer@example.com",
    "userType": "Farmer"
  }
}
```

## MQTT Topics and Payloads

### Publishing (API → Device)
**Topic**: `device/{IMEI}/fc/status`
```json
{
  "deviceType": "FC",
  "imei": "123123123123",
  "Valve": 0,
  "status": "ON"
}
```

### Subscribing (Device → API)
**Topic**: `devices/{IMEI}/fc/status`
```json
{
  "deviceType": "FC",
  "imei": "123123123123",
  "status": "ON",
  "Voltage": 300,
  "Ampere": 20,
  "rssi": 19,
  "faultReason": "NONE"
}
```

## Key Features (Current Phase)

✅ **Phase 1: Authentication**
- User Registration (Username, Password, Email, PhoneNumber, UserType)
- User Login with JWT token
- Password hashing with BCrypt
- Token-based authorization

✅ **Frontend UI**
- Login page with validation
- Registration page with user type selection
- Dashboard with workspace sidebar
- Responsive design with Tailwind CSS

🚀 **Next Phase: Field Controller Management**
- Add Field Controller with IMEI
- Display device status with 3PhaseMotor image
- On/Off control button
- Real-time device data display
- MQTT communication for device control

## Configuration Files

### Backend Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=workorder.cobqxubuodey.us-east-1.rds.amazonaws.com,1433;..."
  },
  "Jwt": {
    "Secret": "your_secret_key",
    "Issuer": "Pijeen",
    "Audience": "PijeenUsers",
    "ExpiryMinutes": 1440
  },
  "Mqtt": {
    "Broker": "broker.emqx.io",
    "Port": 1883,
    "ClientId": "PijeenAPI"
  }
}
```

### Frontend Environment (.env)
```
REACT_APP_API_URL=http://localhost:5000/api
```

## Setup Instructions

### Backend Setup
```bash
cd api
dotnet restore
dotnet ef database update  # Apply migrations
dotnet run
```

### Frontend Setup
```bash
cd client
npm install
npm start
```

### Database Setup
Run the SQL scripts in `database/01_CreateTables.sql` on the MS-SQL database.

## User Types
- **Farmer** - Regular user who owns/operates devices
- **PanchayathMember** - Administrative user at panchayath level
- **Admin** - System administrator (internal only, not shown in registration)

## Security Notes
- Passwords are hashed using BCrypt
- JWT tokens expire after 24 hours (configurable)
- API requires Bearer token for authenticated endpoints
- CORS is enabled for frontend origin only
- SQL Server uses encrypted connection string

## Next Steps
1. Implement Field Controller registration with IMEI input
2. Create MQTT service for device communication
3. Build device control UI with On/Off toggle
4. Implement real-time data updates with signal strength
5. Add data visualization and charts
6. Implement Gate Controller and Master Controller features
