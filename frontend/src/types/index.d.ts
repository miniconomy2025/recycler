
export type JsonBody = Record<string, unknown>; // Exported

export type FetcherOptions = Omit<RequestInit, 'body'> & { // Exported
  body?: BodyInit | null;
};

// Type for Company Revenue data
export interface CompanyRevenue { // Exported
  companyName: string;
  companyTotalOrders: number;
}

// Type for Dashboard Data
export interface DashboardData { // Exported
  totalOrders: number;
  completedOrders: number;
  materialsReadyKg: number;
  pendingOrders: number;
  materialInventory: {
    material: string;
    currentKg: number;
    totalKg: number;
    barColor: string;
  }[];
}

// Type for Stock Item (Raw Materials & Phones)
export interface StockItemData { // Exported
  name: string;
  quantity: number;
  unit: string;
  status?: string; // e.g., 'High', 'Medium', 'Low' for raw materials, 'Awaiting processing' for phones
}

// Type for overall Stock data
export interface StockData { // Exported
  rawMaterials: StockItemData[];
  phones: StockItemData[];
}

// Type for Phone Inventory
export interface PhoneInventoryItem { // Exported
  model: string;
  quantity: number;
  status: string;
}

// Type for Material Order
export interface MaterialOrderItem { // Exported
  name: string;
  quantity: number;
  status: string;
  statusColor: string;
}

// Type for Trace History
export interface TraceHistoryEvent { // Exported
  id: string;
  phoneType: string;
  receivedDate: string;
  processedDate: string;
  materialsExtracted: string[];
  destination: string;
}
