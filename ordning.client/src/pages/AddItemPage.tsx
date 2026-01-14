import { useState, useEffect, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { IconArrowLeft, IconPlus, IconX } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Input, Textarea, Button } from '../components/ui';
import { Header } from '../components/Header';
import { LocationPicker } from '../components/LocationPicker';
import toast from 'react-hot-toast';

type Location = components['schemas']['Location'];
type Item = components['schemas']['Item'];
type CreateItemRequest = components['schemas']['CreateItemRequest'];

export function AddItemPage() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const [formData, setFormData] = useState<CreateItemRequest>({
    name: '',
    description: '',
    locationId: localStorage.getItem('lastSelectedLocationId') || '',
    properties: null,
  });

  useEffect(() => {
    const lastLocationId = localStorage.getItem('lastSelectedLocationId');
    if (lastLocationId) {
      setFormData((prev) => ({ ...prev, locationId: lastLocationId }));
    }
  }, []);

  const [properties, setProperties] = useState<Array<{ key: string; value: string }>>([
    { key: '', value: '' },
  ]);

  const [errors, setErrors] = useState<{
    name?: string;
    locationId?: string;
  }>({});

  const validateForm = (): boolean => {
    const newErrors: typeof errors = {};

    if (!formData.name?.trim()) {
      newErrors.name = 'Name is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handlePropertyChange = (index: number, field: 'key' | 'value', value: string) => {
    const newProperties = [...properties];
    newProperties[index] = { ...newProperties[index], [field]: value };
    setProperties(newProperties);
  };

  const addProperty = () => {
    setProperties([...properties, { key: '', value: '' }]);
  };

  const removeProperty = (index: number) => {
    const newProperties = properties.filter((_, i) => i !== index);
    setProperties(newProperties.length > 0 ? newProperties : [{ key: '', value: '' }]);
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsLoading(true);
    try {
      const propertiesObj: { [key: string]: string } | null = (() => {
        const filtered = properties.filter((p) => p.key.trim() && p.value.trim());
        if (filtered.length === 0) return null;
        const obj: { [key: string]: string } = {};
        filtered.forEach((p) => {
          obj[p.key.trim()] = p.value.trim();
        });
        return obj;
      })();

      const requestData: CreateItemRequest = {
        name: formData.name?.trim() || null,
        description: formData.description?.trim() || null,
        locationId: formData.locationId?.trim() || null,
        properties: propertiesObj,
      };

      const responsePromise = apiClient.POST('/api/Item', {
        body: requestData,
      });

      await unwrapResponse<Item>(responsePromise);
      setIsLoading(false);
      toast.success('Item created successfully');
      navigate('/dashboard');
    } catch (error) {
      console.error('Failed to create item:', error);
      if (error instanceof Error) {
        toast.error(error.message || 'Failed to create item');
      } else {
        toast.error('Failed to create item');
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

          <h1 className="text-2xl font-semibold text-[var(--color-fg)] mb-6">
            Add Item
          </h1>

          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium text-[var(--color-fg)]">
                Location
              </label>
              <LocationPicker
                selectedLocationId={formData.locationId}
                onSelectLocation={(location) => {
                  setFormData({ ...formData, locationId: location?.id || null });
                }}
                allowNone={true}
                disabled={isLoading}
              />
              {errors.locationId && (
                <span className="text-sm text-danger-dark">{errors.locationId}</span>
              )}
            </div>

            <Input
              label="Name"
              type="text"
              placeholder="Enter item name"
              value={formData.name || ''}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              error={errors.name}
              required
              disabled={isLoading}
            />

            <Textarea
              label="Description"
              placeholder="Enter item description (optional)"
              value={formData.description || ''}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              disabled={isLoading}
            />

            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <label className="text-sm font-medium text-[var(--color-fg)]">
                  Properties (optional)
                </label>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={addProperty}
                  icon={<IconPlus size={16} />}
                  disabled={isLoading}
                >
                  Add Property
                </Button>
              </div>
              {properties.map((property, index) => (
                <div key={index} className="flex gap-2 items-start">
                  <Input
                    type="text"
                    placeholder="Key"
                    value={property.key}
                    onChange={(e) => handlePropertyChange(index, 'key', e.target.value)}
                    disabled={isLoading}
                    className="flex-1"
                  />
                  <Input
                    type="text"
                    placeholder="Value"
                    value={property.value}
                    onChange={(e) => handlePropertyChange(index, 'value', e.target.value)}
                    disabled={isLoading}
                    className="flex-1"
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => removeProperty(index)}
                    disabled={isLoading || properties.length === 1}
                    icon={<IconX size={16} />}
                    className="shrink-0"
                  />
                </div>
              ))}
            </div>

            <div className="flex gap-3 pt-4">
              <Button
                type="submit"
                variant="primary"
                loading={isLoading}
                disabled={isLoading}
              >
                Create Item
              </Button>
              <Button
                type="button"
                variant="secondary"
                onClick={() => navigate('/dashboard')}
                disabled={isLoading}
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
