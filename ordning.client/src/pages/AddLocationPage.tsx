import { useState, useEffect, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { IconArrowLeft } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Input, Textarea, Button } from '../components/ui';
import { Header } from '../components/Header';
import { LocationPicker } from '../components/LocationPicker';
import toast from 'react-hot-toast';

type Location = components['schemas']['Location'];
type CreateLocationRequest = components['schemas']['CreateLocationRequest'];

export function AddLocationPage() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const [formData, setFormData] = useState<CreateLocationRequest>({
    id: '',
    name: '',
    description: '',
    parentLocationId: localStorage.getItem('lastSelectedLocationId') || '',
  });

  useEffect(() => {
    const lastLocationId = localStorage.getItem('lastSelectedLocationId');
    if (lastLocationId) {
      setFormData((prev) => ({ ...prev, parentLocationId: lastLocationId }));
    }
  }, []);

  const [errors, setErrors] = useState<{
    id?: string;
    name?: string;
    description?: string;
    parentLocationId?: string;
  }>({});

  const validateForm = (): boolean => {
    const newErrors: typeof errors = {};

    if (!formData.id?.trim()) {
      newErrors.id = 'ID is required';
    }

    if (!formData.name?.trim()) {
      newErrors.name = 'Name is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsLoading(true);
    try {
      const requestData: CreateLocationRequest = {
        id: formData.id?.trim() || null,
        name: formData.name?.trim() || null,
        description: formData.description?.trim() || null,
        parentLocationId: formData.parentLocationId?.trim() || null,
      };

      const responsePromise = apiClient.POST('/api/Location', {
        body: requestData,
      });

      await unwrapResponse<Location>(responsePromise);
      setIsLoading(false);
      toast.success('Location created successfully');
      navigate('/locations');
    } catch (error) {
      console.error('Failed to create location:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to create location');
      } else {
        toast.error('Failed to create location');
      }
    } finally {
      setIsLoading(false);
    }
  };

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

          <h1 className="text-2xl font-semibold text-[var(--color-fg)] mb-3">
            Add Location
          </h1>

          <form onSubmit={handleSubmit} className="space-y-3">
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-[var(--color-fg)]">
                Parent Location
              </label>
              <LocationPicker
                selectedLocationId={formData.parentLocationId}
                onSelectLocation={(location) => {
                  setFormData({ ...formData, parentLocationId: location?.id || null });
                }}
                allowNone={true}
                disabled={isLoading}
              />
              {errors.parentLocationId && (
                <span className="text-sm text-danger-dark">{errors.parentLocationId}</span>
              )}
            </div>

            <Input
              label="ID"
              type="text"
              placeholder="Enter a unique location ID"
              value={formData.id || ''}
              onChange={(e) => setFormData({ ...formData, id: e.target.value })}
              error={errors.id}
              required
              disabled={isLoading}
            />

            <Input
              label="Name"
              type="text"
              placeholder="Enter location name"
              value={formData.name || ''}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              error={errors.name}
              required
              disabled={isLoading}
            />

            <Textarea
              label="Description"
              placeholder="Enter location description (optional)"
              value={formData.description || ''}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              error={errors.description}
              disabled={isLoading}
            />

            <div className="flex gap-3 pt-2">
              <Button
                type="submit"
                variant="primary"
                loading={isLoading}
                disabled={isLoading}
                className="flex-1 md:flex-initial"
              >
                Create Location
              </Button>
              <Button
                type="button"
                variant="secondary"
                onClick={() => navigate('/locations')}
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
