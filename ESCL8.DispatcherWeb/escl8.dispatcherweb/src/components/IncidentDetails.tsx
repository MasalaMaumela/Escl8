import type { Incident } from "../types/incident"

type Props = {
  incident: Incident | null
}

export default function IncidentDetails({ incident }: Props) {

  if (!incident) {
    return <p>Select an incident</p>
  }

  return (
    <div>

      <h2>Incident Details</h2>

      <p><strong>ID:</strong> {incident.id}</p>

      <p><strong>Description:</strong> {incident.description}</p>

      <p><strong>Status:</strong> {incident.status}</p>

      <p>
        <strong>Location:</strong> {incident.latitude}, {incident.longitude}
      </p>

    </div>
  )
}