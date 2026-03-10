import { useEffect, useState, useCallback } from "react";
import {
  getActiveIncidents,
  getIncidentDetails,
  updateIncidentStatus,
  type IncidentDetailsResponse,
} from "./api/incidents";
import { getAmbulances } from "./api/ambulances";
import { assignAmbulance } from "./api/assign";

import type { Incident } from "./types/incident";
import type { Ambulance } from "./types/ambulance";

import MapView from "./components/MapView";

type ApiErrorResponse =
  | string
  | {
      error?: string;
      title?: string;
      errors?: Record<string, string[]>;
    };

export default function App() {
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [ambulances, setAmbulances] = useState<Ambulance[]>([]);
  const [selectedIncident, setSelectedIncident] = useState<Incident | null>(null);
  const [incidentDetails, setIncidentDetails] = useState<IncidentDetailsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadDashboard = useCallback(async () => {
    try {
      const [incidentData, ambulanceData] = await Promise.all([
        getActiveIncidents(),
        getAmbulances(),
      ]);

      setIncidents(incidentData);
      setAmbulances(ambulanceData);

      if (!selectedIncident && incidentData.length > 0) {
        setSelectedIncident(incidentData[0]);
      }

      if (selectedIncident) {
        const updatedSelected =
          incidentData.find((x) => x.id === selectedIncident.id) ?? null;
        setSelectedIncident(updatedSelected);
      }
    } catch (err) {
      console.error(err);
      setError("Failed to load dashboard");
    } finally {
      setLoading(false);
    }
  }, [selectedIncident]);

  const loadIncidentDetails = useCallback(async () => {
    if (!selectedIncident) {
      setIncidentDetails(null);
      return;
    }

    try {
      const data = await getIncidentDetails(selectedIncident.id);
      setIncidentDetails(data);
    } catch (err) {
      console.error("Failed to load incident details:", err);
    }
  }, [selectedIncident]);

  useEffect(() => {
    loadDashboard();

    const timer = setInterval(loadDashboard, 5000);
    return () => clearInterval(timer);
  }, [loadDashboard]);

  useEffect(() => {
    loadIncidentDetails();

    const timer = setInterval(loadIncidentDetails, 5000);
    return () => clearInterval(timer);
  }, [loadIncidentDetails]);

  function extractApiError(err: unknown) {
    let message = "Action failed";

    if (typeof err === "object" && err !== null && "response" in err) {
      const response = (err as { response?: { data?: ApiErrorResponse } }).response;
      const data = response?.data;

      if (typeof data === "string") {
        message = data;
      } else if (data?.error) {
        message = data.error;
      } else if (data?.title) {
        message = data.title;
      } else if (data?.errors) {
        const firstKey = Object.keys(data.errors)[0];
        if (firstKey && data.errors[firstKey]?.length) {
          message = data.errors[firstKey][0];
        }
      }
    }

    return message;
  }

  async function handleAssign(ambulanceId: string) {
    if (!selectedIncident) return;

    try {
      await assignAmbulance(selectedIncident.id, ambulanceId);
      alert("Ambulance assigned");
      await loadDashboard();
      await loadIncidentDetails();
    } catch (err: unknown) {
      console.error(err);
      alert(extractApiError(err));
    }
  }

  async function handleStatusChange(status: string) {
    if (!selectedIncident) return;

    try {
      await updateIncidentStatus(selectedIncident.id, status);
      alert(`Incident set to ${status}`);
      await loadDashboard();
      await loadIncidentDetails();
    } catch (err: unknown) {
      console.error(err);
      alert(extractApiError(err));
    }
  }

  const standbyAmbulances = ambulances.filter(
    (ambulance) => ambulance.status === "Standby"
  );

  return (
    <div
      style={{
        padding: 24,
        fontFamily: "Arial, sans-serif",
        minHeight: "100vh",
        backgroundColor: "#f7f7f7",
      }}
    >
      <h1 style={{ marginBottom: 20 }}>ESCL8 Dispatcher Dashboard</h1>

      {loading && <p>Loading...</p>}
      {error && <p>{error}</p>}

      {!loading && !error && (
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1.5fr",
            gap: 20,
          }}
        >
          <div
            style={{
              background: "white",
              borderRadius: 10,
              padding: 16,
              border: "1px solid #ddd",
            }}
          >
            <h2>Active Incidents</h2>

            {incidents.length === 0 && <p>No active incidents</p>}

            {incidents.map((incident) => (
              <div
                key={incident.id}
                onClick={() => setSelectedIncident(incident)}
                style={{
                  border:
                    selectedIncident?.id === incident.id
                      ? "2px solid #0078d4"
                      : "1px solid #ccc",
                  padding: 12,
                  marginBottom: 10,
                  borderRadius: 8,
                  cursor: "pointer",
                  backgroundColor:
                    selectedIncident?.id === incident.id ? "#f0f8ff" : "white",
                }}
              >
                <strong>{incident.description}</strong>
                <div>Status: {incident.status}</div>
                <div>
                  Location: {incident.latitude}, {incident.longitude}
                </div>
              </div>
            ))}
          </div>

          <div
            style={{
              background: "white",
              borderRadius: 10,
              padding: 16,
              border: "1px solid #ddd",
            }}
          >
            <h2>Incident Details</h2>

            {!selectedIncident && <p>Select an incident</p>}

            {selectedIncident && (
              <div style={{ marginBottom: 20 }}>
                <p>
                  <strong>ID:</strong> {selectedIncident.id}
                </p>
                <p>
                  <strong>Description:</strong> {selectedIncident.description}
                </p>
                <p>
                  <strong>Status:</strong> {selectedIncident.status}
                </p>
                <p>
                  <strong>Latitude:</strong> {selectedIncident.latitude}
                </p>
                <p>
                  <strong>Longitude:</strong> {selectedIncident.longitude}
                </p>

                <h3 style={{ marginTop: 20 }}>Assigned Ambulance</h3>
                {!incidentDetails?.assignedAmbulance && <p>No ambulance assigned</p>}

                {incidentDetails?.assignedAmbulance && (
                  <div
                    style={{
                      border: "1px solid #ccc",
                      padding: 10,
                      borderRadius: 8,
                      marginBottom: 16,
                    }}
                  >
                    <strong>{incidentDetails.assignedAmbulance.displayName}</strong>
                    <div>Status: {incidentDetails.assignedAmbulance.status}</div>
                    <div>
                      Last Location: {incidentDetails.assignedAmbulance.lastLatitude},{" "}
                      {incidentDetails.assignedAmbulance.lastLongitude}
                    </div>
                  </div>
                )}

                <h3>Status Controls</h3>
                <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginBottom: 20 }}>
                  <button onClick={() => handleStatusChange("EnRoute")}>Set EnRoute</button>
                  <button onClick={() => handleStatusChange("Arrived")}>Set Arrived</button>
                  <button onClick={() => handleStatusChange("Resolved")}>Set Resolved</button>
                  <button onClick={() => handleStatusChange("Cancelled")}>Cancel</button>
                </div>
              </div>
            )}

            <h3>Live Map</h3>
            <MapView incidents={incidents} ambulances={ambulances} />

            <h3 style={{ marginTop: 20 }}>Standby Ambulances</h3>

            {standbyAmbulances.length === 0 && <p>No standby ambulances found</p>}

            {standbyAmbulances.map((ambulance) => (
              <div
                key={ambulance.id}
                style={{
                  border: "1px solid #ccc",
                  padding: 10,
                  marginBottom: 8,
                  borderRadius: 8,
                }}
              >
                <strong>{ambulance.displayName}</strong>
                <div>Status: {ambulance.status}</div>
                <div>
                  Location: {ambulance.lastLatitude}, {ambulance.lastLongitude}
                </div>

                <button
                  style={{
                    marginTop: 8,
                    padding: "6px 10px",
                    cursor: selectedIncident ? "pointer" : "not-allowed",
                    opacity: selectedIncident ? 1 : 0.6,
                  }}
                  disabled={!selectedIncident}
                  onClick={() => handleAssign(ambulance.id)}
                >
                  Assign to Incident
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}