import { dispatcherHttp } from "./http";
import type { Incident } from "../types/incident";
import type { Ambulance } from "../types/ambulance";

type DotNetList<T> = {
  $values?: T[];
};

export type LocationPing = {
  id?: string;
  incidentId: string;
  ambulanceId: string;
  latitude: number;
  longitude: number;
  timestampUtc: string;
};

export type IncidentTimeline = {
  createdUtc?: string | null;
  assignedUtc?: string | null;
  enRouteUtc?: string | null;
  arrivedUtc?: string | null;
  resolvedUtc?: string | null;
  cancelledUtc?: string | null;
};

export type IncidentDetailsResponse = {
  incident: Incident;
  assignedAmbulance: Ambulance | null;
  latestLocation: LocationPing | null;
  timeline: IncidentTimeline;
};

export async function getActiveIncidents(): Promise<Incident[]> {
  const res = await dispatcherHttp.get<Incident[] | DotNetList<Incident>>("/incidents/active");

  if (Array.isArray(res.data)) {
    return res.data;
  }

  if (res.data && typeof res.data === "object" && "$values" in res.data) {
    return res.data.$values ?? [];
  }

  return [];
}

export async function getIncidentDetails(
  incidentId: string
): Promise<IncidentDetailsResponse> {
  const res = await dispatcherHttp.get<IncidentDetailsResponse>(
    `/incidents/${incidentId}/details`
  );

  return res.data;
}

export async function updateIncidentStatus(
  incidentId: string,
  status: string
) {
  const res = await dispatcherHttp.post(`/incidents/${incidentId}/status`, {
    status,
  });

  return res.data;
}