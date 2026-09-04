import { Box } from '@mantine/core';
import { NavLink } from 'react-router-dom';
import { useAudience } from '../audience/useAudience';
import classes from './AppNav.module.css';

interface Tab {
  path: string;
  label: string;
  /** Business-only pages: a shopper has no store history to look at. */
  businessOnly?: boolean;
}

const TABS: readonly Tab[] = [
  { path: '/scan', label: 'Scan' },
  { path: '/guide', label: 'Fruit guide' },
  { path: '/history', label: 'History', businessOnly: true },
  { path: '/dashboard', label: 'Dashboard', businessOnly: true },
];

/**
 * The navigation follows the audience: per-store history and a weekly dashboard
 * are not questions a shopper has, and showing them would imply the app is
 * something they need to manage.
 */
export function AppNav() {
  const { audience } = useAudience();
  const visible = TABS.filter((tab) => tab.businessOnly !== true || audience === 'Business');

  return (
    <Box component="nav" className={classes.nav} aria-label="Sections">
      {visible.map((tab) => (
        <NavLink
          key={tab.path}
          to={tab.path}
          className={({ isActive }) => `${classes.tab} ${isActive ? classes.active : ''}`}
        >
          {tab.label}
        </NavLink>
      ))}
    </Box>
  );
}
