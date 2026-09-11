import type { SeatAvailabilityDto } from '@/api/types'

function parseSeat(seatNumber: string): { row: number; column: string } {
  const match = seatNumber.match(/^(\d+)([A-Za-z]+)$/)
  if (!match) return { row: 0, column: seatNumber }
  return { row: Number(match[1]), column: match[2] }
}

export function SeatMapGrid({
  seats,
  selectedSeatId,
  onSelect,
}: {
  seats: SeatAvailabilityDto[]
  selectedSeatId: number | null
  onSelect: (seat: SeatAvailabilityDto) => void
}) {
  const byRow = new Map<number, SeatAvailabilityDto[]>()
  for (const seat of seats) {
    const { row } = parseSeat(seat.seatNumber)
    if (!byRow.has(row)) byRow.set(row, [])
    byRow.get(row)!.push(seat)
  }
  const rows = [...byRow.entries()].sort((a, b) => a[0] - b[0])

  let lastCabin: SeatAvailabilityDto['cabinClass'] | null = null

  return (
    <div className="flex flex-col items-center gap-1.5 py-2">
      {rows.map(([row, rowSeats]) => {
        const sorted = [...rowSeats].sort((a, b) => a.seatNumber.localeCompare(b.seatNumber))
        const cabinChanged = sorted[0]?.cabinClass !== lastCabin
        lastCabin = sorted[0]?.cabinClass ?? lastCabin

        return (
          <div key={row} className="flex flex-col items-center gap-1.5">
            {cabinChanged && (
              <div className="mt-2 mb-0.5 font-mono text-[11px] tracking-[0.2em] text-slate uppercase first:mt-0">
                {sorted[0]?.cabinClass}
              </div>
            )}
            <div className="flex items-center gap-2">
              <span className="w-6 text-right font-mono text-xs text-slate">{row}</span>
              <div className="flex gap-1.5">
                {sorted.map((seat) => {
                  const isSelected = seat.seatId === selectedSeatId
                  const disabled = !seat.isAvailable
                  return (
                    <button
                      key={seat.seatId}
                      type="button"
                      disabled={disabled}
                      onClick={() => onSelect(seat)}
                      title={`${seat.seatNumber} · ${seat.cabinClass}${seat.isExitRow ? ' · exit row' : ''}${disabled ? ' · taken' : ''}`}
                      className={`flex h-8 w-8 items-center justify-center rounded-sm border font-mono text-[11px] transition-colors ${
                        isSelected
                          ? 'border-amber bg-amber text-ink'
                          : disabled
                            ? 'cursor-not-allowed border-panel-line bg-ink text-slate/50'
                            : seat.cabinClass === 'Business'
                              ? 'border-teal/40 bg-teal/10 text-teal hover:border-teal hover:bg-teal/20'
                              : 'border-panel-line bg-ink-raised text-flap hover:border-amber/60 hover:text-amber'
                      }`}
                    >
                      {parseSeat(seat.seatNumber).column}
                    </button>
                  )
                })}
              </div>
            </div>
          </div>
        )
      })}
    </div>
  )
}
