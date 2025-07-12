import React, { useState } from 'react'; 

import { Navbar } from './components/Navbar';
import { HomePage } from './pages/HomePage';
import { RevenuePage } from './pages/RevenuePage';
import { StockPage } from './pages/StockPage';
import { PhonesPage } from './pages/PhonesPage';
import { MaterialOrdersPage } from './pages/MaterialOrdersPage';
import { TraceHistoryPage } from './pages/TraceHistoryPage';

export const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState<string>('dashboard');

  const renderContent = () => {
    switch (activeTab) {
      case 'dashboard':
        return <HomePage />;
      case 'revenue':
        return <RevenuePage />;
      case 'stock':
        return <StockPage />;
      case 'phones':
        return <PhonesPage />;
      case 'material-orders':
        return <MaterialOrdersPage />;
      case 'trace-history':
        return <TraceHistoryPage />;
      default:
        return <HomePage />;
    }
  };

  return (
    <div className="min-h-screen flex flex-col">
      <Navbar activeTab={activeTab} onTabChange={setActiveTab} />
      <main className="flex-grow container mx-auto p-6 lg:p-10">
        {renderContent()}
      </main>
    </div>
  );
};
