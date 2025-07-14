import React, { useEffect, useState } from 'react';
import { fetcher } from '../utils/fetcher';
import { MaterialOrderItem } from '../types'; 

export const MaterialOrdersPage: React.FC = () => {
  const [materialOrders, setMaterialOrders] = useState<MaterialOrderItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchMaterialOrders = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await fetcher<MaterialOrderItem[]>('/material-orders');
        setMaterialOrders(data);
      } catch (err: any) {
        setError(err.message || 'Failed to fetch material orders.');
      } finally {
        setLoading(false);
      }
    };

    fetchMaterialOrders();
  }, []);

  if (loading) {
    return (
      <section id="material-orders-content" className="content-section flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-green-500"></div>
        <p className="ml-4 text-lg text-gray-600">Loading material orders...</p>
      </section>
    );
  }

  if (error) {
    return (
      <section id="material-orders-content" className="content-section">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-xl relative mb-8 shadow-md" role="alert">
          <strong className="font-bold">Error:</strong>
          <span className="block sm:inline">{error}</span>
        </div>
      </section>
    );
  }

  if (materialOrders.length === 0) {
    return (
      <section id="material-orders-content" className="content-section bg-white p-6 rounded-xl shadow-md text-center">
        <p className="text-gray-600">No material orders available.</p>
      </section>
    );
  }

  return (
    <section id="material-orders-content" className="content-section bg-white p-6 rounded-xl shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-3xl font-bold text-gray-800">Material Orders</h2>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {materialOrders.map((material, index) => (
          <div key={index} className="bg-white p-6 rounded-xl shadow-md flex items-center justify-between">
            <div>
              <p className="text-xl font-semibold text-gray-800">{material.name}</p>
              <p className="text-3xl font-bold text-gray-900">{material.quantity}<span className="text-xl ml-1">kg</span></p>
              <p className="text-sm text-gray-500">{material.status}</p>
            </div>
            <div className={`w-3 h-3 rounded-full ${material.statusColor}`}></div>
          </div>
        ))}
      </div>
    </section>
  );
};