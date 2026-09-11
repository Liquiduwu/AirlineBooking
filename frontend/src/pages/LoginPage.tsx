import { useState, type FormEvent } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'
import { ApiError } from '@/api/client'
import { Button, ErrorBanner, Field, Panel, TextInput } from '@/components/ui'
import { SplitFlap } from '@/components/SplitFlap'

export function LoginPage() {
  const { session, login } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (session) return <Navigate to="/flights" replace />

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      await login(username, password)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not reach the booking API.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-ink px-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <div className="font-display text-2xl font-semibold tracking-[0.08em] text-amber">
            <SplitFlap value="AIRLINE" />
          </div>
          <div className="font-display text-sm tracking-[0.3em] text-muted uppercase">
            <SplitFlap value="BOOKING CONSOLE" />
          </div>
        </div>

        <Panel className="p-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <Field label="Username" htmlFor="username">
              <TextInput
                id="username"
                name="username"
                autoComplete="username"
                autoFocus
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
            </Field>
            <Field label="Password" htmlFor="password">
              <TextInput
                id="password"
                name="password"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </Field>
            <ErrorBanner message={error} />
            <Button type="submit" disabled={submitting} className="w-full">
              {submitting ? 'Signing in…' : 'Sign in'}
            </Button>
          </form>
        </Panel>

        <p className="mt-4 text-center font-mono text-xs text-slate">
          demo: <span className="text-muted">admin</span> / <span className="text-muted">Admin#Passw0rd1</span>
        </p>
      </div>
    </div>
  )
}
