import { BarChart } from '@mui/x-charts';
import * as React from 'react';
import { useEffect, useState } from 'react';
import { fetcher } from '../utils/fetcher';

type CompanyRevenue = {
  companyName: string;
  companyTotalOrders: number;
};

export const Revenue = () => {
  const [data, setData] = useState<CompanyRevenue[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetcher('/revenue/company-orders')
      .then((res) => {
        const totals: Record<string, number> = {};

        res.forEach((order: any) => {
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
      .catch((err) => {
        console.error('Error fetching revenue data:', err);
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <section>Loading chart...</section>;
  if (data.length === 0) return <section>No revenue data available.</section>;

  return (
    <section style={{ padding: 24 }}>
      <h2 style={{ textAlign: 'center' }}>Total Revenue per Supplier</h2>
      <BarChart
        height={300}
        series={[
          {
            label: 'Total Revenue',
            data: data.map((d) => d.companyTotalOrders),
          },
        ]}
        xAxis={[
          {
            data: data.map((d) => d.companyName),
            scaleType: 'band',
          },
        ]}
      />
    </section>
  );
};
