import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { IconSearch, IconPlus } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Input, Button } from '../components/ui';
import { Header } from '../components/Header';
import { LocationTree } from '../components/LocationTree';
import toast from 'react-hot-toast';

type Location = components['schemas']['Location'];
type LocationSearchResponse = components['schemas']['LocationSearchResponse'];
type LocationTreeNode = components['schemas']['LocationTreeNode'];

export function LocationsPage() {
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [searchResults, setSearchResults] = useState<Location[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const [tree, setTree] = useState<LocationTreeNode[]>([]);
  const [isLoadingTree, setIsLoadingTree] = useState<boolean>(false);
  const navigate = useNavigate();

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

  const fetchLocationTree = useCallback(async () => {
    setIsLoadingTree(true);
    try {
      const responsePromise = apiClient.GET('/api/Location/tree');
      const data = await unwrapResponse<LocationTreeNode[]>(responsePromise);
      setTree(data || []);
    } catch (error) {
      console.error('Failed to fetch location tree:', error);
      toast.error('Failed to load locations');
      setTree([]);
    } finally {
      setIsLoadingTree(false);
    }
  }, []);

  useEffect(() => {
    if (hasSearchQuery) {
      const timeoutId = setTimeout(() => {
        performSearch(searchQuery);
      }, 300);

      return () => clearTimeout(timeoutId);
    } else {
      setSearchResults([]);
      setIsSearching(false);
    }
  }, [searchQuery, hasSearchQuery, performSearch]);

  useEffect(() => {
    if (!hasSearchQuery) {
      fetchLocationTree();
    }
  }, [hasSearchQuery, fetchLocationTree]);

  const handleLocationClick = (location: Location) => {
    if (location.id) {
      navigate(`/locations/${location.id}`);
    }
  };

  return (
    <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
      <Header />
      <div className="p-4">
        <div className="max-w-4xl mx-auto">
          <div className="mb-6 flex flex-col md:flex-row gap-3">
            <div className="relative flex-1">
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
            <Button
              onClick={() => navigate('/locations/add')}
              icon={<IconPlus size={20} />}
              variant="outlinePrimary"
              className="w-full md:w-auto shrink-0"
            >
              Add Location
            </Button>
          </div>

          {hasSearchQuery ? (
            <>
              {isSearching && (
                <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
                  Searching...
                </div>
              )}

              {!isSearching && searchResults.length === 0 && (
                <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
                  No locations found
                </div>
              )}

              {!isSearching && searchResults.length > 0 && (
                <div className="space-y-2">
                  {searchResults.map((location) => (
                    <div
                      key={location.id}
                      className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4 cursor-pointer hover:bg-[var(--elevation-level-3-dark)] transition-colors"
                      onClick={() => location.id && navigate(`/locations/${location.id}`)}
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
              {isLoadingTree ? (
                <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
                  Loading locations...
                </div>
              ) : (
                <div className="border border-[var(--color-border)] rounded-lg p-2 bg-[var(--elevation-level-1-dark)]">
                  <LocationTree
                    nodes={tree}
                    onSelectLocation={handleLocationClick}
                  />
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}
