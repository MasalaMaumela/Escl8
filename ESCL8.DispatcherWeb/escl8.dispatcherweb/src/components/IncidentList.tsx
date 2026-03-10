import type { Incident } from "../types/incident"

type Props = {
  incidents: Incident[]
  selected: Incident | null
  onSelect: (incident: Incident) => void
}

export default function IncidentList({ incidents, selected, onSelect }: Props) {

  return (
    <div>

      <h2>Active Incidents</h2>

      {incidents.length === 0 && <p>No active incidents</p>}

      {incidents.map((incident) => (

        <div
          key={incident.id}
          onClick={() => onSelect(incident)}
          style={{
            border: selected?.id === incident.id ? "2px solid #0078d4" : "1px solid #ccc",
            padding: 12,
            marginBottom: 10,
            borderRadius: 8,
            cursor: "pointer"
          }}
        >
          <strong>{incident.description}</strong>

          <div>Status: {incident.status}</div>

          <div>
            {incident.latitude}, {incident.longitude}
          </div>

        </div>

      ))}

    </div>
  )
}