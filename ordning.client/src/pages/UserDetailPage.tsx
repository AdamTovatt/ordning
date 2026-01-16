import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { IconArrowLeft, IconX, IconPlus } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Button } from '../components/ui';
import { Header } from '../components/Header';
import { IdTag } from '../components/IdTag';
import toast from 'react-hot-toast';

type User = components['schemas']['User'];
type AddRoleRequest = components['schemas']['AddRoleRequest'];

const AVAILABLE_ROLES = ['admin', 'write'] as const;

export function UserDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isUpdating, setIsUpdating] = useState<boolean>(false);

  useEffect(() => {
    if (id) {
      fetchUser();
    }
  }, [id]);

  const fetchUser = async () => {
    if (!id) return;

    setIsLoading(true);
    try {
      // Get all users and find the one with matching ID
      const allUsers = await unwrapResponse<User[]>(apiClient.GET('/api/User'));
      const foundUser = allUsers.find((u) => u.id === id);
      
      if (!foundUser) {
        toast.error('User not found');
        navigate('/account');
        return;
      }

      setUser(foundUser);
    } catch (error) {
      console.error('Failed to fetch user:', error);
      toast.error('Failed to load user');
      navigate('/account');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddRole = async (role: string) => {
    if (!id || !user) return;

    setIsUpdating(true);
    try {
      const request: AddRoleRequest = {
        role: role,
      };

      const response = await apiClient.POST('/api/User/{id}/roles', {
        params: {
          path: {
            id: id,
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
            || 'Failed to add role';
        throw new Error(errorMessage);
      }

      if (responseData.response.status === 204 || responseData.response.ok) {
        toast.success(`Role "${role}" added successfully`);
        await fetchUser();
      } else {
        throw new Error('Failed to add role');
      }
    } catch (error) {
      console.error('Failed to add role:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to add role');
      } else {
        toast.error('Failed to add role');
      }
    } finally {
      setIsUpdating(false);
    }
  };

  const handleRemoveRole = async (role: string) => {
    if (!id || !user) return;

    setIsUpdating(true);
    try {
      const response = await apiClient.DELETE('/api/User/{id}/roles/{role}', {
        params: {
          path: {
            id: id,
            role: role,
          },
        },
      });

      const responseData = response as { error?: unknown; response: Response };
      
      if (responseData.error) {
        const errorMessage = typeof responseData.error === 'string' 
          ? responseData.error 
          : (responseData.error as { message?: string; detail?: string; title?: string })?.message 
            || (responseData.error as { message?: string; detail?: string; title?: string })?.detail 
            || (responseData.error as { message?: string; detail?: string; title?: string })?.title 
            || 'Failed to remove role';
        throw new Error(errorMessage);
      }

      if (responseData.response.status === 204 || responseData.response.ok) {
        toast.success(`Role "${role}" removed successfully`);
        await fetchUser();
      } else {
        throw new Error('Failed to remove role');
      }
    } catch (error) {
      console.error('Failed to remove role:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to remove role');
      } else {
        toast.error('Failed to remove role');
      }
    } finally {
      setIsUpdating(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
        <Header />
        <div className="p-4">
          <div className="max-w-2xl mx-auto">
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              Loading user...
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
        <Header />
        <div className="p-4">
          <div className="max-w-2xl mx-auto">
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              User not found
            </div>
          </div>
        </div>
      </div>
    );
  }

  const currentRoles = user.roles || [];
  const availableRolesToAdd = AVAILABLE_ROLES.filter((role) => !currentRoles.includes(role));

  return (
    <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
      <Header />
      <div className="p-4">
        <div className="max-w-2xl mx-auto">
          <div className="mb-6">
            <Button
              variant="ghost"
              onClick={() => navigate('/account')}
              icon={<IconArrowLeft size={20} />}
              size="sm"
            >
              Back
            </Button>
          </div>

          <h1 className="text-2xl font-semibold text-[var(--color-fg)] mb-2">
            {user.username || 'Unnamed User'}
          </h1>

          {user.email && (
            <p className="text-[var(--color-fg)] opacity-70 mb-6">
              {user.email}
            </p>
          )}

          <div className="space-y-6">
            <div>
              <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-2">
                User Information
              </div>
              <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4">
                <div className="space-y-2">
                  {user.id && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        User ID
                      </div>
                      <div className="flex items-center gap-2">
                        <IdTag id={user.id} />
                      </div>
                    </div>
                  )}
                  {user.username && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Username
                      </div>
                      <div className="text-[var(--color-fg)]">
                        {user.username}
                      </div>
                    </div>
                  )}
                  {user.email && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Email
                      </div>
                      <div className="text-[var(--color-fg)]">
                        {user.email}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>

            <div>
              <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-2">
                Roles
              </div>
              <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4">
                {currentRoles.length === 0 ? (
                  <div className="text-[var(--color-fg)] opacity-60 text-sm mb-3">
                    No roles assigned
                  </div>
                ) : (
                  <div className="flex flex-wrap gap-2 mb-3">
                    {currentRoles.map((role, index) => (
                      <div
                        key={index}
                        className="px-3 py-1.5 bg-[var(--elevation-level-3-dark)] border border-[var(--color-border)] rounded flex items-center gap-2"
                      >
                        <span className="text-sm text-[var(--color-fg)]">{role}</span>
                        <button
                          type="button"
                          onClick={() => handleRemoveRole(role)}
                          disabled={isUpdating}
                          className="text-[var(--color-fg)] opacity-60 hover:opacity-100 transition-opacity disabled:opacity-40 disabled:cursor-not-allowed"
                        >
                          <IconX size={16} />
                        </button>
                      </div>
                    ))}
                  </div>
                )}

                {availableRolesToAdd.length > 0 && (
                  <div>
                    <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-2">
                      Add Role
                    </div>
                    <div className="flex flex-wrap gap-2">
                      {availableRolesToAdd.map((role) => (
                        <Button
                          key={role}
                          type="button"
                          variant="outlinePrimary"
                          size="sm"
                          onClick={() => handleAddRole(role)}
                          disabled={isUpdating}
                          icon={<IconPlus size={16} />}
                        >
                          {role}
                        </Button>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
