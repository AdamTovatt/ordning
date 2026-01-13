import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { login, saveToken, getToken, getExpiresAt, clearToken, isTokenExpired } from '../services/authService';

interface AuthContextType {
  isAuthenticated: boolean;
  token: string | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    // Check for existing token on mount
    const storedToken = getToken();
    const storedExpiresAt = getExpiresAt();

    if (storedToken && storedExpiresAt) {
      if (!isTokenExpired()) {
        setToken(storedToken);
        setIsAuthenticated(true);
      } else {
        // Token expired, clear it
        clearToken();
      }
    }
    setIsLoading(false);
  }, []);

  const handleLogin = async (username: string, password: string): Promise<void> => {
    try {
      const response = await login(username, password);
      saveToken(response.token, response.expiresAt);
      setToken(response.token);
      setIsAuthenticated(true);
    } catch (error) {
      throw error;
    }
  };

  const handleLogout = (): void => {
    clearToken();
    setToken(null);
    setIsAuthenticated(false);
  };

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated,
        token,
        login: handleLogin,
        logout: handleLogout,
        isLoading,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
