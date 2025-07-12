interface StockItemProps {
  displayName: string;
  quantity: number;
  unit: string;
  status?: string; // Optional status for visual cues
}

export const StockItem: React.FC<StockItemProps> = ({ displayName, quantity, unit, status }) => {
  const getStatusColor = (status?: string) => {
    switch (status) {
      case 'High': return 'bg-green-500';
      case 'Medium': return 'bg-yellow-500';
      case 'Low': return 'bg-red-500';
      case 'Awaiting processing': return 'bg-blue-500';
      case 'In transit': return 'bg-orange-500';
      case 'Processed - Materials ready': return 'bg-purple-500';
      default: return 'bg-gray-400';
    }
  };

  return (
    <div className="flex items-center justify-between p-3 border rounded-lg bg-gray-50 shadow-sm">
      <div className="flex-grow">
        <p className="font-semibold text-gray-800">{displayName}</p>
        <p className="text-sm text-gray-600">Quantity: {quantity} {unit}</p>
      </div>
      {status && (
        <div className={`w-3 h-3 rounded-full ${getStatusColor(status)} ml-4`} title={`Status: ${status}`}></div>
      )}
    </div>
  );
};