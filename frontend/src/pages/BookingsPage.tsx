import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { bookingsApi } from '@/api/endpoints'
import { ApiError } from '@/api/client'
import type { BookingDto } from '@/api/types'
import { Button, EmptyState, ErrorBanner, Panel, SectionLabel } from '@/components/ui'
import { BookingStatusBadge } from '@/components/StatusBadge'
import { HoldCountdown } from '@/components/HoldCountdown'
import { formatMoney } from '@/lib/format'

export function BookingsPage() {
  const [bookings, setBookings] = useState<BookingDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [actioningRef, setActioningRef] = useState<string | null>(null)
  const [searchParams] = useSearchParams()
  const highlightedRef = searchParams.get('ref')

  async function load() {
    setError(null)
    try {
      setBookings(await bookingsApi.getAll())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not load bookings.')
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function handleAction(reference: string, action: 'confirm' | 'cancel') {
    setActioningRef(reference)
    setError(null)
    try {
      const updated = action === 'confirm' ? await bookingsApi.confirm(reference) : await bookingsApi.cancel(reference)
      setBookings((prev) => prev?.map((b) => (b.bookingReference === reference ? updated : b)) ?? null)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'That action failed.')
    } finally {
      setActioningRef(null)
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <div>
        <SectionLabel>Booking board</SectionLabel>
        <h1 className="mt-1 font-display text-2xl font-semibold text-flap">Bookings</h1>
      </div>

      <ErrorBanner message={error} />

      <Panel>
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-panel-line text-left text-xs tracking-[0.08em] text-muted uppercase">
                <th className="px-4 py-3 font-medium">Reference</th>
                <th className="px-4 py-3 font-medium">Passenger</th>
                <th className="px-4 py-3 font-medium">Flight</th>
                <th className="px-4 py-3 font-medium">Seat</th>
                <th className="px-4 py-3 font-medium">Fare</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">Hold expires</th>
                <th className="px-4 py-3 font-medium" />
              </tr>
            </thead>
            <tbody>
              {bookings?.map((booking) => (
                <tr
                  key={booking.id}
                  className={`border-b border-panel-line/60 last:border-0 ${
                    booking.bookingReference === highlightedRef ? 'bg-amber/5' : 'hover:bg-ink-raised'
                  }`}
                >
                  <td className="px-4 py-3 font-mono font-semibold text-amber">{booking.bookingReference}</td>
                  <td className="px-4 py-3 text-flap">{booking.passengerName}</td>
                  <td className="px-4 py-3 font-mono text-muted">{booking.flightNumber}</td>
                  <td className="px-4 py-3 font-mono text-muted">
                    {booking.seatNumber} <span className="text-slate">· {booking.fareClassCode}</span>
                  </td>
                  <td className="px-4 py-3 font-mono text-flap">{formatMoney(booking.fareAmount)}</td>
                  <td className="px-4 py-3">
                    <BookingStatusBadge status={booking.status} />
                  </td>
                  <td className="px-4 py-3">
                    {booking.status === 'Held' && booking.heldUntilUtc ? (
                      <HoldCountdown heldUntilUtc={booking.heldUntilUtc} />
                    ) : (
                      <span className="text-xs text-slate">—</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    {booking.status === 'Held' && (
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="secondary"
                          disabled={actioningRef === booking.bookingReference}
                          onClick={() => handleAction(booking.bookingReference, 'confirm')}
                          className="!px-2.5 !py-1 text-xs"
                        >
                          Confirm
                        </Button>
                        <Button
                          variant="danger"
                          disabled={actioningRef === booking.bookingReference}
                          onClick={() => handleAction(booking.bookingReference, 'cancel')}
                          className="!px-2.5 !py-1 text-xs"
                        >
                          Cancel
                        </Button>
                      </div>
                    )}
                    {booking.status === 'Confirmed' && (
                      <div className="flex justify-end">
                        <Button
                          variant="danger"
                          disabled={actioningRef === booking.bookingReference}
                          onClick={() => handleAction(booking.bookingReference, 'cancel')}
                          className="!px-2.5 !py-1 text-xs"
                        >
                          Cancel
                        </Button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {bookings?.length === 0 && <EmptyState>No bookings yet. Hold a seat from Search to create one.</EmptyState>}
        </div>
      </Panel>
      {bookings && (
        <p className="text-xs text-slate">
          Bookings shown at last load. Held rows tick down live and expire automatically after 15 minutes.
        </p>
      )}
    </div>
  )
}
