import React, { useEffect, useState } from 'react';
import { fetcher } from '../utils/fetcher'; 
import { StockData } from '../types/index';
import { StockItem } from '../components/StockItem'; 

export const StockPage: React.FC = () => {
  const [stock, setStock] = useState<StockData>({
    rawMaterials: [],
    phones: [],
  });
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStock = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await fetcher<StockData>('/stock');
        setStock(data);
      } catch (err: any) {
        setError(`Failed to fetch stock: ${err.message || 'Unknown error'}`);
      } finally {
        setLoading(false);
      }
    };

    fetchStock();
  }, []);

  if (loading) {
    return (
      <section id="stock-content" className="content-section flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-green-500"></div>
        <p className="ml-4 text-lg text-gray-600">Loading stock data...</p>
      </section>
    );
  }

  if (error) {
    return (
      <section id="stock-content" className="content-section">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-xl relative mb-8 shadow-md" role="alert">
          <strong className="font-bold">Error:</strong>
          <span className="block sm:inline">{error}</span>
        </div>
      </section>
    );
  }

  return (
    <section id="stock-content" className="content-section bg-white p-6 rounded-xl shadow-md">
      <h2 className="text-3xl font-bold text-gray-800 mb-6">Current Stock</h2>

      {stock.rawMaterials.length > 0 && (
        <div className="mb-8">
          <h3 className="text-2xl font-semibold text-gray-700 mb-4">Raw Materials</h3>
          <div className="space-y-4">
            {stock.rawMaterials.map((item, index) => (
              <StockItem
                key={`raw-${index}`}
                displayName={item.name}
                quantity={item.quantity}
                unit={item.unit}
                status={item.status}
              />
            ))}
          </div>
        </div>
      )}

      {stock.rawMaterials.length === 0 && (
        <p className="text-gray-600 text-center">No stock data available.</p>
      )}
    </section>
  );
};
