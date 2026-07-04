import React, { useState, useEffect } from 'react';
import deviceAPI from '../services/deviceApi';
import FieldControllerCard from '../components/FieldControllerCard';

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

const FieldController: React.FC = () => {
  const [showAddForm, setShowAddForm] = useState(false);
  const [devices, setDevices] = useState<DeviceData[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Form states
  const [imei, setImei] = useState('');
  const [deviceName, setDeviceName] = useState('');
  const [location, setLocation] = useState('');
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState('');

  const fetchDevices = async () => {
    try {
      setLoading(true);
      const response = await deviceAPI.getUserDevices();
      // Filter only Field Controllers (FC)
      const fieldControllers = response.data.filter((d: any) => d.deviceType === 'FC');
      setDevices(fieldControllers);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load devices');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDevices();
    // Auto-refresh every 30 seconds
    const interval = setInterval(fetchDevices, 30000);
    return () => clearInterval(interval);
  }, []);

  const handleAddDevice = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');
    setSuccess('');
    setFormLoading(true);

    try {
      // Validate IMEI format (basic validation)
      if (imei.length < 10 || !/^\d+$/.test(imei)) {
        setFormError('Please enter a valid IMEI (10+ digits)');
        setFormLoading(false);
        return;
      }

      const response = await deviceAPI.addDevice({
        IMEI: imei,
        DeviceType: 'FC',
        DeviceName: deviceName || `FC-${imei}`,
        Location: location,
      });

      if (response.data.success) {
        setSuccess(`Device added successfully! IMEI: ${imei}`);
        setImei('');
        setDeviceName('');
        setLocation('');
        setShowAddForm(false);
        await fetchDevices();
      }
    } catch (err: any) {
      setFormError(err.response?.data?.message || 'Failed to add device');
    } finally {
      setFormLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-bold text-gray-800">Field Controller</h2>
          <p className="text-gray-600 mt-1">Manage and monitor your field controllers</p>
        </div>
        <button
          onClick={() => setShowAddForm(!showAddForm)}
          className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition font-semibold"
        >
          {showAddForm ? '✕ Cancel' : '➕ Add New Device'}
        </button>
      </div>

      {/* Add Device Form */}
      {showAddForm && (
        <div className="bg-white rounded-lg shadow-lg p-6">
          <h3 className="text-xl font-bold text-gray-800 mb-4">Register New Field Controller</h3>

          <form onSubmit={handleAddDevice} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Device IMEI * (Required)
              </label>
              <input
                type="text"
                value={imei}
                onChange={(e) => setImei(e.target.value)}
                placeholder="e.g., 123123123123"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              />
              <p className="text-xs text-gray-500 mt-1">International Mobile Equipment Identity (10+ digits)</p>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Device Name (Optional)
                </label>
                <input
                  type="text"
                  value={deviceName}
                  onChange={(e) => setDeviceName(e.target.value)}
                  placeholder="e.g., Field-1"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Location (Optional)
                </label>
                <input
                  type="text"
                  value={location}
                  onChange={(e) => setLocation(e.target.value)}
                  placeholder="e.g., Field A, North Plot"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            {formError && (
              <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
                {formError}
              </div>
            )}

            {success && (
              <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded">
                {success}
              </div>
            )}

            <button
              type="submit"
              disabled={formLoading}
              className="w-full bg-blue-600 text-white py-3 rounded-lg hover:bg-blue-700 transition font-semibold disabled:opacity-50"
            >
              {formLoading ? 'Adding Device...' : 'Register Device'}
            </button>
          </form>
        </div>
      )}

      {/* Devices Grid */}
      {loading ? (
        <div className="text-center py-12">
          <p className="text-gray-600">Loading devices...</p>
        </div>
      ) : error ? (
        <div className="bg-red-100 border border-red-400 text-red-700 px-6 py-4 rounded-lg">
          {error}
        </div>
      ) : devices.length === 0 ? (
        <div className="bg-gray-100 rounded-lg p-12 text-center">
          <div className="text-4xl mb-4">🚜</div>
          <h3 className="text-xl font-semibold text-gray-700 mb-2">No Devices Yet</h3>
          <p className="text-gray-600 mb-6">Register your first field controller to get started</p>
          <button
            onClick={() => setShowAddForm(true)}
            className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700"
          >
            Register Device Now
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {devices.map((device) => (
            <FieldControllerCard
              key={device.deviceId}
              device={device}
              onRefresh={fetchDevices}
            />
          ))}
        </div>
      )}

      {/* Refresh Info */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 text-sm text-blue-800">
        <p>💡 Device data refreshes automatically every 30 seconds. You can also use the On/Off buttons to control devices in real-time.</p>
      </div>
    </div>
  );
};

export default FieldController;
