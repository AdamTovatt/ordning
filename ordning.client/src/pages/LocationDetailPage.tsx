import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { IconArrowLeft, IconTrash, IconMapPin, IconInfoCircle, IconPlus } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Button, ConfirmationModal } from '../components/ui';
import { Header } from '../components/Header';
import { IdTag } from '../components/IdTag';
import toast from 'react-hot-toast';

type Location = components['schemas']['Location'];
type Item = components['schemas']['Item'];

export function LocationDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [location, setLocation] = useState<Location | null>(null);
  const [parentLocation, setParentLocation] = useState<Location | null>(null);
  const [items, setItems] = useState<Item[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isLoadingParent, setIsLoadingParent] = useState<boolean>(false);
  const [isLoadingItems, setIsLoadingItems] = useState<boolean>(false);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState<boolean>(false);

  useEffect(() => {
    if (id) {
      fetchLocation();
      fetchItems();
    }
  }, [id]);

  useEffect(() => {
    if (location?.parentLocationId) {
      fetchParentLocation(location.parentLocationId);
    } else {
      setParentLocation(null);
    }
  }, [location?.parentLocationId]);

  const fetchLocation = async () => {
    if (!id) return;

    setIsLoading(true);
    try {
      const responsePromise = apiClient.GET('/api/Location/{id}', {
        params: {
          path: {
            id: id,
          },
        },
      });

      const data = await unwrapResponse<Location>(responsePromise);
      setLocation(data);
    } catch (error) {
      console.error('Failed to fetch location:', error);
      toast.error('Failed to load location');
      navigate('/locations');
    } finally {
      setIsLoading(false);
    }
  };

  const fetchParentLocation = async (parentLocationId: string) => {
    setIsLoadingParent(true);
    try {
      const responsePromise = apiClient.GET('/api/Location/{id}', {
        params: {
          path: {
            id: parentLocationId,
          },
        },
      });

      const data = await unwrapResponse<Location>(responsePromise);
      setParentLocation(data);
    } catch (error) {
      console.error('Failed to fetch parent location:', error);
      setParentLocation(null);
    } finally {
      setIsLoadingParent(false);
    }
  };

  const fetchItems = async () => {
    if (!id) return;

    setIsLoadingItems(true);
    try {
      const responsePromise = apiClient.GET('/api/Item/location/{locationId}', {
        params: {
          path: {
            locationId: id,
          },
        },
      });

      const data = await unwrapResponse<Item[]>(responsePromise);
      setItems(data || []);
    } catch (error) {
      console.error('Failed to fetch items:', error);
      setItems([]);
    } finally {
      setIsLoadingItems(false);
    }
  };

  const handleAddItem = () => {
    if (location?.id) {
      localStorage.setItem('lastSelectedLocationId', location.id);
      navigate('/items/add');
    }
  };

  const handleAddLocation = () => {
    if (location?.id) {
      localStorage.setItem('lastSelectedLocationId', location.id);
      navigate('/locations/add');
    }
  };

  const handleDeleteClick = () => {
    setIsDeleteConfirmOpen(true);
  };

  const handleDelete = async () => {
    if (!id || !location) return;

    setIsDeleteConfirmOpen(false);
    setIsDeleting(true);
    try {
      const response = await apiClient.DELETE('/api/Location/{id}', {
        params: {
          path: {
            id: id,
          },
        },
      });

      const responseData = response as { error?: unknown; response: Response };
      
      // Check for 403 Forbidden status first
      if (responseData.response.status === 403) {
        throw new Error('You lack the required privileges to perform this action');
      }
      
      if (responseData.error) {
        const errorMessage = typeof responseData.error === 'string' 
          ? responseData.error 
          : (responseData.error as { message?: string; detail?: string; title?: string })?.message 
            || (responseData.error as { message?: string; detail?: string; title?: string })?.detail 
            || (responseData.error as { message?: string; detail?: string; title?: string })?.title 
            || 'Failed to delete location';
        throw new Error(errorMessage);
      }

      // DELETE returns 204 No Content, so we just check for success
      if (responseData.response.status === 204 || responseData.response.ok) {
        toast.success('Location deleted successfully');
        navigate('/locations');
      } else {
        throw new Error('Failed to delete location');
      }
    } catch (error) {
      console.error('Failed to delete location:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to delete location');
      } else {
        toast.error('Failed to delete location');
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
              Loading location...
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!location) {
    return (
      <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
        <Header />
        <div className="p-4">
          <div className="max-w-2xl mx-auto">
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              Location not found
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
          <div className="mb-3">
            <Button
              variant="ghost"
              onClick={() => navigate('/locations')}
              icon={<IconArrowLeft size={20} />}
              size="sm"
            >
              Back
            </Button>
          </div>

          <h1 className="text-2xl font-semibold text-[var(--color-fg)] mb-2 flex items-center gap-2 flex-wrap">
            {location.id && <IdTag id={location.id} />}
            <span>{location.name || location.id || 'Unnamed Location'}</span>
          </h1>

          {location.description && (
            <p className="text-[var(--color-fg)] opacity-70 mb-3">
              {location.description}
            </p>
          )}

          <div className="space-y-3">
            {location.parentLocationId && (
              <div>
                <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                  Found at:
                </div>
                <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4 relative">
                  <IconMapPin 
                    className="absolute top-4 right-4 text-[var(--color-fg)] opacity-40" 
                    size={20} 
                  />
                  {isLoadingParent ? (
                    <div className="text-[var(--color-fg)] opacity-60 text-sm pr-6">
                      Loading parent location...
                    </div>
                  ) : parentLocation ? (
                    <div className="space-y-1 pr-6">
                      <div className="text-[var(--color-fg)] font-medium">
                        {parentLocation.name || parentLocation.id || 'Unnamed Location'}
                      </div>
                      {parentLocation.description && (
                        <div className="text-[var(--color-fg)] opacity-70 text-sm">
                          {parentLocation.description}
                        </div>
                      )}
                      {parentLocation.id && (
                        <div className="text-[var(--color-fg)] opacity-50 text-xs font-mono mt-1">
                          ID: {parentLocation.id}
                        </div>
                      )}
                    </div>
                  ) : (
                    <div className="text-[var(--color-fg)] opacity-60 text-sm pr-6">
                      {location.parentLocationId}
                    </div>
                  )}
                </div>
              </div>
            )}

            <div className="flex gap-3 flex-col md:flex-row">
              <Button
                type="button"
                variant="outlinePrimary"
                onClick={handleAddLocation}
                icon={<IconPlus size={20} />}
                className="w-full md:flex-1"
              >
                Add location here
              </Button>
              <Button
                type="button"
                variant="outlinePrimary"
                onClick={handleAddItem}
                icon={<IconPlus size={20} />}
                className="w-full md:flex-1"
              >
                Add item here
              </Button>
            </div>

            <div>
              <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                Location information:
              </div>
              <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4 relative">
                <IconInfoCircle 
                  className="absolute top-4 right-4 text-[var(--color-fg)] opacity-40" 
                  size={20} 
                />
                <div className="pr-6 space-y-2">
                  {location.id && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Location ID
                      </div>
                      <div className="text-[var(--color-fg)] opacity-60 text-sm font-mono">
                        {location.id}
                      </div>
                    </div>
                  )}

                  {location.createdAt && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Created At
                      </div>
                      <div className="text-[var(--color-fg)] opacity-60 text-sm">
                        {new Date(location.createdAt).toLocaleString()}
                      </div>
                    </div>
                  )}

                  {location.updatedAt && (
                    <div>
                      <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                        Updated At
                      </div>
                      <div className="text-[var(--color-fg)] opacity-60 text-sm">
                        {new Date(location.updatedAt).toLocaleString()}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>

            <div>
              <div className="text-sm font-medium text-[var(--color-fg)] opacity-70 mb-1">
                Items in this location:
              </div>
              <div className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4">
                {isLoadingItems ? (
                  <div className="text-[var(--color-fg)] opacity-60 text-sm">
                    Loading items...
                  </div>
                ) : items.length === 0 ? (
                  <div className="text-[var(--color-fg)] opacity-60 text-sm">
                    No items in this location
                  </div>
                ) : (
                  <div className="space-y-1">
                    {items.map((item) => (
                      <div
                        key={item.id}
                        className="bg-[var(--elevation-level-3-dark)] border border-[var(--color-border)] rounded-md p-3 cursor-pointer hover:bg-[var(--elevation-level-4-dark)] transition-colors"
                        onClick={() => item.id && navigate(`/items/${item.id}`)}
                      >
                        <div className="text-[var(--color-fg)] font-medium">
                          {item.name || 'Unnamed Item'}
                        </div>
                        {item.description && (
                          <div className="text-[var(--color-fg)] opacity-70 text-sm mt-1">
                            {item.description}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>

            <div className="flex gap-3 pt-2">
              <Button
                type="button"
                variant="outlineDanger"
                onClick={handleDeleteClick}
                disabled={isDeleting}
                icon={<IconTrash size={20} />}
                className="w-full md:w-auto"
              >
                Delete Location
              </Button>
            </div>
          </div>
        </div>
      </div>

      <ConfirmationModal
        isOpen={isDeleteConfirmOpen}
        onClose={() => setIsDeleteConfirmOpen(false)}
        onConfirm={handleDelete}
        title="Delete Location"
        message={`Are you sure you want to delete "${location?.name || location?.id || 'this location'}"? This action cannot be undone.`}
        confirmText="Delete"
        cancelText="Cancel"
        variant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
}
