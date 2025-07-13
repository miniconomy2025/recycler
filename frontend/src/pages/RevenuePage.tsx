import React, { useState, useEffect } from 'react';
import { BarChart } from '@mui/x-charts'; // Ensure @mui/x-charts is installed
import { fetcher } from '../utils/fetcher'; // Adjust path if needed
import { CompanyRevenue } from '../types'; // Adjust path if needed

export const RevenuePage: React.FC = () => {
  const [data, setData] = useState<CompanyRevenue[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    setError(null);
    fetcher<CompanyRevenue[]>('/company-orders')
      .then((res) => {
        // Grouping logic as provided by the user
        const totals: Record<string, number> = {};
        res.forEach((order: CompanyRevenue) => { // Ensure order is typed as CompanyRevenue
          const { companyName, companyTotalOrders } = order;
          totals[companyName] = (totals[companyName] ?? 0) + companyTotalOrders;
        });

        const grouped: CompanyRevenue[] = Object.entries(totals).map(
          ([companyName, companyTotalOrders]) => ({
            companyName,
            companyTotalOrders,
          })
        );
        setData(grouped);
      })
      .catch((err: any) => {
        console.error('Error fetching revenue data:', err);
        setError(err.message || 'Failed to fetch revenue data.');
      })
      .finally(() => setLoading(false));
  }, []);

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
        <p className="text-lg font-semibold text-green-800">Total Revenue: $1,250,000</p>
        <p className="text-sm text-gray-600">Last 30 days: +15%</p>
      </div>
      <div className="w-full overflow-x-auto h-80 bg-gray-100 rounded-lg flex items-center justify-center text-gray-500 text-center p-4">
        {/* Placeholder for Bar Chart due to external dependency error */}
        <p>Bar Chart Placeholder</p>
        <p className="text-sm mt-2">Chart library not available in this environment.</p>
      </div>
    </section>
  );
};