import React, { useState, useEffect } from 'react';
import { fetcher } from '../utils/fetcher'; // Adjust path if needed
import { DashboardData } from '../types'; // Adjust path if needed
import { Card } from '../components/Card'; // Adjust path if needed
import { MaterialProgressBar } from '../components/MaterialProgressBar'; // Adjust path if needed

export const HomePage: React.FC = () => {
  const [dashboardData, setDashboardData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchDashboardData = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await fetcher<DashboardData>('/dashboard');
        setDashboardData(data);
      } catch (err: any) {
        setError(err.message || 'An unknown error occurred.');
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  if (loading) {
    return (
      <section id="dashboard-content" className="content-section flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-green-500"></div>
        <p className="ml-4 text-lg text-gray-600">Loading dashboard data...</p>
      </section>
    );
  }

  if (error) {
    return (
      <section id="dashboard-content" className="content-section">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-xl relative mb-8 shadow-md" role="alert">
          <strong className="font-bold">Error:</strong>
          <span className="block sm:inline">{error}</span>
        </div>
      </section>
    );
  }

  return (
    <section id="dashboard-content" className="content-section">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <Card title="Total Orders" value={dashboardData?.totalOrders || 0} icon="📦" bgColor="bg-blue-600" />
        <Card title="Materials Ready" value={dashboardData?.materialsReadyKg || 0} unit="kg" icon="🏭" bgColor="bg-purple-600" />
        <Card title="Pending Orders" value={dashboardData?.pendingOrders || 0} icon="🚚" bgColor="bg-orange-600" />
      </div>

      {/* Material Inventory Section */}
      <div className="bg-white p-6 rounded-xl shadow-md">
        <h2 className="text-xl font-semibold text-gray-800 mb-4 flex items-center">
          <span className="text-green-700 mr-2">📊</span> Material Inventory
        </h2>
        <div className="space-y-4">
          {dashboardData?.materialInventory.map((item, index) => (
            <MaterialProgressBar
              key={index}
              material={item.material}
              currentKg={item.currentKg}
              totalKg={item.totalKg}
              barColor={item.barColor}
            />
          ))}
        </div>
      </div>
    </section>
  );
};
