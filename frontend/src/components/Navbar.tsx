interface NavbarProps {
  activeTab: string;
  onTabChange: (tab: string) => void;
}

export const Navbar: React.FC<NavbarProps> = ({ activeTab, onTabChange }) => {
  const navLinks = [
    { name: 'Dashboard', target: 'dashboard' },
    { name: 'Revenue Page', target: 'revenue' },
    { name: 'Stock', target: 'stock' },
    { name: 'Phones', target: 'phones' },
    { name: 'Trace History', target: 'trace-history' },
  ];

  return (
    <nav className="bg-white p-4 shadow-md rounded-b-xl">
      <div className="container mx-auto flex justify-between items-center flex-wrap">
        <div className="flex items-center mb-2 md:mb-0">
          {/* Recycling Sign Logo */}
          <div className="text-3xl text-green-700 mr-2">♻️</div>
          <a href="#" className="text-2xl font-bold text-green-700">Recycler System</a>
          <span className="ml-4 text-sm text-gray-500 hidden md:block">Recycling Management Platform</span>
        </div>
      </div>
      {/* Secondary Navigation for Tabs */}
      <div className="container mx-auto mt-4 border-t border-gray-200 pt-4">
        <div className="flex space-x-8 text-lg font-medium overflow-x-auto pb-2">
          {navLinks.map((link) => (
            <a
              key={link.target}
              href="#"
              className={`nav-link text-gray-700 hover:text-green-700 transition duration-200 ${
                activeTab === link.target ? 'active' : ''
              }`}
              onClick={(e) => {
                e.preventDefault();
                onTabChange(link.target);
              }}
            >
              {link.name}
            </a>
          ))}
        </div>
      </div>
    </nav>
  );
};
