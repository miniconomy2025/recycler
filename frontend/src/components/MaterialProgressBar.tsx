interface MaterialProgressBarProps {
  material: string;
  currentKg: number;
  totalKg: number;
  barColor: string;
}

export const MaterialProgressBar: React.FC<MaterialProgressBarProps> = ({ material, currentKg, totalKg, barColor }) => {
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