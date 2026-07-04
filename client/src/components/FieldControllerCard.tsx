import React, { useState } from 'react';
import deviceAPI from '../services/deviceApi';

interface DeviceData {
  deviceId: number;
  imei: string;
  deviceType: string;
  deviceName: string;
  location?: string;
  isActive: boolean;
  liveStatus?: {
    status: string;
    voltage?: number;
    ampere?: number;
    rssi?: number;
    faultReason?: string;
    lastUpdatedAt: string;
  };
}

interface FieldControllerCardProps {
  device: DeviceData;
  onRefresh: () => void;
}

const FieldControllerCard: React.FC<FieldControllerCardProps> = ({ device, onRefresh }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [showDetails, setShowDetails] = useState(false);

  const status = device.liveStatus?.status || 'UNKNOWN';
  const isOn = status === 'ON';

  const handleControl = async (newStatus: 'ON' | 'OFF') => {
    setIsLoading(true);
    setError('');

    try {
      await deviceAPI.controlDevice(device.deviceId, newStatus);
      onRefresh();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to control device');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-lg overflow-hidden hover:shadow-xl transition">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-500 to-blue-600 p-4 text-white">
        <h3 className="text-lg font-bold">{device.deviceName}</h3>
        <p className="text-sm text-blue-100">IMEI: {device.imei}</p>
      </div>

      {/* Image Section */}
      <div className="bg-gray-100 p-6 flex justify-center items-center h-48">
        <div className="text-center">
          {/* Placeholder for 3PhaseMotor image */}
          <div className="text-6xl mb-3">⚙️</div>
          <p className="text-gray-600 text-sm">3-Phase Motor</p>
        </div>
      </div>

      {/* Status Section */}
      <div className="p-6">
        {/* Current Status */}
        <div className="mb-6">
          <p className="text-gray-600 text-sm mb-2">Current Status</p>
          <div
            className={`inline-block px-4 py-2 rounded-full text-white font-semibold ${
              isOn ? 'bg-green-500' : 'bg-red-500'
            }`}
          >
            {isOn ? '🟢 ON' : '🔴 OFF'}
          </div>
        </div>

        {/* Control Buttons */}
        <div className="grid grid-cols-2 gap-3 mb-6">
          <button
            onClick={() => handleControl('ON')}
            disabled={isLoading || isOn}
            className={`py-2 rounded-lg font-semibold transition ${
              isOn
                ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                : 'bg-green-500 text-white hover:bg-green-600'
            }`}
          >
            {isLoading ? 'Processing...' : 'Turn ON'}
          </button>
          <button
            onClick={() => handleControl('OFF')}
            disabled={isLoading || !isOn}
            className={`py-2 rounded-lg font-semibold transition ${
              !isOn
                ? 'bg-gray-200 text-gray-500 cursor-not-allowed'
                : 'bg-red-500 text-white hover:bg-red-600'
            }`}
          >
            {isLoading ? 'Processing...' : 'Turn OFF'}
          </button>
        </div>

        {error && <div className="bg-red-100 border border-red-400 text-red-700 px-3 py-2 rounded mb-4 text-sm">{error}</div>}

        {/* Details Toggle */}
        <button
          onClick={() => setShowDetails(!showDetails)}
          className="w-full text-blue-600 hover:text-blue-700 font-semibold text-sm py-2"
        >
          {showDetails ? '▼ Hide Details' : '▶ Show Details'}
        </button>

        {/* Telemetry Details */}
        {showDetails && device.liveStatus && (
          <div className="mt-4 pt-4 border-t border-gray-200 space-y-2">
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Voltage:</span>
              <span className="font-semibold">{device.liveStatus.voltage ? `${device.liveStatus.voltage}V` : 'N/A'}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Current:</span>
              <span className="font-semibold">{device.liveStatus.ampere ? `${device.liveStatus.ampere}A` : 'N/A'}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Signal (RSSI):</span>
              <span className={`font-semibold ${device.liveStatus.rssi ? (device.liveStatus.rssi > 50 ? 'text-green-600' : 'text-yellow-600') : ''}`}>
                {device.liveStatus.rssi ? `${device.liveStatus.rssi}dBm` : 'N/A'}
              </span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Fault:</span>
              <span className={`font-semibold ${device.liveStatus.faultReason === 'NONE' ? 'text-green-600' : 'text-red-600'}`}>
                {device.liveStatus.faultReason || 'NONE'}
              </span>
            </div>
            <div className="flex justify-between text-xs text-gray-500 pt-2">
              <span>Updated:</span>
              <span>{new Date(device.liveStatus.lastUpdatedAt).toLocaleTimeString()}</span>
            </div>
          </div>
        )}

        {/* Device Info Footer */}
        <div className="mt-4 pt-4 border-t border-gray-200 text-xs text-gray-500">
          {device.location && <p>📍 {device.location}</p>}
          <p>Device Type: {device.deviceType}</p>
        </div>
      </div>
    </div>
  );
};

export default FieldControllerCard;
