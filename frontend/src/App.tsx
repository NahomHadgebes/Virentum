import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from './auth/RequireAuth';
import { RequireAudience } from './audience/RequireAudience';
import { AppLayout } from './components/AppLayout';
import { LandingPage } from './features/landing/LandingPage';
import { LoginPage } from './features/login/LoginPage';
import { InspectionPage } from './features/inspection/InspectionPage';
import { HistoryPage } from './features/history/HistoryPage';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { FruitGuidePage } from './features/fruits/FruitGuidePage';
import type { ReactNode } from 'react';

/**
 * Two gates, in order: you must have said who you are before the app can word
 * anything for you, and you must be signed in before it will read an image.
 */
function Guarded({ children }: { children: ReactNode }) {
  return (
    <RequireAudience>
      <RequireAuth>
        <AppLayout>{children}</AppLayout>
      </RequireAuth>
    </RequireAudience>
  );
}

export function App() {
  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/scan" element={<Guarded><InspectionPage /></Guarded>} />
      <Route path="/guide" element={<Guarded><FruitGuidePage /></Guarded>} />
      <Route path="/history" element={<Guarded><HistoryPage /></Guarded>} />
      <Route path="/dashboard" element={<Guarded><DashboardPage /></Guarded>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
