import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export function RequireAuth() {
  const { session } = useAuth()
  if (!session) return <Navigate to="/login" replace />
  return <Outlet />
}

export function RequireAdmin() {
  const { session } = useAuth()
  if (session?.role !== 'Admin') return <Navigate to="/flights" replace />
  return <Outlet />
}
