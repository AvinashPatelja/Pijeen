# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
Pijeen is a responsive web application for IoT hardware device control (Field Controller, Gate Controller, Master Controller) via MQTT. Users register/login, then manage and monitor devices through a real-time dashboard with On/Off controls and live telemetry (Voltage, Ampere, RSSI, Fault status).

**Live Connection**: Users can trigger device actions (e.g., turn on motor) → API publishes to MQTT → Device responds with telemetry → API updates DeviceLive/AuditLog → UI refreshes in real-time.

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

## Development Commands

### Backend (.NET Core)
```bash
cd api

# Build & run locally
dotnet build
dotnet run

# Entity Framework migrations (after schema changes)
dotnet ef migrations add MigrationName
dotnet ef database update

# Restore packages
dotnet restore
```

### Frontend (React)
```bash
cd client

# Install dependencies
npm install

# Development server (http://localhost:3000)
npm start

# Build for production
npm run build

# Run tests
npm test
```

### Database
```bash
# Execute on SQL Server (SQL Server Management Studio or sqlcmd)
# Update connection string in api/appsettings.json first
sqlcmd -S server_name -U admin -P password -i database/01_CreateTables.sql
```

## Development Workflow

### Adding a New Device Endpoint
1. Create DTO in `api/Models/DTOs/` (e.g., `AddDeviceRequest.cs`)
2. Add service method in `api/Services/DeviceService.cs`
3. Create controller action in `api/Controllers/DeviceController.cs`
4. Call service, return `Ok()` or `BadRequest()` with response
5. Test via Swagger UI at `http://localhost:5000/swagger`

### Adding a New React Page
1. Create component in `src/pages/`
2. Add route in `App.tsx` using `<Route path="/path" element={<Component />} />`
3. If protected, wrap with `<PrivateRoute>` component
4. Use `apiClient` from `services/api.ts` for API calls
5. Store auth token in localStorage automatically

### Adding MQTT Functionality
- Create `MqttService` in `api/Services/` (singleton)
- Wire up in `Program.cs` with `services.AddSingleton<IMqttService, MqttService>()`
- Inject into controller/service that needs MQTT publish/subscribe
- Handle incoming MQTT messages → update DeviceLive & AuditLog tables

## Architecture & Data Flow

### Authentication Flow
1. **Register**: User submits username/password/email/phone → `AuthService.RegisterAsync()` → Hash password with BCrypt → Save to Users table → Generate JWT token → Return to frontend
2. **Login**: Username/password → `AuthService.LoginAsync()` → Verify BCrypt hash → Generate JWT → Frontend stores in localStorage
3. **Protected Routes**: React `<PrivateRoute>` checks for token, Axios interceptor adds token to all requests as `Authorization: Bearer <token>`

### Device Control Flow
1. **Add Device**: User enters IMEI → `DeviceController.AddDevice()` → Create Devices record → Optionally publish initial MQTT command
2. **Control Device**: User clicks On/Off → `DeviceController.ControlDevice()` → Publish to MQTT `device/{IMEI}/fc/status` → Log action to AuditLog
3. **Hardware Response**: Device publishes to MQTT `devices/{IMEI}/fc/status` → `MqttService` receives → Update DeviceLive table → Trigger UI refresh (polling or WebSocket in future)

### Key Services
- **AuthService** (`api/Services/AuthService.cs`): User registration/login, password hashing
- **MqttService** (to be created): MQTT broker connection, publish/subscribe, message handling
- **DeviceService** (to be created): Device CRUD, device status management
- **apiClient** (`client/src/services/api.ts`): Axios wrapper with JWT token injection

### Database Constraints
- `Users.Username` - UNIQUE
- `Devices.IMEI` - UNIQUE per user
- `DeviceLive.IMEI` - UNIQUE (one live record per device)
- Foreign keys cascade delete for cleanup

## User Types
- **Farmer** - Regular user who owns/operates devices
- **PanchayathMember** - Administrative user at panchayath level
- **Admin** - System administrator (internal only, not shown in registration)

## Security
- Passwords hashed with BCrypt (never stored plain text)
- JWT tokens expire after 24 hours
- Frontend token in localStorage (consider httpOnly cookies in production)
- MQTT broker IP/port in appsettings.json (no credentials exposed in code)

## Deployment Considerations
- Set strong JWT secret in production appsettings
- Use HTTPS in production
- SQL Server connection string should use Windows auth or encrypted passwords
- MQTT broker credentials (if required) should be in environment variables
- React env vars loaded from `.env` file (never commit sensitive vars)

## Next Steps
1. Create `MqttService` for broker communication
2. Create `DeviceService` and `DeviceController` for device management
3. Implement Field Controller (FC) add/control UI with IMEI input
4. Add real-time device data polling/display
5. Extend for Gate Controller (GC) and Master Controller (MC) device types
