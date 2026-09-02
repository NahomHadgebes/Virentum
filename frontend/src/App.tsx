import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from './auth/RequireAuth';
import { AppLayout } from './components/AppLayout';
import { LoginPage } from './features/login/LoginPage';
import { InspectionPage } from './features/inspection/InspectionPage';
import { HistoryPage } from './features/history/HistoryPage';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { FruitGuidePage } from './features/fruits/FruitGuidePage';

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <RequireAuth>
            <AppLayout>
              <InspectionPage />
            </AppLayout>
          </RequireAuth>
        }
      />
      <Route
        path="/history"
        element={
          <RequireAuth>
            <AppLayout>
              <HistoryPage />
            </AppLayout>
          </RequireAuth>
        }
      />
      <Route
        path="/dashboard"
        element={
          <RequireAuth>
            <AppLayout>
              <DashboardPage />
            </AppLayout>
          </RequireAuth>
        }
      />
      <Route
        path="/fruits"
        element={
          <RequireAuth>
            <AppLayout>
              <FruitGuidePage />
            </AppLayout>
          </RequireAuth>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
