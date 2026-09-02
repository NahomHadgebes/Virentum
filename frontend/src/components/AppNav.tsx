import { Tabs } from '@mantine/core';
import { useLocation, useNavigate } from 'react-router-dom';

/** Route path to tab label. The path is the tab value, so no second mapping. */
const TABS: ReadonlyArray<{ path: string; label: string }> = [
  { path: '/', label: 'Inspect' },
  { path: '/history', label: 'History' },
  { path: '/dashboard', label: 'Dashboard' },
  { path: '/fruits', label: 'Fruit guide' },
];

export function AppNav() {
  const location = useLocation();
  const navigate = useNavigate();

  return (
    <Tabs
      value={location.pathname}
      onChange={(path) => {
        if (path !== null) {
          void navigate(path);
        }
      }}
    >
      <Tabs.List>
        {TABS.map((tab) => (
          <Tabs.Tab key={tab.path} value={tab.path}>
            {tab.label}
          </Tabs.Tab>
        ))}
      </Tabs.List>
    </Tabs>
  );
}
