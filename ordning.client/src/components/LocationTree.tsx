import { useState } from 'react';
import { IconChevronRight, IconChevronDown } from '@tabler/icons-react';
import type { components } from '../types/api';

type LocationTreeNode = components['schemas']['LocationTreeNode'];
type Location = components['schemas']['Location'];

interface LocationTreeProps {
  nodes: LocationTreeNode[];
  selectedLocationId?: string | null;
  onSelectLocation: (location: Location) => void;
  level?: number;
}

export function LocationTree({ nodes, selectedLocationId, onSelectLocation, level = 0 }: LocationTreeProps) {
  return (
    <div className="space-y-1">
      {nodes.map((node) => (
        <LocationTreeNode
          key={node.location?.id || ''}
          node={node}
          selectedLocationId={selectedLocationId}
          onSelectLocation={onSelectLocation}
          level={level}
        />
      ))}
    </div>
  );
}

interface LocationTreeNodeProps {
  node: LocationTreeNode;
  selectedLocationId?: string | null;
  onSelectLocation: (location: Location) => void;
  level: number;
}

function LocationTreeNode({ node, selectedLocationId, onSelectLocation, level }: LocationTreeNodeProps) {
  const [isExpanded, setIsExpanded] = useState<boolean>(true);
  const location = node.location;
  const hasChildren = node.children && node.children.length > 0;
  const isSelected = location?.id === selectedLocationId;

  if (!location) return null;

  const handleClick = () => {
    onSelectLocation(location);
  };

  const handleToggle = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (hasChildren) {
      setIsExpanded(!isExpanded);
    }
  };

  return (
    <div>
      <div
        className={`
          flex items-center gap-2 px-3 py-2 rounded-md cursor-pointer
          transition-colors
          ${isSelected 
            ? 'bg-[var(--elevation-level-3-dark)] border border-[var(--brand-color-light)]' 
            : 'hover:bg-[var(--elevation-level-3-dark)]'
          }
        `}
        style={{ paddingLeft: `${12 + level * 24}px` }}
        onClick={handleClick}
      >
        {hasChildren ? (
          <button
            onClick={handleToggle}
            className="flex items-center justify-center w-5 h-5 rounded hover:bg-[var(--elevation-level-4-dark)] transition-colors"
          >
            {isExpanded ? (
              <IconChevronDown size={16} className="text-[var(--color-fg)] opacity-70" />
            ) : (
              <IconChevronRight size={16} className="text-[var(--color-fg)] opacity-70" />
            )}
          </button>
        ) : (
          <div className="w-5" />
        )}
        <div className="flex-1 min-w-0">
          <div className={`text-[var(--color-fg)] font-medium ${isSelected ? 'text-[var(--brand-color-light)]' : ''}`}>
            {location.name || location.id || 'Unnamed Location'}
          </div>
          {location.description && (
            <div className="text-[var(--color-fg)] opacity-60 text-sm truncate">
              {location.description}
            </div>
          )}
        </div>
        {location.id && (
          <div className="text-[var(--color-fg)] opacity-40 text-xs shrink-0">
            {location.id}
          </div>
        )}
      </div>
      {hasChildren && isExpanded && (
        <LocationTree
          nodes={node.children || []}
          selectedLocationId={selectedLocationId}
          onSelectLocation={onSelectLocation}
          level={level + 1}
        />
      )}
    </div>
  );
}
