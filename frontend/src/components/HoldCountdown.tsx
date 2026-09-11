import { useEffect, useState } from 'react'

function formatRemaining(ms: number): string {
  if (ms <= 0) return '00:00'
  const totalSeconds = Math.floor(ms / 1000)
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
}

/** Ticking mm:ss until `heldUntilUtc`, for a Held booking's 15-minute hold window. */
export function HoldCountdown({ heldUntilUtc }: { heldUntilUtc: string }) {
  const target = new Date(heldUntilUtc).getTime()
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    const id = window.setInterval(() => setNow(Date.now()), 1000)
    return () => window.clearInterval(id)
  }, [])

  const remaining = target - now
  const urgent = remaining <= 60_000 && remaining > 0
  const expired = remaining <= 0

  return (
    <span
      className={`font-mono text-xs tabular-nums ${
        expired ? 'text-rust' : urgent ? 'animate-pulse text-rust' : 'text-amber'
      }`}
    >
      {expired ? 'expiring…' : formatRemaining(remaining)}
    </span>
  )
}
