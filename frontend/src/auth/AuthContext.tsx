import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '@/api/endpoints'
import { configureApiClient } from '@/api/client'
import type { StaffRole } from '@/api/types'

interface Session {
  accessToken: string
  expiresAtUtc: string
  username: string
  role: StaffRole
}

interface AuthContextValue {
  session: Session | null
  login: (username: string, password: string) => Promise<void>
  logout: () => void
}

const STORAGE_KEY = 'airline.session'

const AuthContext = createContext<AuthContextValue | null>(null)

function readStoredSession(): Session | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) return null
  try {
    const parsed = JSON.parse(raw) as Session
    if (new Date(parsed.expiresAtUtc).getTime() <= Date.now()) return null
    return parsed
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => readStoredSession())

  const logout = useCallback(() => {
    sessionStorage.removeItem(STORAGE_KEY)
    setSession(null)
  }, [])

  const login = useCallback(async (username: string, password: string) => {
    const response = await authApi.login({ username, password })
    const next: Session = {
      accessToken: response.accessToken,
      expiresAtUtc: response.expiresAtUtc,
      username: response.username,
      role: response.role,
    }
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(next))
    setSession(next)
  }, [])

  useEffect(() => {
    configureApiClient({ token: session?.accessToken ?? null, onUnauthorized: logout })
  }, [session, logout])

  // A held booking expires after 15 minutes and this token after 60 - re-check the stored
  // expiry on an interval so a stale tab logs itself out instead of throwing 401s at every click.
  useEffect(() => {
    if (!session) return
    const id = window.setInterval(() => {
      if (new Date(session.expiresAtUtc).getTime() <= Date.now()) logout()
    }, 15_000)
    return () => window.clearInterval(id)
  }, [session, logout])

  const value = useMemo(() => ({ session, login, logout }), [session, login, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
