import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'
import { TokenCountdown } from './TokenCountdown'

const NAV_ITEMS = [
  { to: '/flights', label: 'Search', glyph: '⌕' },
  { to: '/bookings', label: 'Bookings', glyph: '⌸' },
  { to: '/passengers', label: 'Passengers', glyph: '☺' },
]

const ADMIN_NAV_ITEMS = [{ to: '/fleet', label: 'Fleet & fares', glyph: '✈' }]

export function AppShell() {
  const { session, logout } = useAuth()
  if (!session) return null

  const items = session.role === 'Admin' ? [...NAV_ITEMS, ...ADMIN_NAV_ITEMS] : NAV_ITEMS

  return (
    <div className="flex min-h-screen bg-ink">
      <aside className="flex w-56 shrink-0 flex-col border-r border-panel-line bg-ink-raised">
        <div className="flex items-center gap-2 border-b border-panel-line px-5 py-5">
          <span className="text-lg text-amber">✈</span>
          <span className="font-display text-sm font-semibold tracking-[0.06em] text-flap">AIRLINE BOOKING</span>
        </div>
        <nav className="flex flex-1 flex-col gap-0.5 px-3 py-4">
          {items.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-sm px-3 py-2 text-sm font-medium transition-colors ${
                  isActive ? 'bg-amber/10 text-amber' : 'text-muted hover:bg-panel hover:text-flap'
                }`
              }
            >
              <span aria-hidden className="w-4 text-center">
                {item.glyph}
              </span>
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="border-t border-panel-line px-5 py-4 text-xs text-slate">
          Agent console · v1
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center justify-between border-b border-panel-line bg-ink-raised px-6 py-3">
          <div className="flex items-center gap-2">
            <TokenCountdown expiresAtUtc={session.expiresAtUtc} />
          </div>
          <div className="flex items-center gap-4">
            <div className="text-right">
              <div className="font-mono text-sm text-flap">{session.username}</div>
              <div className="text-xs tracking-wide text-muted uppercase">{session.role}</div>
            </div>
            <button
              onClick={logout}
              className="rounded-sm border border-panel-line px-3 py-1.5 text-xs font-medium text-muted transition-colors hover:border-rust/50 hover:text-rust"
            >
              Sign out
            </button>
          </div>
        </header>
        <main className="min-w-0 flex-1 px-6 py-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
