interface CardProps {
  title: string;
  value: string | number; 
  icon: string;
  bgColor: string;
  unit?: string;
  statusColor?: string; 
  description?: string; 
}

export const Card: React.FC<CardProps> = ({
  title,
  value,
  icon,
  bgColor,
  unit,
  statusColor,
  description
}) => {
  return (
    <div className={`${bgColor} text-white p-4 rounded-xl shadow-md flex items-center justify-between`}>
      <div>
        <p className="text-sm font-medium text-white">{title}</p>
        <p className="text-3xl font-extrabold text-white">
          {value}
          {unit && <span className="text-lg ml-1">{unit}</span>}
        </p>
        {description && <p className="text-xs mt-1 text-white">{description}</p>}
      </div>

      <div className="flex-shrink-0 w-12 h-12 flex items-center justify-center text-4xl">
        <span role="img" aria-label={title}>
          {icon}
        </span>
      </div>

      {statusColor && <div className={`w-3 h-3 rounded-full ${statusColor}`}></div>}
    </div>
  );
};
