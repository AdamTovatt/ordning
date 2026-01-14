import { useLocation, useNavigate } from 'react-router-dom';
import { IconBox, IconMapPin } from '@tabler/icons-react';
import { Button } from './ui';

export function Header() {
  const navigate = useNavigate();
  const location = useLocation();

  const isItemsPage = location.pathname === '/dashboard' || location.pathname.startsWith('/items');
  const isLocationsPage = location.pathname.startsWith('/locations');

  return (
    <header className="bg-[var(--elevation-level-2-dark)] border-b border-[var(--color-border)] sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 py-3">
        <nav className="flex items-center gap-3">
          <Button
            variant={isItemsPage ? 'primary' : 'ghost'}
            onClick={() => navigate('/dashboard')}
            icon={<IconBox size={20} />}
            size="md"
          >
            Items
          </Button>
          <Button
            variant={isLocationsPage ? 'primary' : 'ghost'}
            onClick={() => navigate('/locations')}
            icon={<IconMapPin size={20} />}
            size="md"
          >
            Locations
          </Button>
        </nav>
      </div>
    </header>
  );
}
