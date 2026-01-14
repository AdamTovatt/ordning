import createClient from 'openapi-fetch';
import type { paths } from '../types/api';
import { getToken, clearToken } from './authService';

export class ApiError extends Error {
  public status: number;
  public statusText: string;

  constructor(
    message: string,
    status: number,
    statusText: string,
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.statusText = statusText;
  }
}

// Create the typed API client
export const apiClient = createClient<paths>({ baseUrl: '' });

// Add auth middleware to inject token and handle 401 errors
apiClient.use({
  async onRequest({ request }) {
    const token = getToken();
    if (token) {
      request.headers.set('Authorization', `Bearer ${token}`);
    }
    return request;
  },
  async onResponse({ response }) {
    if (response.status === 401) {
      clearToken();
      // Only redirect if we're not already on the login page
      // This prevents page refresh when login fails on the login page
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return response;
  },
});

// Helper function to unwrap openapi-fetch responses and throw errors
export async function unwrapResponse<T>(
  promise: Promise<{ data?: T; error?: any; response: Response }>,
): Promise<T> {
  const { data, error, response } = await promise;

  if (error) {
    let errorMessage = `Request failed: ${response.statusText}`;
    
    if (typeof error === 'object' && error !== null) {
      const errorObj = error as { message?: string; error?: string; detail?: string; title?: string };
      errorMessage = errorObj.message || errorObj.error || errorObj.detail || errorObj.title || errorMessage;
    } else if (typeof error === 'string') {
      errorMessage = error;
    }

    throw new ApiError(errorMessage, response.status, response.statusText);
  }

  if (!data) {
    throw new ApiError('No data returned', response.status, response.statusText);
  }

  return data;
}
