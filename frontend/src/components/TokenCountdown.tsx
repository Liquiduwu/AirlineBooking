import { useEffect, useState } from 'react'

/** Session-length indicator in the top bar - this JWT has no refresh flow, so agents should
 * see the 60-minute clock running down instead of getting surprise-logged-out mid-task. */
export function TokenCountdown({ expiresAtUtc }: { expiresAtUtc: string }) {
  const target = new Date(expiresAtUtc).getTime()
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    const id = window.setInterval(() => setNow(Date.now()), 1000)
    return () => window.clearInterval(id)
  }, [])

  const remainingMs = Math.max(0, target - now)
  const minutes = Math.floor(remainingMs / 60_000)
  const seconds = Math.floor((remainingMs % 60_000) / 1000)
  const low = remainingMs < 5 * 60_000

  return (
    <div className={`flex items-center gap-1.5 font-mono text-xs ${low ? 'text-amber' : 'text-slate'}`}>
      <span aria-hidden>◷</span>
      <span>
        session {String(minutes).padStart(2, '0')}:{String(seconds).padStart(2, '0')}
      </span>
    </div>
  )
}
