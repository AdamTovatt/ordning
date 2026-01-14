import { useState, useEffect } from 'react';
import { IconMapPin } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Button } from './ui';
import { Modal } from './ui/Modal';
import { LocationTree } from './LocationTree';
import toast from 'react-hot-toast';

type Location = components['schemas']['Location'];
type LocationTreeNode = components['schemas']['LocationTreeNode'];

export interface LocationPickerProps {
  selectedLocationId?: string | null;
  onSelectLocation: (location: Location | null) => void;
  allowNone?: boolean;
  disabled?: boolean;
}

export function LocationPicker({ selectedLocationId, onSelectLocation, allowNone = false, disabled = false }: LocationPickerProps) {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const [tree, setTree] = useState<LocationTreeNode[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [selectedLocation, setSelectedLocation] = useState<Location | null>(null);

  useEffect(() => {
    if (isOpen) {
      fetchLocationTree();
    }
  }, [isOpen]);

  useEffect(() => {
    if (selectedLocationId) {
      if (tree.length > 0) {
        const location = findLocationInTree(tree, selectedLocationId);
        setSelectedLocation(location || null);
      } else if (!isOpen) {
        fetchLocationForDisplay();
      }
    } else {
      setSelectedLocation(null);
    }
  }, [selectedLocationId, tree, isOpen]);

  useEffect(() => {
    if (selectedLocationId && tree.length === 0) {
      fetchLocationForDisplay();
    }
  }, [selectedLocationId]);

  useEffect(() => {
    if (selectedLocationId && !isOpen && tree.length === 0) {
      fetchLocationForDisplay();
    }
  }, []);

  const fetchLocationForDisplay = async () => {
    if (!selectedLocationId) return;
    
    try {
      const responsePromise = apiClient.GET('/api/Location/tree');
      const data = await unwrapResponse<LocationTreeNode[]>(responsePromise);
      setTree(data || []);
      const location = findLocationInTree(data || [], selectedLocationId);
      setSelectedLocation(location || null);
    } catch (error) {
      console.error('Failed to fetch location tree:', error);
    }
  };

  const fetchLocationTree = async () => {
    setIsLoading(true);
    try {
      const responsePromise = apiClient.GET('/api/Location/tree');
      const data = await unwrapResponse<LocationTreeNode[]>(responsePromise);
      setTree(data || []);
    } catch (error) {
      console.error('Failed to fetch location tree:', error);
      toast.error('Failed to load locations');
      setTree([]);
    } finally {
      setIsLoading(false);
    }
  };

  const findLocationInTree = (nodes: LocationTreeNode[], id: string): Location | null => {
    for (const node of nodes) {
      if (node.location?.id === id) {
        return node.location;
      }
      if (node.children) {
        const found = findLocationInTree(node.children, id);
        if (found) return found;
      }
    }
    return null;
  };

  const handleSelectLocation = (location: Location) => {
    setSelectedLocation(location);
    onSelectLocation(location);
    // Store last selected location in localStorage
    if (location?.id) {
      localStorage.setItem('lastSelectedLocationId', location.id);
    }
    setIsOpen(false);
  };

  const handleClear = () => {
    setSelectedLocation(null);
    onSelectLocation(null);
    setIsOpen(false);
  };

  const getDisplayText = (): string => {
    if (selectedLocation) {
      return selectedLocation.name || selectedLocation.id || 'Selected Location';
    }
    return 'Select location...';
  };

  return (
    <>
      <Button
        type="button"
        variant="secondary"
        onClick={() => setIsOpen(true)}
        disabled={disabled}
        icon={<IconMapPin size={20} />}
        className="w-full justify-start"
      >
        {getDisplayText()}
      </Button>

      <Modal
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        title="Select Location"
        className="max-w-2xl"
      >
        <div className="space-y-4">
          {isLoading ? (
            <div className="text-center py-8 text-[var(--color-fg)] opacity-70">
              Loading locations...
            </div>
          ) : (
            <>
              <div className="max-h-[60vh] overflow-y-auto border border-[var(--color-border)] rounded-lg p-2 bg-[var(--elevation-level-1-dark)]">
                <LocationTree
                  nodes={tree}
                  selectedLocationId={selectedLocationId}
                  onSelectLocation={handleSelectLocation}
                />
              </div>
              {allowNone && (
                <div className="flex justify-end">
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={handleClear}
                  >
                    Clear Selection
                  </Button>
                </div>
              )}
            </>
          )}
        </div>
      </Modal>
    </>
  );
}
