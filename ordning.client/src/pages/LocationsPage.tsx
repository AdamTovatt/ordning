import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { IconSearch, IconPlus } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Input, Button } from '../components/ui';
import { Header } from '../components/Header';
import toast from 'react-hot-toast';

type Location = components['schemas']['Location'];
type LocationSearchResponse = components['schemas']['LocationSearchResponse'];

export function LocationsPage() {
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [searchResults, setSearchResults] = useState<Location[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const navigate = useNavigate();

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
            q: query,
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
    const timeoutId = setTimeout(() => {
      performSearch(searchQuery);
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchQuery, performSearch]);

  return (
    <div className="min-h-screen bg-[var(--elevation-level-1-dark)]">
      <Header />
      <div className="p-4">
        <div className="max-w-4xl mx-auto">
          <div className="mb-6">
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
          </div>

          <div className="mb-6 flex justify-center">
            <Button
              onClick={() => navigate('/locations/add')}
              icon={<IconPlus size={20} />}
              variant="primary"
              className="w-full md:w-auto"
            >
              Add Location
            </Button>
          </div>

          {isSearching && (
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              Searching...
            </div>
          )}

          {!isSearching && searchQuery && searchResults.length === 0 && (
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              No locations found
            </div>
          )}

          {!isSearching && searchQuery && searchResults.length > 0 && (
            <div className="space-y-2">
              {searchResults.map((location) => (
                <div
                  key={location.id}
                  className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4"
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

          {!searchQuery && (
            <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
              Start typing to search for locations
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
