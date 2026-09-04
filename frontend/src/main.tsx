import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { MantineProvider } from '@mantine/core';
import { BrowserRouter } from 'react-router-dom';
import '@mantine/core/styles.css';
import '@mantine/dropzone/styles.css';
import './theme.css';
import { App } from './App';
import { AuthProvider } from './auth/AuthContext';
import { AudienceProvider } from './audience/AudienceContext';
import { ErrorBoundary } from './components/ErrorBoundary';
import { theme } from './theme';

const container = document.getElementById('root');

if (container === null) {
  throw new Error('Root element #root is missing from index.html.');
}

createRoot(container).render(
  <StrictMode>
    <MantineProvider theme={theme} defaultColorScheme="auto">
      {/* Outermost catch: the landing and login pages sit outside the app
          layout, and a provider itself can throw. Nothing below this may end
          as a blank page. */}
      <ErrorBoundary>
        <BrowserRouter>
          <AuthProvider>
            <AudienceProvider>
              <App />
            </AudienceProvider>
          </AuthProvider>
        </BrowserRouter>
      </ErrorBoundary>
    </MantineProvider>
  </StrictMode>,
);
