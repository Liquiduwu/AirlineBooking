import { useEffect, useState, type FormEvent } from 'react'
import { aircraftApi, authApi, fareClassesApi, flightsApi } from '@/api/endpoints'
import { ApiError } from '@/api/client'
import type { AircraftDto, CabinClass, FareClassDto, StaffRole } from '@/api/types'
import { Button, ErrorBanner, Field, Panel, Select, SectionLabel, TextInput } from '@/components/ui'

export function FleetPage() {
  return (
    <div className="flex flex-col gap-6">
      <div>
        <SectionLabel>Back office</SectionLabel>
        <h1 className="mt-1 font-display text-2xl font-semibold text-flap">Fleet &amp; fares</h1>
      </div>
      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <AircraftSection />
        <FareClassSection />
        <ScheduleFlightSection />
        <OnboardStaffSection />
      </div>
    </div>
  )
}

function AircraftSection() {
  const [aircraft, setAircraft] = useState<AircraftDto[]>([])
  const [registrationNumber, setRegistrationNumber] = useState('')
  const [model, setModel] = useState('')
  const [totalSeats, setTotalSeats] = useState(180)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function load() {
    try {
      setAircraft(await aircraftApi.getAll())
    } catch {
      /* surfaced via create-form errors; list can stay empty */
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      const created = await aircraftApi.create({ registrationNumber, model, totalSeats })
      setAircraft((prev) => [created, ...prev])
      setRegistrationNumber('')
      setModel('')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not register that aircraft.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Panel className="p-5">
      <SectionLabel>Fleet</SectionLabel>
      <form onSubmit={handleSubmit} className="mt-3 grid grid-cols-3 gap-2">
        <TextInput
          placeholder="Reg. number"
          value={registrationNumber}
          onChange={(e) => setRegistrationNumber(e.target.value)}
          className="font-mono uppercase"
          required
        />
        <TextInput placeholder="Model" value={model} onChange={(e) => setModel(e.target.value)} required />
        <TextInput
          type="number"
          min={1}
          placeholder="Seats"
          value={totalSeats}
          onChange={(e) => setTotalSeats(Number(e.target.value))}
          required
        />
        <Button type="submit" disabled={submitting} className="col-span-3">
          {submitting ? 'Registering…' : 'Register aircraft'}
        </Button>
      </form>
      <ErrorBanner message={error} />
      <ul className="mt-4 divide-y divide-panel-line">
        {aircraft.map((a) => (
          <li key={a.id} className="flex justify-between py-2 text-sm">
            <span className="font-mono text-flap">{a.registrationNumber}</span>
            <span className="text-muted">{a.model}</span>
            <span className="font-mono text-muted">{a.totalSeats} seats</span>
          </li>
        ))}
      </ul>
    </Panel>
  )
}

function FareClassSection() {
  const [fareClasses, setFareClasses] = useState<FareClassDto[]>([])
  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [cabinClass, setCabinClass] = useState<CabinClass>('Economy')
  const [priceMultiplier, setPriceMultiplier] = useState(1)
  const [isRefundable, setIsRefundable] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function load() {
    try {
      setFareClasses(await fareClassesApi.getAll())
    } catch {
      /* create-form surfaces its own errors */
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      const created = await fareClassesApi.create({ code, name, cabinClass, priceMultiplier, isRefundable })
      setFareClasses((prev) => [created, ...prev])
      setCode('')
      setName('')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not create that fare class.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Panel className="p-5">
      <SectionLabel>Fare classes</SectionLabel>
      <form onSubmit={handleSubmit} className="mt-3 flex flex-col gap-2">
        <div className="grid grid-cols-2 gap-2">
          <TextInput
            placeholder="Code (e.g. Y)"
            value={code}
            onChange={(e) => setCode(e.target.value.toUpperCase())}
            className="font-mono uppercase"
            required
          />
          <TextInput placeholder="Name" value={name} onChange={(e) => setName(e.target.value)} required />
        </div>
        <div className="grid grid-cols-2 gap-2">
          <Select value={cabinClass} onChange={(e) => setCabinClass(e.target.value as CabinClass)}>
            <option value="Economy">Economy</option>
            <option value="Business">Business</option>
          </Select>
          <TextInput
            type="number"
            step="0.01"
            min={0}
            placeholder="Multiplier"
            value={priceMultiplier}
            onChange={(e) => setPriceMultiplier(Number(e.target.value))}
            required
          />
        </div>
        <label className="flex items-center gap-2 text-sm text-muted">
          <input type="checkbox" checked={isRefundable} onChange={(e) => setIsRefundable(e.target.checked)} />
          Refundable
        </label>
        <Button type="submit" disabled={submitting}>
          {submitting ? 'Creating…' : 'Create fare class'}
        </Button>
      </form>
      <ErrorBanner message={error} />
      <ul className="mt-4 divide-y divide-panel-line">
        {fareClasses.map((fc) => (
          <li key={fc.id} className="flex justify-between py-2 text-sm">
            <span className="font-mono text-amber">{fc.code}</span>
            <span className="text-muted">
              {fc.name} · {fc.cabinClass}
            </span>
            <span className="font-mono text-muted">×{fc.priceMultiplier}</span>
          </li>
        ))}
      </ul>
    </Panel>
  )
}

function ScheduleFlightSection() {
  const [aircraft, setAircraft] = useState<AircraftDto[]>([])
  const [flightNumber, setFlightNumber] = useState('')
  const [originAirportCode, setOriginAirportCode] = useState('')
  const [destinationAirportCode, setDestinationAirportCode] = useState('')
  const [departureUtc, setDepartureUtc] = useState('')
  const [arrivalUtc, setArrivalUtc] = useState('')
  const [aircraftId, setAircraftId] = useState<number | null>(null)
  const [baseFare, setBaseFare] = useState(100)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    aircraftApi
      .getAll()
      .then((list) => {
        setAircraft(list)
        if (list.length > 0) setAircraftId(list[0].id)
      })
      .catch(() => {})
  }, [])

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (!aircraftId) return
    setSubmitting(true)
    setError(null)
    setSuccess(null)
    try {
      const flight = await flightsApi.create({
        flightNumber,
        originAirportCode,
        destinationAirportCode,
        departureUtc: new Date(departureUtc).toISOString(),
        arrivalUtc: new Date(arrivalUtc).toISOString(),
        aircraftId,
        baseFare,
      })
      setSuccess(`Scheduled ${flight.flightNumber}.`)
      setFlightNumber('')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not schedule that flight.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Panel className="p-5">
      <SectionLabel>Schedule a flight</SectionLabel>
      <form onSubmit={handleSubmit} className="mt-3 flex flex-col gap-2">
        <div className="grid grid-cols-3 gap-2">
          <TextInput
            placeholder="Flight no."
            value={flightNumber}
            onChange={(e) => setFlightNumber(e.target.value.toUpperCase())}
            className="font-mono uppercase"
            required
          />
          <TextInput
            placeholder="Origin"
            maxLength={3}
            value={originAirportCode}
            onChange={(e) => setOriginAirportCode(e.target.value.toUpperCase())}
            className="font-mono uppercase"
            required
          />
          <TextInput
            placeholder="Destination"
            maxLength={3}
            value={destinationAirportCode}
            onChange={(e) => setDestinationAirportCode(e.target.value.toUpperCase())}
            className="font-mono uppercase"
            required
          />
        </div>
        <div className="grid grid-cols-2 gap-2">
          <Field label="Departs" htmlFor="departureUtc">
            <TextInput
              id="departureUtc"
              type="datetime-local"
              value={departureUtc}
              onChange={(e) => setDepartureUtc(e.target.value)}
              required
            />
          </Field>
          <Field label="Arrives" htmlFor="arrivalUtc">
            <TextInput
              id="arrivalUtc"
              type="datetime-local"
              value={arrivalUtc}
              onChange={(e) => setArrivalUtc(e.target.value)}
              required
            />
          </Field>
        </div>
        <div className="grid grid-cols-2 gap-2">
          <Select value={aircraftId ?? ''} onChange={(e) => setAircraftId(Number(e.target.value))}>
            {aircraft.map((a) => (
              <option key={a.id} value={a.id}>
                {a.registrationNumber} · {a.model}
              </option>
            ))}
          </Select>
          <TextInput
            type="number"
            step="0.01"
            min={0}
            placeholder="Base fare"
            value={baseFare}
            onChange={(e) => setBaseFare(Number(e.target.value))}
            required
          />
        </div>
        <Button type="submit" disabled={submitting || !aircraftId}>
          {submitting ? 'Scheduling…' : 'Schedule flight'}
        </Button>
      </form>
      <ErrorBanner message={error} />
      {success && <p className="mt-2 text-sm text-teal">{success}</p>}
    </Panel>
  )
}

function OnboardStaffSection() {
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<StaffRole>('Agent')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    setSuccess(null)
    try {
      const user = await authApi.register({ username, email, password, role })
      setSuccess(`Onboarded ${user.username} as ${user.role}.`)
      setUsername('')
      setEmail('')
      setPassword('')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not onboard that account.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Panel className="p-5">
      <SectionLabel>Onboard staff</SectionLabel>
      <form onSubmit={handleSubmit} className="mt-3 flex flex-col gap-2">
        <TextInput placeholder="Username" value={username} onChange={(e) => setUsername(e.target.value)} required />
        <TextInput
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <TextInput
          type="password"
          placeholder="Temporary password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <Select value={role} onChange={(e) => setRole(e.target.value as StaffRole)}>
          <option value="Agent">Agent</option>
          <option value="Admin">Admin</option>
        </Select>
        <Button type="submit" disabled={submitting}>
          {submitting ? 'Onboarding…' : 'Create account'}
        </Button>
      </form>
      <ErrorBanner message={error} />
      {success && <p className="mt-2 text-sm text-teal">{success}</p>}
    </Panel>
  )
}
