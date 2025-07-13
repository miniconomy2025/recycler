import React, { useEffect, useMemo, useState } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  getPaginationRowModel,
  flexRender,
} from '@tanstack/react-table';
import {fetcher} from "../utils/fetcher";

type LogEntry = {
  id: number;
  requestSource: string;
  requestEndpoint: string;
  requestBody: string;
  response: string;
  timestamp: string;
};

const LogTable = () => {
  const [data, setData] = useState<LogEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const BASE_URL = 'https://recycler.projects.bbdgrad.com/internal';

  // Fetch data from your API
  useEffect(() => {
    fetcher<LogEntry[]>('/log')
      .then(response => {
        console.log('response', response);
        setData(response);
        setLoading(false);
      })
      .catch(error => {
        console.error('Error fetching logs:', error);
        setLoading(false);
      });
  }, []);

  const columns = useMemo(() => [
    { accessorKey: 'id', header: 'ID' },
    { accessorKey: 'requestSource', header: 'Source' },
    { accessorKey: 'requestEndpoint', header: 'Endpoint' },
    { accessorKey: 'requestBody', header: 'Request Body' },
    { accessorKey: 'response', header: 'Response' },
    { accessorKey: 'timestamp', header: 'Timestamp' },
  ], []);

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  console.log('data', data);
  console.log('table', table);
  console.log('table.getRowModel()', table.getRowModel());
  console.log('table.getRowModel().rows', table.getRowModel().rows);


  if (loading) return <p>Loading logs...</p>;

  return (
    <div>
      <table cellPadding="5">
        <thead>
        {table.getHeaderGroups().map(headerGroup => (
          <tr key={headerGroup.id}>
            {headerGroup.headers.map(header => (
              <th key={header.id}>
                {flexRender(header.column.columnDef.header, header.getContext())}
              </th>
            ))}
          </tr>
        ))}
        </thead>
        <tbody>
        {table.getRowModel().rows.map(row => (
          <tr key={row.id}>
            {row.getVisibleCells().map(cell => (
              <td key={cell.id}>
                {flexRender(cell.column.columnDef.cell, cell.getContext())}
              </td>
            ))}
          </tr>
        ))}
        </tbody>
      </table>

      <div style={{ marginTop: '10px' }}>
        <button onClick={() => table.previousPage()} disabled={!table.getCanPreviousPage()}>
          Previous
        </button>
        <span style={{ margin: '0 10px' }}>
          Page {table.getState().pagination.pageIndex + 1} of {table.getPageCount()}
        </span>
        <button onClick={() => table.nextPage()} disabled={!table.getCanNextPage()}>
          Next
        </button>
      </div>
    </div>
  );
};

export default LogTable;
