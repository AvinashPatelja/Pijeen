import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import FieldController from './FieldController';

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  const [activeMenu, setActiveMenu] = useState('field-controller');

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
          {activeMenu === 'field-controller' && <FieldController />}

          {activeMenu === 'gate-controller' && (
            <div>
              <h2 className="text-3xl font-bold text-gray-800 mb-6">Gate Controller</h2>
              <div className="bg-gray-100 rounded-lg p-12 text-center">
                <div className="text-6xl mb-4">🚪</div>
                <p className="text-gray-600 text-lg">Gate Controller feature coming soon...</p>
              </div>
            </div>
          )}

          {activeMenu === 'master-controller' && (
            <div>
              <h2 className="text-3xl font-bold text-gray-800 mb-6">Master Controller</h2>
              <div className="bg-gray-100 rounded-lg p-12 text-center">
                <div className="text-6xl mb-4">⚙️</div>
                <p className="text-gray-600 text-lg">Master Controller feature coming soon...</p>
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
};

export default Dashboard;
