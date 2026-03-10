import { ambulanceHttp } from "./http";
import type { Ambulance } from "../types/ambulance";

type DotNetList<T> = {
  $values?: T[];
};

export async function getAmbulances(): Promise<Ambulance[]> {
  const res = await ambulanceHttp.get<Ambulance[] | DotNetList<Ambulance>>("/ambulances");

  if (Array.isArray(res.data)) {
    return res.data;
  }

  if (res.data && typeof res.data === "object" && "$values" in res.data) {
    return res.data.$values ?? [];
  }

  return [];
}