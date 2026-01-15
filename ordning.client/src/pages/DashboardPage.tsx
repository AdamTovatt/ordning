import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { IconSearch, IconPlus } from '@tabler/icons-react';
import { apiClient, unwrapResponse } from '../services/apiClient';
import type { components } from '../types/api';
import { Input, Button } from '../components/ui';
import { Header } from '../components/Header';
import toast from 'react-hot-toast';

type Item = components['schemas']['Item'];
type ItemSearchResponse = components['schemas']['ItemSearchResponse'];

export function DashboardPage() {
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [searchResults, setSearchResults] = useState<Item[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const navigate = useNavigate();

  const performSearch = useCallback(async (query: string) => {
    setIsSearching(true);
    try {
      const responsePromise = apiClient.GET('/api/Item/search', {
        params: {
          query: {
            q: query || '',
            limit: 50,
            offset: 0,
          },
        },
      });

      const data = await unwrapResponse<ItemSearchResponse>(responsePromise);
      setSearchResults(data.results || []);
    } catch (error) {
      console.error('Search failed:', error);
      toast.error('Failed to search items');
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

  useEffect(() => {
    performSearch('');
  }, [performSearch]);

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
              placeholder="Search items..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-12"
            />
          </div>
          <Button
            onClick={() => navigate('/items/add')}
            icon={<IconPlus size={20} />}
            variant="outlinePrimary"
            className="w-full md:w-auto shrink-0"
          >
            Add Item
          </Button>
        </div>

        {isSearching && searchResults.length === 0 && (
          <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
            Searching...
          </div>
        )}

        {!isSearching && searchResults.length === 0 && (
          <div className="text-[var(--color-fg)] opacity-70 text-center py-8">
            No items found
          </div>
        )}

        {searchResults.length > 0 && (
          <div className="space-y-2">
            {searchResults.map((item) => (
              <div
                key={item.id}
                className="bg-[var(--elevation-level-2-dark)] border border-[var(--color-border)] rounded-md p-4 cursor-pointer hover:bg-[var(--elevation-level-3-dark)] transition-colors"
                onClick={() => item.id && navigate(`/items/${item.id}`)}
              >
                <div className="flex items-center justify-between gap-4">
                  <div className="text-[var(--color-fg)] font-medium">{item.name || 'Unnamed Item'}</div>
                  {item.locationId && (
                    <div className="text-[var(--color-fg)] opacity-50 text-sm shrink-0">
                      {item.locationId}
                    </div>
                  )}
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
    </div>
  );
}
