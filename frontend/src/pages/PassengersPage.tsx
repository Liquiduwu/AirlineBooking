import { useEffect, useMemo, useState } from 'react'
import { passengersApi } from '@/api/endpoints'
import { ApiError } from '@/api/client'
import type { PassengerDto } from '@/api/types'
import { Button, EmptyState, ErrorBanner, Panel, SectionLabel, TextInput } from '@/components/ui'
import { PassengerCreateForm } from './PassengerCreateForm'
import { PassengerEditRow } from './PassengerEditRow'
import { formatDate } from '@/lib/format'

export function PassengersPage() {
  const [passengers, setPassengers] = useState<PassengerDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [query, setQuery] = useState('')
  const [creating, setCreating] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)

  async function load() {
    setError(null)
    try {
      setPassengers(await passengersApi.getAll())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not load passengers.')
    }
  }

  useEffect(() => {
    load()
  }, [])

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase()
    if (!q || !passengers) return passengers
    return passengers.filter(
      (p) =>
        `${p.firstName} ${p.lastName}`.toLowerCase().includes(q) ||
        p.email.toLowerCase().includes(q) ||
        p.passportNumber.toLowerCase().includes(q),
    )
  }, [passengers, query])

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-start justify-between">
        <div>
          <SectionLabel>Passenger records</SectionLabel>
          <h1 className="mt-1 font-display text-2xl font-semibold text-flap">Passengers</h1>
        </div>
        <Button onClick={() => setCreating((v) => !v)} variant={creating ? 'secondary' : 'primary'}>
          {creating ? 'Close' : '+ New passenger'}
        </Button>
      </div>

      {creating && (
        <Panel className="max-w-md p-5">
          <SectionLabel>New passenger</SectionLabel>
          <PassengerCreateForm
            onCreated={(p) => {
              setPassengers((prev) => [p, ...(prev ?? [])])
              setCreating(false)
            }}
            onCancel={() => setCreating(false)}
          />
        </Panel>
      )}

      <ErrorBanner message={error} />

      <TextInput
        placeholder="Search name, email, or passport…"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className="max-w-sm"
      />

      <Panel>
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-panel-line text-left text-xs tracking-[0.08em] text-muted uppercase">
                <th className="px-4 py-3 font-medium">Name</th>
                <th className="px-4 py-3 font-medium">Email</th>
                <th className="px-4 py-3 font-medium">Phone</th>
                <th className="px-4 py-3 font-medium">Passport</th>
                <th className="px-4 py-3 font-medium">Born</th>
                <th className="px-4 py-3 font-medium" />
              </tr>
            </thead>
            <tbody>
              {filtered?.map((p) =>
                editingId === p.id ? (
                  <PassengerEditRow
                    key={p.id}
                    passenger={p}
                    onSaved={(updated) => {
                      setPassengers((prev) => prev?.map((x) => (x.id === updated.id ? updated : x)) ?? null)
                      setEditingId(null)
                    }}
                    onCancel={() => setEditingId(null)}
                  />
                ) : (
                  <tr key={p.id} className="border-b border-panel-line/60 last:border-0 hover:bg-ink-raised">
                    <td className="px-4 py-3 text-flap">
                      {p.firstName} {p.lastName}
                    </td>
                    <td className="px-4 py-3 text-muted">{p.email}</td>
                    <td className="px-4 py-3 font-mono text-muted">{p.phoneNumber ?? '—'}</td>
                    <td className="px-4 py-3 font-mono text-muted">{p.passportNumber}</td>
                    <td className="px-4 py-3 text-muted">{formatDate(p.dateOfBirth)}</td>
                    <td className="px-4 py-3 text-right">
                      <button className="text-xs text-amber hover:underline" onClick={() => setEditingId(p.id)}>
                        Edit
                      </button>
                    </td>
                  </tr>
                ),
              )}
            </tbody>
          </table>
          {filtered?.length === 0 && <EmptyState>No passengers match that search.</EmptyState>}
        </div>
      </Panel>
    </div>
  )
}
