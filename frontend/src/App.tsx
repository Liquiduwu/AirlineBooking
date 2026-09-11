import { Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from '@/auth/AuthContext'
import { AppShell } from '@/components/AppShell'
import { RequireAdmin, RequireAuth } from '@/components/RouteGuards'
import { LoginPage } from '@/pages/LoginPage'
import { FlightsPage } from '@/pages/FlightsPage'
import { FlightDetailPage } from '@/pages/FlightDetailPage'
import { BookingsPage } from '@/pages/BookingsPage'
import { PassengersPage } from '@/pages/PassengersPage'
import { FleetPage } from '@/pages/FleetPage'

export function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<RequireAuth />}>
          <Route element={<AppShell />}>
            <Route path="/flights" element={<FlightsPage />} />
            <Route path="/flights/:id" element={<FlightDetailPage />} />
            <Route path="/bookings" element={<BookingsPage />} />
            <Route path="/passengers" element={<PassengersPage />} />
            <Route element={<RequireAdmin />}>
              <Route path="/fleet" element={<FleetPage />} />
            </Route>
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/flights" replace />} />
      </Routes>
    </AuthProvider>
  )
}
