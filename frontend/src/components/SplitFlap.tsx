import { useEffect, useState } from 'react'

const CHARSET = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'
const TICK_MS = 45
const TOTAL_TICKS = 7

function randomChar(): string {
  return CHARSET[Math.floor(Math.random() * CHARSET.length)]
}

/**
 * Renders `value` the way a Solari split-flap departure board displays a new value: characters
 * flicker through random glyphs and settle left-to-right, instead of just appearing. This is
 * the console's signature motion - reused everywhere a status or code changes (booking status,
 * flight status, board rows mounting).
 *
 * The effect re-runs (and its own cleanup cancels the in-flight interval) every time `value`
 * changes, including on mount - no extra "already animated this value" guard, since one broke
 * under StrictMode's double-invoked effects: the guard's ref survives the mount/cleanup/remount
 * cycle even though the interval it was guarding gets cancelled, so the second run skipped the
 * animation entirely and the flap stayed blank.
 */
export function SplitFlap({ value, className }: { value: string; className?: string }) {
  const [display, setDisplay] = useState('')

  useEffect(() => {
    const target = value
    let tick = 0
    const id = window.setInterval(() => {
      tick += 1
      if (tick >= TOTAL_TICKS) {
        setDisplay(target)
        window.clearInterval(id)
        return
      }
      const revealCount = Math.floor((tick / TOTAL_TICKS) * target.length)
      let next = ''
      for (let i = 0; i < target.length; i++) {
        next += i < revealCount ? target[i] : randomChar()
      }
      setDisplay(next)
    }, TICK_MS)
    return () => window.clearInterval(id)
  }, [value])

  return (
    <span className={className} aria-label={value}>
      {display}
    </span>
  )
}
