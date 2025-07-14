export type JsonBody = Record<string, unknown>; 

export type FetcherOptions = Omit<RequestInit, 'body'> & {
  body?: BodyInit | null;
};


export interface CompanyRevenue { 
  companyName: string;
  orderNumber: string;
  status: string;
  createdAt: string; 
  items: RevenueOrderItemDto[];
  companyTotalOrders: number;
}


export interface RevenueOrderItemDto {
  materialName: string;
  quantityKg: number;
  totalPrice: number | null;
}


export interface DashboardData { 
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


export interface StockItemData { 
  name: string;
  quantity: number;
  unit: string;
  status?: string; 
}


export interface StockData { 
  rawMaterials: StockItemData[];
  phones: StockItemData[];
}


export interface PhoneInventoryItem { 
  model: string;
  quantity: number;
  status: string;
}


export interface MaterialOrderItem { 
  name: string;
  quantity: number;
  status: string;
  statusColor: string;
}

export interface TraceHistoryEvent {
  id: string;
  phoneType: string;
  receivedDate: string;
  processedDate: string;
  materialsExtracted: string[];
  destination: string;
  rawData?: {
    id: string;
    requestSource: string;
    requestEndpoint: string;
    timestamp: string | number;
    requestBody: string;
    response: any;
  };
}