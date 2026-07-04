import apiClient from './api';

export const deviceAPI = {
  addDevice: (data: {
    IMEI: string;
    DeviceType: string;
    DeviceName?: string;
    Location?: string;
  }) => apiClient.post('/device/add', data),

  getUserDevices: () => apiClient.get('/device/user/devices'),

  getDevice: (deviceId: number) => apiClient.get(`/device/${deviceId}`),

  controlDevice: (deviceId: number, status: 'ON' | 'OFF') =>
    apiClient.post(`/device/${deviceId}/control`, { status }),

  getAuditLogs: (deviceId: number, limit: number = 50) =>
    apiClient.get(`/device/${deviceId}/audit-logs`, { params: { limit } }),
};

export default deviceAPI;
