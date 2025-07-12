import React, { useEffect, useState } from 'react';
import { fetcher } from '../utils/fetcher'; // Adjust path if needed
import { PhoneInventoryItem } from '../types'; // Adjust path if needed
import { StockItem } from '../components/StockItem'; // Reusing StockItem for consistency

export const PhonesPage: React.FC = () => {
  const [phoneInventory, setPhoneInventory] = useState<PhoneInventoryItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchPhoneData = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await fetcher<PhoneInventoryItem[]>('/phones');
        setPhoneInventory(data);
      } catch (err: any) {
        setError(err.message || 'Failed to fetch phone data.');
      } finally {
        setLoading(false);
      }
    };

    fetchPhoneData();
  }, []);

  if (loading) {
    return (
      <section id="phones-content" className="content-section flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-green-500"></div>
        <p className="ml-4 text-lg text-gray-600">Loading phone inventory...</p>
      </section>
    );
  }

  if (error) {
    return (
      <section id="phones-content" className="content-section">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-xl relative mb-8 shadow-md" role="alert">
          <strong className="font-bold">Error:</strong>
          <span className="block sm:inline">{error}</span>
        </div>
      </section>
    );
  }

  if (phoneInventory.length === 0) {
    return (
      <section id="phones-content" className="content-section bg-white p-6 rounded-xl shadow-md text-center">
        <p className="text-gray-600">No phone data available.</p>
      </section>
    );
  }

  return (
    <section id="phones-content" className="content-section bg-white p-6 rounded-xl shadow-md">
      <h2 className="text-3xl font-bold text-gray-800 mb-6">Available Phones</h2>
      <p className="text-gray-600 mb-4">List of phones currently available for recycling or processing.</p>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {phoneInventory.map((phone, index) => (
          <StockItem
            key={index}
            displayName={phone.model}
            quantity={phone.quantity}
            unit="units" // Assuming 'units' for phones
            status={phone.status}
          />
        ))}
      </div>
    </section>
  );
};