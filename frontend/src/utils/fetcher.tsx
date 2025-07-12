import { FetcherOptions } from '../types'; 

const BASE_URL = 'https://recycler.projects.bbdgrad.com/internal'; 

export async function fetcher<T>(url: string, options?: FetcherOptions): Promise<T> {
  try {
    const { body, headers = {}, ...rest } = options || {};

    const finalBody =
      body === null || body === undefined
        ? body
        : typeof body === 'object' &&
          !(body instanceof FormData) &&
          !(body instanceof Blob) &&
          !(body instanceof URLSearchParams) &&
          !(body instanceof ReadableStream) &&
          !(body instanceof ArrayBuffer) &&
          !ArrayBuffer.isView(body)
        ? JSON.stringify(body)
        : body;

    const response = await fetch(`${BASE_URL}${url}`, {
      method: options?.method || 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...headers,
      },
      body: finalBody,
      ...rest,
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(errorData.message || `HTTP error! Status: ${response.status}`);
    }

    return await response.json() as T;
  } catch (error) {
    console.error('Fetcher error:', error);
    throw error;
  }
}
