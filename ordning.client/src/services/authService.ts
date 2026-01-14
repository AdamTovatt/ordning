import { apiClient, ApiError } from './apiClient';
import type { components } from '../types/api';

type LoginAuthRequest = components['schemas']['LoginAuthRequest'];
type AuthResponse = {
  token: string;
  expiresAt: string;
};

const TOKEN_STORAGE_KEY = 'auth_token';
const EXPIRES_AT_STORAGE_KEY = 'auth_expires_at';

export async function login(username: string, password: string): Promise<AuthResponse> {
  try {
    const loginData: LoginAuthRequest = { username, password };
    const { data, error, response } = await apiClient.POST('/api/auth/login', {
      body: loginData,
    });

    if (error || !data) {
      if (response.status === 401) {
        throw new Error('Invalid username or password');
      }
      throw new Error('Login failed');
    }

    // The API returns token and expiresAt, but we need to parse it from the response
    // Since Swagger doesn't show the response body type, we'll cast it
    return data as unknown as AuthResponse;
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      throw new Error('Invalid username or password');
    }
    throw new Error(error instanceof Error ? error.message : 'Login failed');
  }
}

export function saveToken(token: string, expiresAt: string): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, token);
  localStorage.setItem(EXPIRES_AT_STORAGE_KEY, expiresAt);
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_STORAGE_KEY);
}

export function getExpiresAt(): string | null {
  return localStorage.getItem(EXPIRES_AT_STORAGE_KEY);
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
  localStorage.removeItem(EXPIRES_AT_STORAGE_KEY);
}

export function isTokenExpired(): boolean {
  const expiresAt = getExpiresAt();
  if (!expiresAt) {
    return true;
  }

  const expirationDate = new Date(expiresAt);
  const now = new Date();
  return now >= expirationDate;
}
