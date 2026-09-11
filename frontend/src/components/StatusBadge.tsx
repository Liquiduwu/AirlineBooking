import type { BookingStatus, FlightStatus } from '@/api/types'
import { SplitFlap } from './SplitFlap'

const BOOKING_TONE: Record<BookingStatus, string> = {
  Held: 'text-amber border-amber-dim/60 bg-amber/10',
  Confirmed: 'text-teal border-teal/50 bg-teal/10',
  Cancelled: 'text-rust border-rust/50 bg-rust/10',
  Expired: 'text-muted border-slate/60 bg-slate/10',
}

const FLIGHT_TONE: Record<FlightStatus, string> = {
  Scheduled: 'text-flap border-panel-line bg-panel',
  Delayed: 'text-amber border-amber-dim/60 bg-amber/10',
  Departed: 'text-teal border-teal/50 bg-teal/10',
  Arrived: 'text-teal border-teal/50 bg-teal/10',
  Cancelled: 'text-rust border-rust/50 bg-rust/10',
}

function Chip({ tone, value }: { tone: string; value: string }) {
  return (
    <span
      className={`inline-flex items-center rounded-sm border px-2 py-0.5 font-mono text-xs font-medium tracking-[0.14em] uppercase ${tone}`}
    >
      <SplitFlap value={value} />
    </span>
  )
}

export function BookingStatusBadge({ status }: { status: BookingStatus }) {
  return <Chip tone={BOOKING_TONE[status]} value={status} />
}

export function FlightStatusBadge({ status }: { status: FlightStatus }) {
  return <Chip tone={FLIGHT_TONE[status]} value={status} />
}
