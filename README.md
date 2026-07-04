# Pijeen - Hardware Control System

A responsive web application for controlling and monitoring IoT hardware devices (Field Controller, Gate Controller, Master Controller) using MQTT protocol.

## Features

- 🔐 Secure user authentication with JWT
- 🏠 Responsive dashboard interface
- 📱 Real-time device monitoring via MQTT
- 💾 SQLServer database with audit logging
- 🎨 Modern UI with Tailwind CSS
- ⚡ .NET Core API with Entity Framework Core

## Tech Stack

- **Backend**: .NET Core 8, Entity Framework Core, MQTTnet
- **Frontend**: React 18, TypeScript, Tailwind CSS
- **Database**: Microsoft SQL Server
- **MQTT Broker**: broker.emqx.io

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQL Server connection
- MQTT Broker access

### Backend Setup
```bash
cd api
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend Setup
```bash
cd client
npm install
npm start
```

## Project Structure
- `/api` - .NET Core API backend
- `/client` - React frontend
- `/database` - SQL scripts
- `/Images` - Project assets

## Documentation
See `CLAUDE.md` for detailed architecture and implementation notes.

## License
Proprietary - Pijeen Project
