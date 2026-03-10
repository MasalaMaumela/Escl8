import axios from "axios";

const baseURL = "https://localhost:7241/api/v1";

export const dispatcherHttp = axios.create({
  baseURL,
  timeout: 15000,
  headers: {
    "X-API-KEY": "dev-dispatcher-key-123",
  },
});

export const ambulanceHttp = axios.create({
  baseURL,
  timeout: 15000,
  headers: {
    "X-API-KEY": "dev-ambulance-key-456",
  },
});