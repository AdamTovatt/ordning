import { useState, useEffect, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { IconArrowLeft } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Input, Textarea, Button } from '../components/ui';
import { Header } from '../components/Header';
import toast from 'react-hot-toast';

type Location = components['schemas']['Location'];
type UpdateLocationRequest = components['schemas']['UpdateLocationRequest'];

export function EditLocationPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isLoadingLocation, setIsLoadingLocation] = useState<boolean>(true);
  const [location, setLocation] = useState<Location | null>(null);

  const [formData, setFormData] = useState<UpdateLocationRequest>({
    name: '',
    description: '',
    parentLocationId: null,
  });

  const [errors, setErrors] = useState<{
    name?: string;
  }>({});

  useEffect(() => {
    if (id) {
      fetchLocation();
    }
  }, [id]);

  useEffect(() => {
    if (location) {
      setFormData({
        name: location.name || '',
        description: location.description || '',
        parentLocationId: location.parentLocationId || null,
      });
    }
  }, [location]);

  const fetchLocation = async () => {
    if (!id) return;

    setIsLoadingLocation(true);
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
      navigate(`/locations/${id}`);
    } finally {
      setIsLoadingLocation(false);
    }
  };

  const validateForm = (): boolean => {
    const newErrors: typeof errors = {};

    if (!formData.name?.trim()) {
      newErrors.name = 'Name is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();

    if (!validateForm() || !id) {
      return;
    }

    setIsLoading(true);
    try {
      const requestData: UpdateLocationRequest = {
        name: formData.name?.trim() || null,
        description: formData.description?.trim() || null,
        parentLocationId: formData.parentLocationId || null,
      };

      const responsePromise = apiClient.PUT('/api/Location/{id}', {
        params: {
          path: {
            id: id,
          },
        },
        body: requestData,
      });

      await unwrapResponse<Location>(responsePromise);
      setIsLoading(false);
      toast.success('Location updated successfully');
      navigate(`/locations/${id}`);
    } catch (error) {
      console.error('Failed to update location:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to update location');
      } else {
        toast.error('Failed to update location');
      }
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoadingLocation) {
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
              onClick={() => navigate(`/locations/${id}`)}
              icon={<IconArrowLeft size={20} />}
              size="sm"
            >
              Back
            </Button>
          </div>

          <h1 className="text-2xl font-semibold text-[var(--color-fg)] mb-3">
            Edit Location
          </h1>

          <form onSubmit={handleSubmit} className="space-y-3">
            <Input
              label="Name"
              type="text"
              placeholder="Enter location name"
              value={formData.name || ''}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              error={errors.name}
              required
              disabled={isLoading}
              autoComplete="off"
            />

            <Textarea
              label="Description"
              placeholder="Enter location description (optional)"
              value={formData.description || ''}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              disabled={isLoading}
            />

            <div className="flex gap-3 pt-2">
              <Button
                type="submit"
                variant="outlinePrimary"
                loading={isLoading}
                disabled={isLoading}
                className="flex-1 md:flex-initial"
              >
                Update Location
              </Button>
              <Button
                type="button"
                variant="secondary"
                onClick={() => navigate(`/locations/${id}`)}
                disabled={isLoading}
                className="flex-1 md:flex-initial"
              >
                Cancel
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
