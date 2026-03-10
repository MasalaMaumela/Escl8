# ESCL8 – Emergency Escalation & Ambulance Dispatch System 

ESCL8 is a real-time emergency escalation and ambulance dispatch platform designed to help emergency response teams coordinate incidents, track ambulances, and manage response workflows efficiently.

This project demonstrates a full-stack system architecture with a .NET backend and a React dispatcher dashboard.

Core Features

 Incident Management
- Create and track emergency incidents
- View active incidents in a dispatcher dashboard
- Manage incident lifecycle

Ambulance Management
- Register ambulances
- Track ambulance status
- Track ambulance locations

Dispatcher Dashboard
- View active incidents
- Assign ambulances to incidents
- Monitor incident details
- View ambulances on a live map

Incident Workflow

Open → Assigned → EnRoute → Arrived → Resolved

Real-Time Map
- Displays incidents
- Displays ambulance locations
- Built using Leaflet + OpenStreetMap

---

System Architecture

Dispatcher Dashboard (React + TypeScript)  
↓  
.NET 8 REST API  
↓  
PostgreSQL Database  

Technology Stack

Backend
- .NET 8 Web API
- Entity Framework Core
- PostgreSQL
- Swagger API Documentation

Frontend
- React
- TypeScript
- Vite
- Axios
- Leaflet Maps
- OpenStreetMap

Security

The API uses API key authentication.

Two roles exist:

Dispatcher API Key  
Used for incident assignment and dispatcher actions.

Ambulance API Key  
Used for ambulance status and location updates.

Key API Endpoints

Incidents

POST /api/v1/incidents  
GET /api/v1/incidents  
GET /api/v1/incidents/active  
GET /api/v1/incidents/{id}/details  
POST /api/v1/incidents/{id}/assign  
POST /api/v1/incidents/{id}/reassign  
POST /api/v1/incidents/{id}/status  

Ambulances

POST /api/v1/ambulances  
GET /api/v1/ambulances  
POST /api/v1/ambulances/{id}/status  
POST /api/v1/ambulances/{id}/location  

Dispatcher Dashboard

The dispatcher dashboard allows operators to:

- View active incidents
- Select incidents
- View incident details
- See ambulances on the map
- Assign ambulances to incidents

Future Improvements

Planned features include:

- Automatic nearest ambulance dispatch
- Real-time GPS streaming
- Mobile ambulance tracking app
- Multi-company EMS support
- Incident analytics dashboard
- SMS or push notifications

Author

Masala Maumela

Software Developer  
Full Stack Systems Builder

Project Purpose

This project was built to demonstrate the design and implementation of a real-world emergency response coordination system including backend services, a dispatcher interface, and location-based tracking.
