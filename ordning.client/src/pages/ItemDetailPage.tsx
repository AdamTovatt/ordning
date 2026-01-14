import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { IconArrowLeft, IconTrash, IconMapPin, IconInfoCircle, IconChevronRight } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Button } from '../components/ui';
import { Header } from '../components/Header';
import { IdTag } from '../components/IdTag';
import toast from 'react-hot-toast';

type Item = components['schemas']['Item'];
type Location = components['schemas']['Location'];

export function ItemDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [item, setItem] = useState<Item | null>(null);
  const [location, setLocation] = useState<Location | null>(null);
  const [locationPath, setLocationPath] = useState<Location[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isLoadingLocation, setIsLoadingLocation] = useState<boolean>(false);
  const [isLoadingPath, setIsLoadingPath] = useState<boolean>(false);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  useEffect(() => {
    if (id) {
      fetchItem();
    }
  }, [id]);

  useEffect(() => {
    if (item?.locationId) {
      fetchLocation(item.locationId);
    } else {
      setLocation(null);
      setLocationPath([]);
    }
  }, [item?.locationId]);

  useEffect(() => {
    if (location) {
      fetchLocationPath(location);
    } else {
      setLocationPath([]);
    }
  }, [location]);

  const fetchItem = async () => {
    if (!id) return;

    setIsLoading(true);
    try {
      const responsePromise = apiClient.GET('/api/Item/{id}', {
        params: {
          path: {
            id: id,
          },
        },
      });

      const data = await unwrapResponse<Item>(responsePromise);
      setItem(data);
    } catch (error) {
      console.error('Failed to fetch item:', error);
      toast.error('Failed to load item');
      navigate('/dashboard');
    } finally {
      setIsLoading(false);
    }
  };

  const fetchLocation = async (locationId: string) => {
    setIsLoadingLocation(true);
    try {
      const responsePromise = apiClient.GET('/api/Location/{id}', {
        params: {
          path: {
            id: locationId,
          },
        },
      });

      const data = await unwrapResponse<Location>(responsePromise);
      setLocation(data);
    } catch (error) {
      console.error('Failed to fetch location:', error);
      // Don't show error toast for location, just leave it as null
      setLocation(null);
    } finally {
      setIsLoadingLocation(false);
    }
  };

  const fetchLocationPath = async (currentLocation: Location) => {
    if (!currentLocation.id) {
      setLocationPath([]);
      return;
    }

    setIsLoadingPath(true);
    try {
      const responsePromise = apiClient.GET('/api/Location/{id}/path', {
        params: {
          path: {
            id: currentLocation.id,
          },
        },
      });

      const path = await unwrapResponse<Location[]>(responsePromise);
      setLocationPath(path || []);
    } catch (error) {
      console.error('Failed to fetch location path:', error);
      setLocationPath([currentLocation]); // At least show the current location
    } finally {
      setIsLoadingPath(false);
    }
  };

  const handleDelete = async () => {
    if (!id || !item) return;

    const confirmed = window.confirm(`Are you sure you want to delete "${item.name || 'this item'}"? This action cannot be undone.`);
    if (!confirmed) return;

    setIsDeleting(true);
    try {
      const response = await apiClient.DELETE('/api/Item/{id}', {
        params: {
          path: {
            id: id,
          },
        },
      });

      if (response.error) {
        const errorMessage = typeof response.error === 'string' 
          ? response.error 
          : (response.error as { message?: string; detail?: string; title?: string })?.message 
            || (response.error as { message?: string; detail?: string; title?: string })?.detail 
            || (response.error as { message?: string; detail?: string; title?: string })?.title 
            || 'Failed to delete item';
        throw new Error(errorMessage);
      }

      // DELETE returns 204 No Content, so we just check for success
      if (response.response.status === 204 || response.response.ok) {
        toast.success('Item deleted successfully');
        navigate('/dashboard');
      } else {
        throw new Error('Failed to delete item');
      }
    } catch (error) {
      console.error('Failed to delete item:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to delete item');
      } else {
        toast.error('Failed to delete item');
      }
    } finally {
      setIsDeleting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
        <Header />
        <div className="p-4">
          <div className="max-w-2xl mx-auto">
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              Loading item...
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!item) {
    return (
      <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
        <Header />
        <div className="p-4">
          <div className="max-w-2xl mx-auto">
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              Item not found
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
          <div className="mb-6">
            <Button
              variant="ghost"
              onClick={() => navigate('/dashboard')}
              icon={<IconArrowLeft size={20} />}
              size="sm"
            >
              Back
            </Button>
          </div>

          <h1 className="text-2xl font-semibold text-[var(--color-fg)] mb-2">
            {item.name || 'Unnamed Item'}
          </h1>

          {item.description && (
            <p className="text-[var(--color-fg)] opacity-70 mb-6">
              {item.description}
            </p>
          )}

          <div className="space-y-6">
            {item.locationId && (
              <div>
                <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-2">
                  Found at:
                </div>
                <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4 relative">
                  <IconMapPin 
                    className="absolute top-4 right-4 text-[var(--color-fg)] opacity-40" 
                    size={20} 
                  />
                  {isLoadingLocation ? (
                    <div className="text-[var(--color-fg)] opacity-60 text-sm">
                      Loading location...
                    </div>
                  ) : location ? (
                    <div className="space-y-1 pr-6">
                      <div className="text-[var(--color-fg)] font-medium flex items-center gap-2 flex-wrap">
                        {location.id && <IdTag id={location.id} />}
                        <span>{location.name || location.id || 'Unnamed Location'}</span>
                      </div>
                      {location.description && (
                        <div className="text-[var(--color-fg)] opacity-70 text-sm">
                          {location.description}
                        </div>
                      )}
                      {location.id && (
                        <div className="text-[var(--color-fg)] opacity-50 text-xs font-mono mt-1">
                          ID: {location.id}
                        </div>
                      )}
                      {locationPath.length > 1 && (
                        <div className="pt-2 mt-2 border-t border-[var(--color-border)]">
                          {isLoadingPath ? (
                            <div className="text-[var(--color-fg)] opacity-60 text-xs">
                              Loading path...
                            </div>
                          ) : (
                            <div className="text-[var(--color-fg)] opacity-60 text-xs flex items-center gap-1 flex-wrap">
                              {locationPath.map((pathLocation, index) => (
                                <span key={pathLocation.id || index} className="flex items-center gap-1">
                                  {index > 0 && (
                                    <IconChevronRight size={12} className="opacity-40" />
                                  )}
                                  <span>{pathLocation.name || pathLocation.id || 'Unnamed Location'}</span>
                                </span>
                              ))}
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  ) : (
                    <div className="text-[var(--color-fg)] opacity-60 text-sm pr-6">
                      {item.locationId}
                    </div>
                  )}
                </div>
              </div>
            )}

            <div>
              <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-2">
                Item information:
              </div>
              <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4 relative">
                <IconInfoCircle 
                  className="absolute top-4 right-4 text-[var(--color-fg)] opacity-40" 
                  size={20} 
                />
                <div className="pr-6 space-y-4">
                  {item.createdAt && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Created At
                      </div>
                      <div className="text-[var(--color-fg)] opacity-60 text-sm">
                        {new Date(item.createdAt).toLocaleString()}
                      </div>
                    </div>
                  )}

                  {item.properties && Object.keys(item.properties).length > 0 && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-2">
                        Properties
                      </div>
                      <div className="space-y-2">
                        {Object.entries(item.properties).map(([key, value]) => (
                          <div key={key} className="flex gap-2">
                            <div className="text-[var(--color-fg)] font-medium min-w-[100px]">
                              {key}:
                            </div>
                            <div className="text-[var(--color-fg)] opacity-80">
                              {value}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>

            <div className="flex gap-3 pt-4">
              <Button
                type="button"
                variant="danger"
                onClick={handleDelete}
                loading={isDeleting}
                disabled={isDeleting}
                icon={<IconTrash size={20} />}
                className="w-full md:w-auto"
              >
                Delete Item
              </Button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
