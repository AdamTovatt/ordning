import { useState, useEffect, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { IconEye, IconEyeOff, IconUserPlus, IconLogout } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Input, Button, IconButton } from '../components/ui';
import { Header } from '../components/Header';
import { IdTag } from '../components/IdTag';
import { useAuth } from '../contexts/AuthContext';
import toast from 'react-hot-toast';

type User = components['schemas']['User'];
type CreateUserRequest = components['schemas']['CreateUserRequest'];
type UpdatePasswordRequest = components['schemas']['UpdatePasswordRequest'];

export function AccountPage() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isAdmin, setIsAdmin] = useState<boolean>(false);
  const [userCount, setUserCount] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isLoadingUser, setIsLoadingUser] = useState<boolean>(false);
  const [isLoadingPassword, setIsLoadingPassword] = useState<boolean>(false);
  const [isLoadingCreateUser, setIsLoadingCreateUser] = useState<boolean>(false);
  
  const [passwordData, setPasswordData] = useState<{
    newPassword: string;
    confirmPassword: string;
  }>({
    newPassword: '',
    confirmPassword: '',
  });
  const [showNewPassword, setShowNewPassword] = useState<boolean>(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState<boolean>(false);
  const [passwordError, setPasswordError] = useState<string | undefined>(undefined);

  const [createUserData, setCreateUserData] = useState<CreateUserRequest>({
    username: '',
    email: '',
    password: '',
    roles: [],
  });
  const [showCreateUserPassword, setShowCreateUserPassword] = useState<boolean>(false);
  const [createUserErrors, setCreateUserErrors] = useState<{
    username?: string;
    email?: string;
    password?: string;
    roles?: string;
  }>({});
  const [showCreateUserForm, setShowCreateUserForm] = useState<boolean>(false);

  useEffect(() => {
    fetchUserData();
  }, []);

  const fetchUserData = async () => {
    setIsLoading(true);
    try {
      const [userResponse, adminResponse] = await Promise.all([
        apiClient.GET('/api/User/me'),
        apiClient.GET('/api/User/is-admin'),
      ]);

      const user = await unwrapResponse<User>(userResponse);
      const admin = await unwrapResponse<boolean>(adminResponse);

      setCurrentUser(user);
      setIsAdmin(admin);

      if (admin) {
        try {
          const countResponse = await apiClient.GET('/api/User/count');
          const count = await unwrapResponse<number>(countResponse);
          setUserCount(count);
        } catch (error) {
          console.error('Failed to fetch user count:', error);
        }
      }
    } catch (error) {
      console.error('Failed to fetch user data:', error);
      toast.error('Failed to load account information');
    } finally {
      setIsLoading(false);
    }
  };

  const handlePasswordChange = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();

    if (!passwordData.newPassword.trim()) {
      setPasswordError('New password is required');
      return;
    }

    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setPasswordError('Passwords do not match');
      return;
    }

    if (passwordData.newPassword.length < 3) {
      setPasswordError('Password must be at least 3 characters long');
      return;
    }

    if (!currentUser?.id) {
      toast.error('User ID not found');
      return;
    }

    setPasswordError(undefined);
    setIsLoadingPassword(true);

    try {
      const request: UpdatePasswordRequest = {
        newPassword: passwordData.newPassword,
      };

      const response = await apiClient.PUT('/api/User/{id}/password', {
        params: {
          path: {
            id: currentUser.id,
          },
        },
        body: request,
      });

      const responseData = response as { error?: unknown; response: Response };
      
      if (responseData.error) {
        const errorMessage = typeof responseData.error === 'string' 
          ? responseData.error 
          : (responseData.error as { message?: string; detail?: string; title?: string })?.message 
            || (responseData.error as { message?: string; detail?: string; title?: string })?.detail 
            || (responseData.error as { message?: string; detail?: string; title?: string })?.title 
            || 'Failed to update password';
        throw new Error(errorMessage);
      }

      if (responseData.response.status === 204 || responseData.response.ok) {
        toast.success('Password updated successfully');
        setPasswordData({ newPassword: '', confirmPassword: '' });
      } else {
        throw new Error('Failed to update password');
      }
    } catch (error) {
      console.error('Failed to update password:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to update password');
        setPasswordError(error.message || 'Failed to update password');
      } else {
        toast.error('Failed to update password');
        setPasswordError('Failed to update password');
      }
    } finally {
      setIsLoadingPassword(false);
    }
  };

  const validateCreateUserForm = (): boolean => {
    const newErrors: typeof createUserErrors = {};

    if (!createUserData.username?.trim()) {
      newErrors.username = 'Username is required';
    }

    if (!createUserData.email?.trim()) {
      newErrors.email = 'Email is required';
    }

    if (!createUserData.password?.trim()) {
      newErrors.password = 'Password is required';
    } else if (createUserData.password.length < 3) {
      newErrors.password = 'Password must be at least 3 characters long';
    }

    setCreateUserErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleCreateUser = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();

    if (!validateCreateUserForm()) {
      return;
    }

    setIsLoadingCreateUser(true);

    try {
      const request: CreateUserRequest = {
        username: createUserData.username?.trim() || null,
        email: createUserData.email?.trim() || null,
        password: createUserData.password?.trim() || null,
        roles: createUserData.roles || null,
      };

      const responsePromise = apiClient.POST('/api/User', {
        body: request,
      });

      await unwrapResponse<User>(responsePromise);
      toast.success('User created successfully');
      setCreateUserData({
        username: '',
        email: '',
        password: '',
        roles: [],
      });
      setShowCreateUserForm(false);
      setCreateUserErrors({});
      
      // Refresh user count
      if (isAdmin) {
        try {
          const countResponse = await apiClient.GET('/api/User/count');
          const count = await unwrapResponse<number>(countResponse);
          setUserCount(count);
        } catch (error) {
          console.error('Failed to refresh user count:', error);
        }
      }
    } catch (error) {
      console.error('Failed to create user:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to create user');
      } else {
        toast.error('Failed to create user');
      }
    } finally {
      setIsLoadingCreateUser(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
        <Header />
        <div className="p-4">
          <div className="max-w-2xl mx-auto">
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              Loading account information...
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
      <Header />
      <div className="p-4">
        <div className="max-w-2xl mx-auto">
          <h1 className="text-2xl font-semibold text-[var(--color-fg)] mb-6">
            Account
          </h1>

          <div className="space-y-6">
            {/* User Information */}
            <div>
              <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                User Information
              </div>
              <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4">
                <div className="space-y-2">
                  {currentUser?.id && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        User ID
                      </div>
                      <div className="flex items-center gap-2">
                        <IdTag id={currentUser.id} />
                      </div>
                    </div>
                  )}
                  {currentUser?.username && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Username
                      </div>
                      <div className="text-[var(--color-fg)]">
                        {currentUser.username}
                      </div>
                    </div>
                  )}
                  {currentUser?.email && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Email
                      </div>
                      <div className="text-[var(--color-fg)]">
                        {currentUser.email}
                      </div>
                    </div>
                  )}
                  {currentUser?.roles && currentUser.roles.length > 0 && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Roles
                      </div>
                      <div className="flex flex-wrap gap-2">
                        {currentUser.roles.map((role, index) => (
                          <span
                            key={index}
                            className="px-2 py-1 bg-[var(--elevation-level-3-dark)] border border-[var(--color-border)] rounded text-sm text-[var(--color-fg)]"
                          >
                            {role}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Change Password */}
            <div>
              <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                Change Password
              </div>
              <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4">
                <form onSubmit={handlePasswordChange} className="space-y-3">
                  <div className="relative">
                    <label className="text-sm font-medium text-[var(--color-fg)] mb-1 block">
                      New Password
                    </label>
                    <div className="relative">
                      <input
                        type={showNewPassword ? 'text' : 'password'}
                        value={passwordData.newPassword}
                        onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                        placeholder="Enter new password"
                        disabled={isLoadingPassword}
                        required
                        autoComplete="new-password"
                        className="w-full px-4 py-3 pr-10 rounded-md bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)] border border-[var(--color-border)] transition-all focus:outline-none focus:border-[var(--elevation-level-4-dark)] focus:shadow-[0_0_8px_2px_rgba(121,94,169,0.15)] placeholder:text-[var(--color-fg)] placeholder:opacity-50 disabled:opacity-60 disabled:cursor-not-allowed"
                      />
                      <IconButton
                        type="button"
                        className="absolute right-3 top-1/2 -translate-y-1/2 z-10"
                        onClick={() => setShowNewPassword(!showNewPassword)}
                        disabled={isLoadingPassword}
                        size="sm"
                      >
                        {showNewPassword ? <IconEyeOff size={16} /> : <IconEye size={16} />}
                      </IconButton>
                    </div>
                  </div>
                  <div className="relative">
                    <label className="text-sm font-medium text-[var(--color-fg)] mb-1 block">
                      Confirm Password
                    </label>
                    <div className="relative">
                      <input
                        type={showConfirmPassword ? 'text' : 'password'}
                        value={passwordData.confirmPassword}
                        onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                        placeholder="Confirm new password"
                        disabled={isLoadingPassword}
                        required
                        autoComplete="new-password"
                        className="w-full px-4 py-3 pr-10 rounded-md bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)] border border-[var(--color-border)] transition-all focus:outline-none focus:border-[var(--elevation-level-4-dark)] focus:shadow-[0_0_8px_2px_rgba(121,94,169,0.15)] placeholder:text-[var(--color-fg)] placeholder:opacity-50 disabled:opacity-60 disabled:cursor-not-allowed"
                      />
                      <IconButton
                        type="button"
                        className="absolute right-3 top-1/2 -translate-y-1/2 z-10"
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        disabled={isLoadingPassword}
                        size="sm"
                      >
                        {showConfirmPassword ? <IconEyeOff size={16} /> : <IconEye size={16} />}
                      </IconButton>
                    </div>
                  </div>
                  {passwordError && (
                    <span className="text-sm text-danger-dark">{passwordError}</span>
                  )}
                  <Button
                    type="submit"
                    variant="outlinePrimary"
                    loading={isLoadingPassword}
                    disabled={isLoadingPassword}
                    className="w-full md:w-auto"
                  >
                    Update Password
                  </Button>
                </form>
              </div>
            </div>

            {/* Admin Section */}
            {isAdmin && (
              <div>
                <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                  Admin
                </div>
                <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4 space-y-4">
                  {userCount !== null && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Total Users
                      </div>
                      <div className="text-[var(--color-fg)] text-lg font-semibold">
                        {userCount}
                      </div>
                    </div>
                  )}

                  {!showCreateUserForm ? (
                    <Button
                      type="button"
                      variant="outlinePrimary"
                      onClick={() => setShowCreateUserForm(true)}
                      icon={<IconUserPlus size={20} />}
                      className="w-full md:w-auto"
                    >
                      Create New User
                    </Button>
                  ) : (
                    <div className="space-y-3">
                      <div className="flex items-center justify-between">
                        <div className="text-sm font-medium text-[var(--color-fg)]">
                          Create New User
                        </div>
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => {
                            setShowCreateUserForm(false);
                            setCreateUserData({
                              username: '',
                              email: '',
                              password: '',
                              roles: [],
                            });
                            setCreateUserErrors({});
                          }}
                        >
                          Cancel
                        </Button>
                      </div>
                      <form onSubmit={handleCreateUser} className="space-y-3">
                        <Input
                          label="Username"
                          type="text"
                          placeholder="Enter username"
                          value={createUserData.username || ''}
                          onChange={(e) => setCreateUserData({ ...createUserData, username: e.target.value })}
                          error={createUserErrors.username}
                          required
                          disabled={isLoadingCreateUser}
                          autoComplete="off"
                        />
                        <Input
                          label="Email"
                          type="email"
                          placeholder="Enter email"
                          value={createUserData.email || ''}
                          onChange={(e) => setCreateUserData({ ...createUserData, email: e.target.value })}
                          error={createUserErrors.email}
                          required
                          disabled={isLoadingCreateUser}
                          autoComplete="off"
                        />
                        <div className="relative">
                          <label className="text-sm font-medium text-[var(--color-fg)] mb-1 block">
                            Password
                          </label>
                          <div className="relative">
                            <input
                              type={showCreateUserPassword ? 'text' : 'password'}
                              value={createUserData.password || ''}
                              onChange={(e) => setCreateUserData({ ...createUserData, password: e.target.value })}
                              placeholder="Enter password"
                              disabled={isLoadingCreateUser}
                              required
                              autoComplete="new-password"
                              className="w-full px-4 py-3 pr-10 rounded-md bg-[var(--elevation-level-3-dark)] text-[var(--color-fg)] border border-[var(--color-border)] transition-all focus:outline-none focus:border-[var(--elevation-level-4-dark)] focus:shadow-[0_0_8px_2px_rgba(121,94,169,0.15)] placeholder:text-[var(--color-fg)] placeholder:opacity-50 disabled:opacity-60 disabled:cursor-not-allowed"
                            />
                            <IconButton
                              type="button"
                              className="absolute right-3 top-1/2 -translate-y-1/2 z-10"
                              onClick={() => setShowCreateUserPassword(!showCreateUserPassword)}
                              disabled={isLoadingCreateUser}
                              size="sm"
                            >
                              {showCreateUserPassword ? <IconEyeOff size={16} /> : <IconEye size={16} />}
                            </IconButton>
                          </div>
                          {createUserErrors.password && (
                            <span className="text-sm text-danger-dark mt-1 block">{createUserErrors.password}</span>
                          )}
                        </div>
                        <Button
                          type="submit"
                          variant="outlinePrimary"
                          loading={isLoadingCreateUser}
                          disabled={isLoadingCreateUser}
                          icon={<IconUserPlus size={20} />}
                          className="w-full md:w-auto"
                        >
                          Create User
                        </Button>
                      </form>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Logout Section */}
            <div>
              <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                Session
              </div>
              <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4">
                <Button
                  type="button"
                  variant="outlineDanger"
                  onClick={() => {
                    logout();
                    navigate('/login');
                    toast.success('Logged out successfully');
                  }}
                  icon={<IconLogout size={20} />}
                  className="w-full md:w-auto"
                >
                  Logout
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
