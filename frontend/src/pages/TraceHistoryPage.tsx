import React, { useEffect, useState } from 'react';
import { fetcher } from '../utils/fetcher'; 

export const TraceHistoryPage: React.FC = () => {
  const [traceHistory, setTraceHistory] = useState<any[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTraceHistory = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await fetcher<any[]>('/log');
        setTraceHistory(data);
      } catch (err: any) {
        setError(err.message || 'Failed to fetch trace history.');
      } finally {
        setLoading(false);
      }
    };

    fetchTraceHistory();
  }, []);

  const getServiceColor = (requestBody: string) => {
    if (requestBody.includes('ThohBackgroundService')) {
      return 'bg-purple-50 border-purple-200';
    } else if (requestBody.includes('RecyclingBackgroundService')) {
      return 'bg-green-50 border-green-200';
    }
    return 'bg-gray-50 border-gray-200';
  };

  const getServiceBadgeColor = (requestBody: string) => {
    if (requestBody.includes('ThohBackgroundService')) {
      return 'bg-purple-100 text-purple-800 border-purple-300';
    } else if (requestBody.includes('RecyclingBackgroundService')) {
      return 'bg-green-100 text-green-800 border-green-300';
    }
    return 'bg-gray-100 text-gray-800 border-gray-300';
  };

  const getServiceName = (requestBody: string) => {
    if (requestBody.includes('ThohBackgroundService')) {
      return 'THOH Service';
    } else if (requestBody.includes('RecyclingBackgroundService')) {
      return 'Recycling Service';
    }
    return 'Unknown Service';
  };

  const getStatus = (response: string) => {
    if (response.includes('failed') || response.includes('No recycling machines')) {
      return { text: 'Failed', color: 'bg-red-100 text-red-800' };
    } else if (response.includes('starting') || response.includes('Running')) {
      return { text: 'Running', color: 'bg-blue-100 text-blue-800' };
    } else if (response.includes('updating') || response.includes('Retrieving')) {
      return { text: 'Processing', color: 'bg-yellow-100 text-yellow-800' };
    }
    return { text: 'Completed', color: 'bg-green-100 text-green-800' };
  };

  if (loading) {
    return (
      <section id="trace-history-content" className="content-section flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-green-500"></div>
        <p className="ml-4 text-lg text-gray-600">Loading trace history...</p>
      </section>
    );
  }

  if (error) {
    return (
      <section id="trace-history-content" className="content-section">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-xl relative mb-8 shadow-md" role="alert">
          <strong className="font-bold">Error:</strong>
          <span className="block sm:inline">{error}</span>
        </div>
      </section>
    );
  }

  if (traceHistory.length === 0) {
    return (
      <section id="trace-history-content" className="content-section bg-white p-6 rounded-xl shadow-md text-center">
        <p className="text-gray-600">No trace history available.</p>
      </section>
    );
  }

  return (
    <section id="trace-history-content" className="content-section bg-white p-6 rounded-xl shadow-md">
      <div className="mb-8">
        <h2 className="text-3xl font-bold text-gray-800 mb-2">System Trace History</h2>
        <p className="text-gray-600">Real-time activity log of all system operations and background services</p>
        <div className="mt-4 flex flex-wrap gap-2">
          <span className="px-3 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full">
            Recycling Service
          </span>
          <span className="px-3 py-1 text-xs font-medium bg-purple-100 text-purple-800 rounded-full">
            THOH Service
          </span>
          <span className="px-3 py-1 text-xs font-medium bg-gray-100 text-gray-800 rounded-full">
            Total Events: {traceHistory.length}
          </span>
        </div>
      </div>

      {/* Scrollable log container */}
      <div className="space-y-4 max-h-[600px] overflow-y-auto pr-2">
        {traceHistory.map((log, index) => {
          const status = getStatus(log.response);
          return (
            <div key={log.id} className={`border rounded-xl shadow-sm hover:shadow-md transition-shadow ${getServiceColor(log.requestBody)}`}>
              {/* Header */}
              <div className="p-4 border-b border-opacity-30">
                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
                  <div className="flex items-center gap-3">
                    <span className="text-lg font-bold text-gray-700">#{log.id}</span>
                    <span className={`px-3 py-1 text-xs font-medium rounded-full border ${getServiceBadgeColor(log.requestBody)}`}>
                      {getServiceName(log.requestBody)}
                    </span>
                    <span className={`px-2 py-1 text-xs font-medium rounded ${status.color}`}>
                      {status.text}
                    </span>
                  </div>
                  <span className="text-sm text-gray-500 font-mono">
                    {new Date(log.timestamp).toLocaleString()}
                  </span>
                </div>
              </div>

              {/* Content */}
              <div className="p-4">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  {/* Left Column */}
                  <div className="space-y-3">
                    <div>
                      <label className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Request Source</label>
                      <p className="text-sm text-gray-800 font-mono bg-white px-2 py-1 rounded border">
                        {log.requestSource || '<empty>'}
                      </p>
                    </div>
                    <div>
                      <label className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Request Endpoint</label>
                      <p className="text-sm text-gray-800 font-mono bg-white px-2 py-1 rounded border break-all">
                        {log.requestEndpoint || '<empty>'}
                      </p>
                    </div>
                    <div>
                      <label className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Timestamp</label>
                      <p className="text-sm text-gray-800 font-mono bg-white px-2 py-1 rounded border">
                        {log.timestamp}
                      </p>
                    </div>
                  </div>

                  {/* Right Column */}
                  <div className="space-y-3">
                    <div>
                      <label className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Request Body</label>
                      <div className="bg-white border rounded p-3 text-sm font-mono text-gray-800 max-h-20 overflow-y-auto">
                        {log.requestBody}
                      </div>
                    </div>
                    <div>
                      <label className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Response</label>
                      <div className="bg-white border rounded p-3 text-sm font-mono text-gray-800 max-h-24 overflow-y-auto">
                        {log.response}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Summary Footer */}
      <div className="mt-8 p-4 bg-gray-50 rounded-xl border">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-center">
          <div>
            <p className="text-2xl font-bold text-green-600">
              {traceHistory.filter(log => log.requestBody.includes('RecyclingBackgroundService')).length}
            </p>
            <p className="text-sm text-gray-600">Recycling Events</p>
          </div>
          <div>
            <p className="text-2xl font-bold text-purple-600">
              {traceHistory.filter(log => log.requestBody.includes('ThohBackgroundService')).length}
            </p>
            <p className="text-sm text-gray-600">THOH Events</p>
          </div>
          <div>
            <p className="text-2xl font-bold text-red-600">
              {traceHistory.filter(log => log.response.includes('failed') || log.response.includes('No recycling machines')).length}
            </p>
            <p className="text-sm text-gray-600">Failed Operations</p>
          </div>
        </div>
      </div>
    </section>
  );
};
