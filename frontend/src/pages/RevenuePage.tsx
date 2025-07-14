import React, { useState, useEffect } from 'react';
import { BarChart } from '@mui/x-charts'; 
import { fetcher } from '../utils/fetcher'; 
import { CompanyRevenue } from '../types'; 

export const RevenuePage: React.FC = () => {
  const [data, setData] = useState<CompanyRevenue[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalRevenue, setTotalRevenue] = useState(0);
  const [lastTransactionDate, setLastTransactionDate] = useState<string>('');

  useEffect(() => {
    setLoading(true);
    setError(null);

    fetcher<CompanyRevenue[]>('/company-orders')
      .then((res) => {
        console.log('API Response:', res); // Debugging: check raw response

        let calculatedTotalRevenue = 0;
        let mostRecentDate = '';

        res.forEach((order: CompanyRevenue) => {
          // Safely parse revenue value as a number
          const revenue = Number(order.companyTotalOrders);
          console.log('Parsed Revenue:', revenue); // Debugging

          calculatedTotalRevenue += isNaN(revenue) ? 0 : revenue;

          if (order.createdAt) {
            const orderDate = new Date(order.createdAt);
            const recentDate = new Date(mostRecentDate);
            if (!mostRecentDate || orderDate > recentDate) {
              mostRecentDate = order.createdAt;
            }
          }
        });

        setData(res);
        setTotalRevenue(calculatedTotalRevenue);
        setLastTransactionDate(mostRecentDate);
      })
      .catch((err: any) => {
        console.error('Error fetching revenue data:', err);
        setError(err.message || 'Failed to fetch revenue data.');
      })
      .finally(() => setLoading(false));
  }, []);

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-BB', {
      style: 'currency',
      currency: 'BBD',
    }).format(amount);
  };

  const formatDate = (dateString: string): string => {
    if (!dateString) return 'No recent transactions';
    try {
      const date = new Date(dateString);
      return `Last transaction: ${date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })}`;
    } catch {
      return 'Invalid date format';
    }
  };

  if (loading) {
    return (
      <section id="revenue-page-content" className="content-section flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-green-500"></div>
        <p className="ml-4 text-lg text-gray-600">Loading revenue data...</p>
      </section>
    );
  }

  if (error) {
    return (
      <section id="revenue-page-content" className="content-section">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-xl relative mb-8 shadow-md" role="alert">
          <strong className="font-bold">Error:</strong>
          <span className="block sm:inline">{error}</span>
        </div>
      </section>
    );
  }

  if (data.length === 0) {
    return (
      <section id="revenue-page-content" className="content-section bg-white p-6 rounded-xl shadow-md text-center">
        <p className="text-gray-600">No revenue data available.</p>
      </section>
    );
  }

  return (
    <section id="revenue-page-content" className="content-section bg-white p-6 rounded-xl shadow-md">
      <h2 className="text-3xl font-bold text-gray-800 mb-6 text-center">Total Revenue per Supplier</h2>
      <div className="mt-4 p-4 bg-green-50 rounded-lg border border-green-200 mb-6">
        <p className="text-lg font-semibold text-green-800">
          Total Revenue: {formatCurrency(totalRevenue)}
        </p>
        <p className="text-sm text-gray-600">
          {formatDate(lastTransactionDate)}
        </p>
      </div>
      <div className="w-full h-72 bg-gray-100 rounded-lg p-4">
        <BarChart
          xAxis={[{ scaleType: 'band', data: data.map(d => d.companyName) }]}
          series={[
            {
              data: data.map(d => Number(d.companyTotalOrders) || 0),
              label: 'Revenue',
              color: '#4caf50',
            }
          ]}
          width={700}
          height={280}
        />
      </div>

    </section>
  );
};
