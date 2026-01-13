import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { IconLogin, IconEye, IconEyeOff } from '@tabler/icons-react';
import { useAuth } from '../contexts/AuthContext';
import { Button, IconButton } from '../components/ui';

export function LoginPage() {
  const [username, setUsername] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [showPassword, setShowPassword] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/dashboard', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  if (isAuthenticated) {
    return null;
  }

  const handleSubmit = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();
    e.stopPropagation();
    if (!username.trim() || !password.trim()) {
      setError('Please enter both username and password');
      return;
    }

    setError('');
    setIsLoading(true);

    try {
      await login(username, password);
      navigate('/dashboard');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed. Please try again.');
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen h-screen flex flex-col items-center justify-center bg-[var(--color-bg)] text-[var(--color-fg)] p-0 gap-8 overflow-hidden">
      <div className="w-full max-w-[400px] px-4">
        <div className="login-form-wrapper">
          <div className="login-form-animated-border"></div>
          <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-2xl p-10 shadow-[0_8px_32px_rgba(0,0,0,0.3)] relative z-[1]">
            <form onSubmit={handleSubmit} action="#" className="flex flex-col gap-5">
            {error && (
              <div className="flex items-center gap-2 p-3 rounded-md bg-[rgba(182,35,36,0.1)] border border-[rgba(182,35,36,0.3)] text-[var(--dark-danger-color)] text-sm">
                <svg className="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
                <span>{error}</span>
              </div>
            )}

            <div className="flex flex-col gap-2">
              <label htmlFor="username" className="text-sm font-medium text-[var(--color-fg)] select-none">
                Username
              </label>
              <input
                type="text"
                id="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="Enter your username"
                disabled={isLoading}
                required
                autoComplete="off"
                className="w-full px-4 py-3 rounded-md bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)] border border-[var(--color-border)] text-base transition-[border-color,box-shadow] focus:outline-none focus:border-[var(--elevation-level-4-dark)] focus:shadow-[0_0_8px_2px_rgba(121,94,169,0.15)] placeholder:text-[var(--color-fg)] placeholder:opacity-50 disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>

            <div className="flex flex-col gap-2">
              <label htmlFor="password" className="text-sm font-medium text-[var(--color-fg)] select-none">
                Password
              </label>
              <div className="relative w-full">
                <input
                  type={showPassword ? 'text' : 'password'}
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter your password"
                  disabled={isLoading}
                  required
                  autoComplete="off"
                  className="w-full px-4 py-3 pr-10 rounded-md bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)] border border-[var(--color-border)] text-base transition-all focus:outline-none focus:border-[var(--elevation-level-4-dark)] focus:shadow-[0_0_8px_2px_rgba(121,94,169,0.15)] placeholder:text-[var(--color-fg)] placeholder:opacity-50 disabled:opacity-60 disabled:cursor-not-allowed"
                />
                <IconButton
                  type="button"
                  className="absolute right-3 top-1/2 -translate-y-1/2 z-10"
                  onClick={() => setShowPassword(!showPassword)}
                  disabled={isLoading}
                  size="sm"
                >
                  {showPassword ? <IconEyeOff size={16} /> : <IconEye size={16} />}
                </IconButton>
              </div>
            </div>

            <Button
              type="submit"
              variant="outlinePrimary"
              size="md"
              className="w-full mt-2"
              disabled={isLoading || !username.trim() || !password.trim()}
              loading={isLoading}
              icon={!isLoading ? <IconLogin size={16} /> : undefined}
            >
              {!isLoading && 'Sign In'}
            </Button>
          </form>
          </div>
        </div>
      </div>
    </div>
  );
}
