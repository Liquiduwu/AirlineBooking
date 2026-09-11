import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { bookingsApi, fareClassesApi, flightsApi, passengersApi } from '@/api/endpoints'
import { ApiError } from '@/api/client'
import type { FareClassDto, FareQuoteDto, FlightSummaryDto, PassengerDto, SeatAvailabilityDto } from '@/api/types'
import { Button, ErrorBanner, Panel, SectionLabel, Select, TextInput } from '@/components/ui'
import { FlightStatusBadge } from '@/components/StatusBadge'
import { SeatMapGrid } from '@/components/SeatMapGrid'
import { formatDateTime, formatMoney } from '@/lib/format'
import { PassengerCreateForm } from './PassengerCreateForm'

export function FlightDetailPage() {
  const { id } = useParams<{ id: string }>()
  const flightId = Number(id)
  const navigate = useNavigate()

  const [flight, setFlight] = useState<FlightSummaryDto | null>(null)
  const [seats, setSeats] = useState<SeatAvailabilityDto[]>([])
  const [fareClasses, setFareClasses] = useState<FareClassDto[]>([])
  const [passengers, setPassengers] = useState<PassengerDto[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)

  const [selectedSeat, setSelectedSeat] = useState<SeatAvailabilityDto | null>(null)
  const [fareClassId, setFareClassId] = useState<number | null>(null)
  const [quote, setQuote] = useState<FareQuoteDto | null>(null)

  const [passengerQuery, setPassengerQuery] = useState('')
  const [passengerId, setPassengerId] = useState<number | null>(null)
  const [showNewPassenger, setShowNewPassenger] = useState(false)

  const [holdError, setHoldError] = useState<string | null>(null)
  const [holding, setHolding] = useState(false)

  async function loadAll() {
    setLoadError(null)
    try {
      const [flightRes, seatsRes, fareClassRes, passengersRes] = await Promise.all([
        flightsApi.getById(flightId),
        flightsApi.getSeatMap(flightId),
        fareClassesApi.getAll(),
        passengersApi.getAll(),
      ])
      setFlight(flightRes)
      setSeats(seatsRes.seats)
      setFareClasses(fareClassRes)
      setPassengers(passengersRes)
      if (fareClassRes.length > 0) setFareClassId(fareClassRes[0].id)
    } catch (err) {
      setLoadError(err instanceof ApiError ? err.message : 'Could not load this flight.')
    }
  }

  useEffect(() => {
    loadAll()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [flightId])

  useEffect(() => {
    if (!fareClassId) {
      setQuote(null)
      return
    }
    flightsApi
      .getFareQuote(flightId, fareClassId)
      .then(setQuote)
      .catch(() => setQuote(null))
  }, [flightId, fareClassId])

  const filteredPassengers = useMemo(() => {
    const q = passengerQuery.trim().toLowerCase()
    if (!q) return passengers.slice(0, 8)
    return passengers
      .filter(
        (p) =>
          `${p.firstName} ${p.lastName}`.toLowerCase().includes(q) ||
          p.email.toLowerCase().includes(q) ||
          p.passportNumber.toLowerCase().includes(q),
      )
      .slice(0, 8)
  }, [passengers, passengerQuery])

  const selectedPassenger = passengers.find((p) => p.id === passengerId) ?? null

  async function handleHold() {
    if (!selectedSeat || !fareClassId || !passengerId) return
    setHolding(true)
    setHoldError(null)
    try {
      const booking = await bookingsApi.createHold({
        passengerId,
        flightId,
        seatId: selectedSeat.seatId,
        fareClassId,
      })
      navigate(`/bookings?ref=${booking.bookingReference}`)
    } catch (err) {
      setHoldError(err instanceof ApiError ? err.message : 'Could not hold that seat.')
    } finally {
      setHolding(false)
    }
  }

  if (loadError) {
    return (
      <div className="flex flex-col gap-4">
        <ErrorBanner message={loadError} />
        <Link to="/flights" className="text-sm text-amber hover:underline">
          ← Back to search
        </Link>
      </div>
    )
  }

  if (!flight) return <p className="text-sm text-muted">Loading flight…</p>

  const canHold = selectedSeat && fareClassId && passengerId

  return (
    <div className="flex flex-col gap-6">
      <div>
        <Link to="/flights" className="text-xs text-muted hover:text-amber">
          ← Back to search
        </Link>
        <div className="mt-2 flex items-center gap-3">
          <h1 className="font-mono text-2xl font-semibold text-amber">{flight.flightNumber}</h1>
          <FlightStatusBadge status={flight.status} />
        </div>
        <p className="mt-1 font-mono text-sm text-muted">
          {flight.originAirportCode} → {flight.destinationAirportCode} · {formatDateTime(flight.departureUtc)} →{' '}
          {formatDateTime(flight.arrivalUtc)} · {flight.aircraftModel}
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_360px]">
        <Panel className="p-5">
          <SectionLabel>Seat map</SectionLabel>
          <SeatMapGrid seats={seats} selectedSeatId={selectedSeat?.seatId ?? null} onSelect={setSelectedSeat} />
        </Panel>

        <div className="flex flex-col gap-4">
          <Panel className="p-5">
            <SectionLabel>Fare class</SectionLabel>
            <div className="mt-3">
              <Select value={fareClassId ?? ''} onChange={(e) => setFareClassId(Number(e.target.value))}>
                {fareClasses.map((fc) => (
                  <option key={fc.id} value={fc.id}>
                    {fc.code} · {fc.name} ({fc.cabinClass})
                  </option>
                ))}
              </Select>
            </div>

            {quote && (
              <dl className="mt-4 space-y-1.5 font-mono text-sm">
                <div className="flex justify-between text-muted">
                  <dt>Base fare</dt>
                  <dd>{formatMoney(quote.baseFare)}</dd>
                </div>
                <div className="flex justify-between text-muted">
                  <dt>Multiplier</dt>
                  <dd>×{quote.priceMultiplier}</dd>
                </div>
                <div className="flex justify-between text-muted">
                  <dt>Load surcharge</dt>
                  <dd>{formatMoney(quote.loadFactorSurcharge)}</dd>
                </div>
                <div className="mt-2 flex justify-between border-t border-panel-line pt-2 text-base font-semibold text-amber">
                  <dt>Total</dt>
                  <dd>{formatMoney(quote.totalFare)}</dd>
                </div>
              </dl>
            )}
          </Panel>

          <Panel className="p-5">
            <SectionLabel>Passenger</SectionLabel>
            {selectedPassenger ? (
              <div className="mt-3 flex items-center justify-between rounded-sm border border-panel-line bg-ink px-3 py-2">
                <div>
                  <div className="text-sm text-flap">
                    {selectedPassenger.firstName} {selectedPassenger.lastName}
                  </div>
                  <div className="text-xs text-muted">{selectedPassenger.email}</div>
                </div>
                <button
                  className="text-xs text-muted hover:text-rust"
                  onClick={() => setPassengerId(null)}
                  type="button"
                >
                  change
                </button>
              </div>
            ) : showNewPassenger ? (
              <PassengerCreateForm
                onCreated={(p) => {
                  setPassengers((prev) => [p, ...prev])
                  setPassengerId(p.id)
                  setShowNewPassenger(false)
                }}
                onCancel={() => setShowNewPassenger(false)}
              />
            ) : (
              <div className="mt-3 flex flex-col gap-2">
                <TextInput
                  placeholder="Search name, email, or passport…"
                  value={passengerQuery}
                  onChange={(e) => setPassengerQuery(e.target.value)}
                />
                <div className="flex flex-col gap-1">
                  {filteredPassengers.map((p) => (
                    <button
                      key={p.id}
                      type="button"
                      onClick={() => setPassengerId(p.id)}
                      className="rounded-sm border border-transparent px-2 py-1.5 text-left text-sm text-flap hover:border-panel-line hover:bg-ink"
                    >
                      {p.firstName} {p.lastName}{' '}
                      <span className="font-mono text-xs text-muted">{p.passportNumber}</span>
                    </button>
                  ))}
                </div>
                <button
                  type="button"
                  onClick={() => setShowNewPassenger(true)}
                  className="mt-1 text-left text-xs text-amber hover:underline"
                >
                  + New passenger
                </button>
              </div>
            )}
          </Panel>

          <ErrorBanner message={holdError} />

          <Button disabled={!canHold || holding} onClick={handleHold} className="w-full">
            {holding
              ? 'Holding seat…'
              : selectedSeat
                ? `Hold seat ${selectedSeat.seatNumber}`
                : 'Select a seat to continue'}
          </Button>
          <p className="text-center text-xs text-slate">A hold expires after 15 minutes unless confirmed.</p>
        </div>
      </div>
    </div>
  )
}
