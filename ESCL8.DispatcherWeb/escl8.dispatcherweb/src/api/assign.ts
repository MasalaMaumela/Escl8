import { dispatcherHttp } from "./http";

export async function assignAmbulance(
  incidentId: string,
  ambulanceId: string
) {
  const res = await dispatcherHttp.post(`/incidents/${incidentId}/assign`, {
    ambulanceId: ambulanceId,
  });

  return res.data;
}