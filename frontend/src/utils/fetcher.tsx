
import { FetcherOptions } from "../types";

export async function fetcher<T>(url: string, options?: FetcherOptions): Promise<T> {
  try {

    const baseURL = process.env.REACT_APP_API_URL || 'https://recycler-api.projects.bbdgrad.com'; 

    const endpointMapping: Record<string, string> = {
      '/dashboard': '/internal/stock', 
      '/stock': '/internal/stock',
      '/phones': '/internal/stock',
      '/phone-inventory': '/internal/stock',
      '/company-orders': '/internal/revenue/company-orders',
      '/company-revenue': '/api/revenue',
      '/log': '/internal/log',
    };

    const mappedEndpoint = endpointMapping[url] || url;
    const fullUrl = `${baseURL}${mappedEndpoint}`;

    console.log(`Fetching from: ${fullUrl}`); 
    console.log(`Environment REACT_APP_API_URL: ${process.env.REACT_APP_API_URL}`); 
    console.log(`Original endpoint: ${url}, Mapped to: ${mappedEndpoint}`); 

    let requestBody: BodyInit | null = null;
    if (options?.body) {
      if (typeof options.body === 'string' || options.body instanceof FormData || options.body instanceof ArrayBuffer) {
        requestBody = options.body;
      } else {
        requestBody = JSON.stringify(options.body);
      }
    }

    const response = await fetch(fullUrl, {
      method: options?.method || 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
      body: requestBody,
      mode: options?.mode || 'cors',
      credentials: options?.credentials || 'omit',
      cache: options?.cache,
      redirect: options?.redirect,
      referrerPolicy: options?.referrerPolicy,
      signal: options?.signal,
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || response.statusText}`);
    }

    const data = await response.json();
    

    return transformData(url, data) as T;

  } catch (error) {
    console.error('Fetcher error:', error);
    
   
    if (error instanceof TypeError && error.message.includes('fetch')) {
      throw new Error('Unable to connect to the API. Please ensure the backend server is running.');
    }
    
    throw error; 
  }
}

/**
 * Transform data from your C# controllers to match frontend expectations
 */
function transformData(endpoint: string, data: any): any {
  switch (endpoint) {
    case '/dashboard':
 
      return transformStockToDashboard(data);
    
    case '/stock':

      return {
        rawMaterials: data.rawMaterials?.map((item: any) => ({
          name: item.name,
          quantity: item.quantity,
          unit: item.unit,
          status: getStatus(item.quantity)
        })) || [],
        phones: data.phones?.map((item: any) => ({
          name: item.name,
          quantity: item.quantity,
          unit: item.unit,
          status: item.quantity > 0 ? 'In Stock' : 'Out of Stock'
        })) || []
      };
    
    case '/phones':
    case '/phone-inventory':

      return data.phones?.map((item: any) => ({
        model: item.name,
        quantity: item.quantity,
        status: item.quantity > 50 ? 'In Stock' : item.quantity > 10 ? 'Low Stock' : 'Out of Stock'
      })) || [];
    
    case '/revenue/company-orders':
    case '/company-revenue':

      return transformToRevenue(data);
    
    case '/log':

      return data;
    
    
    default:
      return data;
  }
}

/**
 * Transform stock data to dashboard format
 */
function transformStockToDashboard(data: any): any {
  const rawMaterials = data.rawMaterials || [];
  const phones = data.phones || [];

  return {
    totalOrders: data.totalOrders || 0,
    completedOrders: data.completedOrders || 0,
    pendingOrders: data.pendingOrders || 0,
    materialsReadyKg: rawMaterials.reduce((sum: number, item: any) => sum + (item.quantity || 0), 0),
    materialInventory: rawMaterials.map((item: any) => ({
      material: item.name,
      currentKg: item.quantity,
      totalKg: item.quantity + Math.floor(item.quantity * 0.3),
      barColor: getBarColor(item.quantity),
    })),
  };
}


/**
 * Transform materials data to material orders format
 */
function transformToMaterialOrders(data: any): any {
  if (Array.isArray(data)) {
    return data.map((item: any) => ({
      name: item.name || item.materialName || 'Unknown',
      quantity: item.quantity || item.availableQuantity || 0,
      status: item.status || 'Available',
      statusColor: getStatusColor(item.status || 'Available')
    }));
  }
  
  return data;
}

/**
 * Transform revenue data to expected format
 */
function transformToRevenue(data: any): any {
  if (Array.isArray(data)) {
    return data.map((item: any) => ({
      companyName: item.companyName || item.name || 'Unknown Company',
      companyTotalOrders: item.totalOrders || item.orderCount || item.companyTotalOrders || 0
    }));
  }
  
  return data;
}


/**
 * Transform logs to trace history format
 */
function transformToTraceHistory(data: any): any {
  if (Array.isArray(data)) {
    return data.map((log: any, index: number) => ({
      id: `#LOG-${log.id || index}`,
      phoneType: extractPhoneTypeFromLog(log),
      receivedDate: formatDate(log.timestamp),
      processedDate: formatDate(log.timestamp), // Using same timestamp, adjust as needed
      materialsExtracted: extractMaterialsFromLog(log),
      destination: extractDestinationFromLog(log)
    }));
  }
  
  return data;
}

/**
 * Helper function to extract phone type from log data
 */
function extractPhoneTypeFromLog(log: any): string {
  try {

    if (log.requestBody) {
      const requestData = JSON.parse(log.requestBody);
      if (requestData.phoneType || requestData.model) {
        return requestData.phoneType || requestData.model;
      }
    }
    
    if (log.requestEndpoint && log.requestEndpoint.includes('phone')) {
      return 'Phone Processing';
    }
    
    return 'Mixed Processing';
  } catch (error) {
    return 'Unknown Processing';
  }
}

/**
 * Helper function to extract materials from log data
 */
function extractMaterialsFromLog(log: any): string[] {
  try {
    if (log.response) {
      const responseData = JSON.parse(log.response);
      if (responseData.rawMaterials) {
        return responseData.rawMaterials.map((m: any) => m.name).slice(0, 3);
      }
      if (responseData.materials) {
        return responseData.materials.slice(0, 3);
      }
    }
    
    return ['Copper', 'Plastic', 'Aluminum']; 
  } catch (error) {
    return ['Processing'];
  }
}

/**
 * Helper function to extract destination from log data
 */
function extractDestinationFromLog(log: any): string {
  try {

    if (log.requestSource) {
      return `Processed by ${log.requestSource}`;
    }
    
    return 'Material Processing Facility';
  } catch (error) {
    return 'Unknown Destination';
  }
}

/**
 * Helper function to get bar color based on quantity
 */
function getBarColor(quantity: number): string {
  if (quantity >= 500) return 'bg-green-500';
  if (quantity >= 200) return 'bg-yellow-500';
  if (quantity >= 100) return 'bg-orange-500';
  return 'bg-red-500';
}

/**
 * Helper function to get status based on quantity
 */
function getStatus(quantity: number): string {
  if (quantity >= 500) return 'High';
  if (quantity >= 100) return 'Medium';
  return 'Low';
}

/**
 * Helper function to get status color
 */
function getStatusColor(status: string): string {
  switch (status.toLowerCase()) {
    case 'available':
    case 'completed':
    case 'high':
      return 'bg-green-500';
    case 'pending':
    case 'medium':
      return 'bg-yellow-500';
    case 'processing':
      return 'bg-blue-500';
    case 'low':
    case 'cancelled':
      return 'bg-red-500';
    default:
      return 'bg-gray-500';
  }
}

/**
 * Helper function to format date strings
 */
function formatDate(dateString: string | number | Date): string {
  if (!dateString) return '';
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return '';
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
}