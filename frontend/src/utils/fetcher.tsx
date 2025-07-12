// --- src/utils/fetcher.ts ---

import { FetcherOptions } from "../types";

/**
 * A generic fetcher utility for making API requests to your existing C# backend.
 * @param url The endpoint to fetch from.
 * @param options Optional FetcherOptions for the request.
 * @returns A Promise that resolves with the parsed JSON data.
 * @throws An error if the network request fails or the response is not OK.
 */
export async function fetcher<T>(url: string, options?: FetcherOptions): Promise<T> {
  try {
    // Get base URL from environment variable or default to local C# API
    const baseURL = process.env.REACT_APP_API_URL || 'https://localhost:443'; // Changed to HTTP
    
    // Map your frontend endpoints to your existing C# controller endpoints
    const endpointMapping: Record<string, string> = {
      '/dashboard': '/internal/stock', // Use development endpoint
      '/stock': '/internal/stock',
      '/phones': '/internal/stock',
      '/phone-inventory': '/internal/stock',
      '/material-orders': '/api/materials',
      '/revenue/company-orders': '/api/revenue',
      '/company-revenue': '/api/revenue',
      '/machines': '/api/machines',
    };

    const mappedEndpoint = endpointMapping[url] || url;
    const fullUrl = `${baseURL}${mappedEndpoint}`;

    console.log(`Fetching from: ${fullUrl}`); // Debug log
    console.log(`Environment REACT_APP_API_URL: ${process.env.REACT_APP_API_URL}`); // Debug log
    console.log(`Original endpoint: ${url}, Mapped to: ${mappedEndpoint}`); // Debug log

    // Prepare the request body
    let requestBody: BodyInit | null = null;
    if (options?.body) {
      if (typeof options.body === 'string' || options.body instanceof FormData || options.body instanceof ArrayBuffer) {
        requestBody = options.body;
      } else {
        // Convert JsonBody to string
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
    
    // Transform data based on endpoint to match your frontend expectations
    return transformData(url, data) as T;

  } catch (error) {
    console.error('Fetcher error:', error);
    
    // If network error (API not available), provide helpful error message
    if (error instanceof TypeError && error.message.includes('fetch')) {
      throw new Error('Unable to connect to the API. Please ensure the backend server is running.');
    }
    
    throw error; // Re-throw to be caught by the component
  }
}

/**
 * Transform data from your C# controllers to match frontend expectations
 */
function transformData(endpoint: string, data: any): any {
  switch (endpoint) {
    case '/dashboard':
      // Transform stock data to dashboard format
      return transformStockToDashboard(data);
    
    case '/stock':
      // Your stock data is already in the right format!
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
      // Extract only phones from stock data
      return data.phones?.map((item: any) => ({
        model: item.name,
        quantity: item.quantity,
        status: item.quantity > 50 ? 'In Stock' : item.quantity > 10 ? 'Low Stock' : 'Out of Stock'
      })) || [];
    
    case '/material-orders':
      // Transform materials data to material orders format
      return transformToMaterialOrders(data);
    
    case '/revenue/company-orders':
    case '/company-revenue':
      // Transform revenue data to expected format
      return transformToRevenue(data);
    
    case '/machines':
      // Return machines data as-is or transform if needed
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
  
  // Calculate dashboard metrics from your stock data
  const totalMaterialsKg = rawMaterials.reduce((sum: number, item: any) => sum + (item.quantity || 0), 0);
  const totalPhones = phones.reduce((sum: number, item: any) => sum + (item.quantity || 0), 0);
  
  return {
    totalOrders: totalPhones, // Using phone count as total orders for now
    completedOrders: Math.floor(totalPhones * 0.8), // 80% completed
    materialsReadyKg: totalMaterialsKg,
    pendingOrders: Math.floor(totalPhones * 0.2), // 20% pending
    materialInventory: rawMaterials.map((item: any) => ({
      material: item.name,
      currentKg: item.quantity,
      totalKg: item.quantity + Math.floor(item.quantity * 0.3), // Add 30% for target
      barColor: getBarColor(item.quantity)
    }))
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