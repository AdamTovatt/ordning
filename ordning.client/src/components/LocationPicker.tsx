import { useState, useEffect, useCallback } from 'react';
import { IconMapPin, IconSearch } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Button, Input } from './ui';
import { Modal } from './ui/Modal';
import { LocationTree } from './LocationTree';
import toast from 'react-hot-toast';

type Location = components['schemas']['Location'];
type LocationTreeNode = components['schemas']['LocationTreeNode'];
type LocationSearchResponse = components['schemas']['LocationSearchResponse'];

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
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [searchResults, setSearchResults] = useState<Location[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);

  const hasSearchQuery = searchQuery.trim().length > 0;

  const performSearch = useCallback(async (query: string) => {
    if (!query.trim()) {
      setSearchResults([]);
      return;
    }

    setIsSearching(true);
    try {
      const responsePromise = apiClient.GET('/api/Location/search', {
        params: {
          query: {
            q: query.trim(),
            limit: 50,
            offset: 0,
          },
        },
      });

      const data = await unwrapResponse<LocationSearchResponse>(responsePromise);
      setSearchResults(data.results || []);
    } catch (error) {
      console.error('Search failed:', error);
      toast.error('Failed to search locations');
      setSearchResults([]);
    } finally {
      setIsSearching(false);
    }
  }, []);

  useEffect(() => {
    if (isOpen) {
      setSearchQuery('');
      setSearchResults([]);
      fetchLocationTree();
    }
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) return;

    if (hasSearchQuery) {
      const timeoutId = setTimeout(() => {
        performSearch(searchQuery);
      }, 300);

      return () => clearTimeout(timeoutId);
    } else {
      setSearchResults([]);
      setIsSearching(false);
    }
  }, [searchQuery, hasSearchQuery, performSearch, isOpen]);

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
          <div className="relative">
            <IconSearch 
              className="absolute left-4 top-1/2 -translate-y-1/2 text-[var(--color-fg)] opacity-50" 
              size={20} 
            />
            <Input
              type="text"
              placeholder="Search locations..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-12"
            />
          </div>

          {hasSearchQuery ? (
            <>
              {isSearching && (
                <div className="text-center py-8 text-[var(--color-fg)] opacity-70">
                  Searching...
                </div>
              )}

              {!isSearching && searchResults.length === 0 && (
                <div className="text-center py-8 text-[var(--color-fg)] opacity-70">
                  No locations found
                </div>
              )}

              {!isSearching && searchResults.length > 0 && (
                <div className="max-h-[60vh] overflow-y-auto space-y-2">
                  {searchResults.map((location) => (
                    <div
                      key={location.id}
                      className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4 cursor-pointer transition-colors hover:bg-[var(--elevation-level-3-dark)]"
                      onClick={() => location.id && handleSelectLocation(location)}
                    >
                      <div className="text-[var(--color-fg)] font-medium">{location.name || 'Unnamed Location'}</div>
                      {location.description && (
                        <div className="text-[var(--color-fg)] opacity-70 text-sm mt-1">
                          {location.description}
                        </div>
                      )}
                      {location.id && (
                        <div className="text-[var(--color-fg)] opacity-50 text-xs mt-2">
                          ID: {location.id}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </>
          ) : (
            <>
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
            </>
          )}
        </div>
      </Modal>
    </>
  );
}
