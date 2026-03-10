import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import type { LatLngExpression } from "leaflet";
import type { Incident } from "../types/incident";
import type { Ambulance } from "../types/ambulance";

type Props = {
  incidents: Incident[];
  ambulances: Ambulance[];
};

export default function MapView({ incidents, ambulances }: Props) {
  const center: LatLngExpression = [-26.2041, 28.0473];

  return (
    <MapContainer
      center={center}
      zoom={11}
      style={{ height: "400px", width: "100%", borderRadius: "10px" }}
    >
      <TileLayer
        attribution="© OpenStreetMap contributors"
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      {incidents.map((incident) => {
        if (incident.latitude === null || incident.longitude === null) {
          return null;
        }

        const position: LatLngExpression = [
          incident.latitude,
          incident.longitude,
        ];

        return (
          <Marker key={`incident-${incident.id}`} position={position}>
            <Popup>
              <strong>Incident</strong>
              <br />
              {incident.description}
              <br />
              Status: {incident.status}
            </Popup>
          </Marker>
        );
      })}

      {ambulances.map((ambulance) => {
        if (ambulance.lastLatitude === null || ambulance.lastLongitude === null) {
          return null;
        }

        const position: LatLngExpression = [
          ambulance.lastLatitude,
          ambulance.lastLongitude,
        ];

        return (
          <Marker key={`ambulance-${ambulance.id}`} position={position}>
            <Popup>
              <strong>{ambulance.displayName}</strong>
              <br />
              Status: {ambulance.status}
            </Popup>
          </Marker>
        );
      })}
    </MapContainer>
  );
}