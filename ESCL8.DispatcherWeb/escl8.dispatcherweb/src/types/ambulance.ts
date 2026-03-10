export type Ambulance = {
  id: string;
  companyId: string;
  isPublic: boolean;
  displayName: string;
  status: string;
  lastLatitude: number | null;
  lastLongitude: number | null;
  lastSeenUtc: string | null;
};