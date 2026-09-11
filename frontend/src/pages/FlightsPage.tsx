import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { flightsApi } from '@/api/endpoints'
import { ApiError } from '@/api/client'
import type { FlightSummaryDto } from '@/api/types'
import { Button, EmptyState, ErrorBanner, Field, Panel, SectionLabel, TextInput } from '@/components/ui'
import { FlightStatusBadge } from '@/components/StatusBadge'
import { formatDateTime, formatMoney } from '@/lib/format'

export function FlightsPage() {
  const [origin, setOrigin] = useState('')
  const [destination, setDestination] = useState('')
  const [date, setDate] = useState('')
  const [flights, setFlights] = useState<FlightSummaryDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  async function runSearch(params: { originAirportCode?: string; destinationAirportCode?: string; departureDate?: string }) {
    setLoading(true)
    setError(null)
    try {
      setFlights(await flightsApi.search(params))
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not load flights.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    runSearch({})
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    runSearch({
      originAirportCode: origin || undefined,
      destinationAirportCode: destination || undefined,
      departureDate: date || undefined,
    })
  }

  return (
    <div className="flex flex-col gap-6">
      <div>
        <SectionLabel>Flight search</SectionLabel>
        <h1 className="mt-1 font-display text-2xl font-semibold text-flap">Find a flight</h1>
      </div>

      <Panel className="p-5">
        <form onSubmit={handleSubmit} className="flex flex-wrap items-end gap-4">
          <div className="w-28">
            <Field label="Origin" htmlFor="origin">
              <TextInput
                id="origin"
                placeholder="KWI"
                maxLength={3}
                value={origin}
                onChange={(e) => setOrigin(e.target.value.toUpperCase())}
                className="font-mono uppercase"
              />
            </Field>
          </div>
          <div className="w-28">
            <Field label="Destination" htmlFor="destination">
              <TextInput
                id="destination"
                placeholder="DXB"
                maxLength={3}
                value={destination}
                onChange={(e) => setDestination(e.target.value.toUpperCase())}
                className="font-mono uppercase"
              />
            </Field>
          </div>
          <div className="w-44">
            <Field label="Departure date" htmlFor="date">
              <TextInput id="date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
            </Field>
          </div>
          <Button type="submit" disabled={loading}>
            {loading ? 'Searching…' : 'Search'}
          </Button>
        </form>
      </Panel>

      <ErrorBanner message={error} />

      <Panel>
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-panel-line text-left text-xs tracking-[0.08em] text-muted uppercase">
                <th className="px-4 py-3 font-medium">Flight</th>
                <th className="px-4 py-3 font-medium">Route</th>
                <th className="px-4 py-3 font-medium">Departs</th>
                <th className="px-4 py-3 font-medium">Arrives</th>
                <th className="px-4 py-3 font-medium">Aircraft</th>
                <th className="px-4 py-3 font-medium">Base fare</th>
                <th className="px-4 py-3 font-medium">Status</th>
              </tr>
            </thead>
            <tbody>
              {flights?.map((flight) => (
                <tr key={flight.id} className="border-b border-panel-line/60 last:border-0 hover:bg-ink-raised">
                  <td className="px-4 py-3">
                    <Link
                      to={`/flights/${flight.id}`}
                      className="font-mono text-sm font-semibold text-amber hover:underline"
                    >
                      {flight.flightNumber}
                    </Link>
                  </td>
                  <td className="px-4 py-3 font-mono text-flap">
                    {flight.originAirportCode} <span className="text-slate">→</span> {flight.destinationAirportCode}
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-muted">{formatDateTime(flight.departureUtc)}</td>
                  <td className="px-4 py-3 font-mono text-xs text-muted">{formatDateTime(flight.arrivalUtc)}</td>
                  <td className="px-4 py-3 text-flap">{flight.aircraftModel}</td>
                  <td className="px-4 py-3 font-mono text-flap">{formatMoney(flight.baseFare)}</td>
                  <td className="px-4 py-3">
                    <FlightStatusBadge status={flight.status} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {flights?.length === 0 && <EmptyState>No flights match that search.</EmptyState>}
        </div>
      </Panel>
    </div>
  )
}
