import React, { useEffect, useState } from 'react';
import { fetcher } from '../utils/fetcher'; // Adjust path if needed
import { TraceHistoryEvent } from '../types'; // Adjust path if needed

export const TraceHistoryPage: React.FC = () => {
  const [traceHistory, setTraceHistory] = useState<TraceHistoryEvent[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

   useEffect(() => {
    const fetchTraceHistory = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await fetcher<TraceHistoryEvent[]>('/log');
        setTraceHistory(data);
      } catch (err: any) {
        setError(err.message || 'Failed to fetch trace history.');
      } finally {
        setLoading(false);
      }
    };

    fetchTraceHistory();
  }, []);

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
      <h2 className="text-3xl font-bold text-gray-800 mb-6">Trace History</h2>
      <p className="text-gray-600 mb-4">View the complete trace history of recycled materials.</p>
      
      <div className="space-y-4">
        {traceHistory.map((event, index) => (
          <div key={index} className="p-4 bg-blue-50 rounded-lg border border-blue-200 shadow-sm">
            <p className="font-semibold text-blue-800 mb-3">Batch ID: {event.id}</p>
            
            {event.rawData && (
              <div className="space-y-2 text-sm text-gray-700">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <p><strong>ID:</strong> {event.rawData.id}</p>
                    <p><strong>Request Source:</strong> "{event.rawData.requestSource}"</p>
                    <p><strong>Request Endpoint:</strong> "{event.rawData.requestEndpoint}"</p>
                    <p><strong>Timestamp:</strong> "{event.rawData.timestamp}"</p>
                  </div>
                  <div className="space-y-2">
                    <div>
                      <p><strong>Request Body:</strong></p>
                      <div className="bg-white p-2 rounded border text-xs font-mono max-h-20 overflow-y-auto break-all">
                        {event.rawData.requestBody}
                      </div>
                    </div>
                    <div>
                      <p><strong>Response:</strong></p>
                      <div className="bg-white p-2 rounded border text-xs font-mono max-h-20 overflow-y-auto break-all">
                        {event.rawData.response}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        ))}
      </div>
    </section>
  );
};