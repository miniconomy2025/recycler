interface CardProps {
  title: string;
  value: string | number; // Value can be string or number
  icon: string;
  bgColor: string;
  unit?: string;
  statusColor?: string; // For material order status dot
  description?: string; // For material order available text
}

export const Card: React.FC<CardProps> = ({ title, value, icon, bgColor, unit, statusColor, description }) => {
  return (
    <div className={`${bgColor} text-white p-6 rounded-xl shadow-md flex items-center justify-between`}>
      <div>
        <p className="text-sm opacity-80">{title}</p>
        <p className="text-4xl font-bold">
          {value}
          {unit && <span className="text-2xl ml-1">{unit}</span>}
        </p>
        {description && <p className="text-sm opacity-80">{description}</p>}
      </div>
      <div className="text-5xl opacity-70">{icon}</div>
      {statusColor && <div className={`w-3 h-3 rounded-full ${statusColor}`}></div>}
    </div>
  );
};

// --- src/components/MaterialProgressBar.tsx ---
interface MaterialProgressBarProps {
  material: string;
  currentKg: number;
  totalKg: number;
  barColor: string;
}

const MaterialProgressBar: React.FC<MaterialProgressBarProps> = ({ material, currentKg, totalKg, barColor }) => {
  const percentage = (currentKg / totalKg) * 100;
  return (
    <div className="flex items-center justify-between">
      <span className="text-gray-700 w-1/4">{material}</span>
      <div className="w-3/4 bg-gray-200 rounded-full h-2">
        <div className={`${barColor} rounded-full h-full`} style={{ width: `${percentage}%` }}></div>
      </div>
      <span className="ml-4 text-gray-700 font-medium">{currentKg}kg</span>
    </div>
  );
};