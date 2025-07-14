interface MaterialProgressBarProps {
  material: string;
  currentKg: number;
  totalKg: number;
  maxKg: number; 
}

export const MaterialProgressBar: React.FC<MaterialProgressBarProps> = ({
  material,
  currentKg,
  totalKg,
  maxKg,
}) => {
  const widthPercent = Math.min((currentKg / maxKg) * 100, 100); 

  return (
    <div className="space-y-1">
      <div className="flex justify-between items-center">
        <span className="text-sm font-semibold text-gray-800">{material}</span>
        <span className="text-sm text-gray-600">{currentKg}kg </span>
      </div>
      <div className="w-full bg-gray-200 rounded-full h-4 overflow-hidden">
        <div
          className="bg-green-500 h-full rounded-full transition-all duration-300"
          style={{ width: `${widthPercent}%` }}
        ></div>
      </div>
    </div>
  );
};
