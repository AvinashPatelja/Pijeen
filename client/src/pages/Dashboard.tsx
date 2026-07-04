import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  const [activeMenu, setActiveMenu] = useState('overview');

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
          <h1 className="text-3xl font-bold text-blue-600">Pijeen</h1>
          <div className="flex items-center gap-4">
            <span className="text-gray-700">Welcome, {user.username}</span>
            <button
              onClick={handleLogout}
              className="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700"
            >
              Logout
            </button>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Sidebar */}
        <aside className="w-64 bg-white shadow-lg min-h-screen">
          <nav className="p-6 space-y-4">
            <h2 className="text-lg font-semibold text-gray-800 mb-6">Workspace</h2>

            <button
              onClick={() => setActiveMenu('field-controller')}
              className={`w-full text-left px-4 py-2 rounded-lg transition ${
                activeMenu === 'field-controller'
                  ? 'bg-blue-600 text-white'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              🚜 Field Controller
            </button>

            <button
              onClick={() => setActiveMenu('gate-controller')}
              className={`w-full text-left px-4 py-2 rounded-lg transition ${
                activeMenu === 'gate-controller'
                  ? 'bg-blue-600 text-white'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              🚪 Gate Controller
            </button>

            <button
              onClick={() => setActiveMenu('master-controller')}
              className={`w-full text-left px-4 py-2 rounded-lg transition ${
                activeMenu === 'master-controller'
                  ? 'bg-blue-600 text-white'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              ⚙️ Master Controller
            </button>
          </nav>
        </aside>

        {/* Main Content */}
        <main className="flex-1 p-8">
          {activeMenu === 'field-controller' && (
            <div className="bg-white rounded-lg shadow-lg p-8">
              <h2 className="text-2xl font-bold text-gray-800 mb-6">Field Controller</h2>
              <p className="text-gray-600 mb-6">
                Manage and monitor your field controllers here. Add new devices, control their status, and view real-time data.
              </p>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center hover:border-blue-500 cursor-pointer transition">
                  <div className="text-4xl mb-4">➕</div>
                  <h3 className="text-lg font-semibold text-gray-700">Add Field Controller</h3>
                  <p className="text-gray-500 text-sm mt-2">Register a new device using IMEI</p>
                </div>
              </div>

              <p className="text-sm text-gray-500 mt-8">
                Feature coming soon - Add and manage your devices here
              </p>
            </div>
          )}

          {activeMenu === 'gate-controller' && (
            <div className="bg-white rounded-lg shadow-lg p-8">
              <h2 className="text-2xl font-bold text-gray-800 mb-6">Gate Controller</h2>
              <p className="text-gray-600">Gate Controller feature coming soon...</p>
            </div>
          )}

          {activeMenu === 'master-controller' && (
            <div className="bg-white rounded-lg shadow-lg p-8">
              <h2 className="text-2xl font-bold text-gray-800 mb-6">Master Controller</h2>
              <p className="text-gray-600">Master Controller feature coming soon...</p>
            </div>
          )}
        </main>
      </div>
    </div>
  );
};

export default Dashboard;
