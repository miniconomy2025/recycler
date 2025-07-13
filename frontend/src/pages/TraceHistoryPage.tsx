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
      
      <div className="space-y-6">
        {traceHistory.map((event, index) => (
          <div key={index} className="border border-blue-200 rounded-lg overflow-hidden">
            {/* Formatted trace history display */}
            <div className="p-4 bg-blue-50">
              <h3 className="font-semibold text-blue-800 text-lg mb-3">Batch ID: {event.id}</h3>
              <ul className="list-disc list-inside text-sm text-gray-700 space-y-1">
                <li><strong>Phone Type:</strong> {event.phoneType}</li>
                <li><strong>Received:</strong> {event.receivedDate}</li>
                <li><strong>Processed:</strong> {event.processedDate}</li>
                <li><strong>Materials Extracted:</strong> {event.materialsExtracted.join(', ')}</li>
                <li><strong>Destination:</strong> {event.destination}</li>
              </ul>
            </div>
            
            {/* Raw log data display */}
            {event.rawData && (
              <div className="p-4 bg-gray-50 border-t border-blue-200">
                <h4 className="font-semibold text-gray-800 mb-3">Raw Log Data:</h4>
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 text-sm">
                  <div>
                    <p><strong className="text-gray-700">ID:</strong> <span className="text-gray-600">{event.rawData.id}</span></p>
                    <p><strong className="text-gray-700">Request Source:</strong> <span className="text-gray-600 break-all">{event.rawData.requestSource}</span></p>
                    <p><strong className="text-gray-700">Request Endpoint:</strong> <span className="text-gray-600 break-all">{event.rawData.requestEndpoint}</span></p>
                    <p><strong className="text-gray-700">Timestamp:</strong> <span className="text-gray-600">{new Date(event.rawData.timestamp).toLocaleString()}</span></p>
                  </div>
                  <div>
                    <div className="mb-3">
                      <p className="font-semibold text-gray-700 mb-1">Request Body:</p>
                      <div className="bg-white p-2 rounded border text-xs font-mono max-h-20 overflow-y-auto">
                        {event.rawData.requestBody}
                      </div>
                    </div>
                    <div>
                      <p className="font-semibold text-gray-700 mb-1">Response:</p>
                      <div className="bg-white p-2 rounded border text-xs font-mono max-h-20 overflow-y-auto">
                        {typeof event.rawData.response === 'string' ? event.rawData.response : JSON.stringify(event.rawData.response, null, 2)}
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