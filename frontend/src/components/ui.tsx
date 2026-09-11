import type { ButtonHTMLAttributes, InputHTMLAttributes, LabelHTMLAttributes, ReactNode, SelectHTMLAttributes } from 'react'

export function Panel({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <div className={`board-surface rounded-md border border-panel-line bg-panel ${className}`}>{children}</div>
  )
}

export function SectionLabel({ children }: { children: ReactNode }) {
  return (
    <h2 className="font-display text-sm font-semibold tracking-[0.08em] text-muted uppercase">{children}</h2>
  )
}

export function Field({ label, htmlFor, children }: { label: string; htmlFor: string; children: ReactNode }) {
  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={htmlFor} className="text-xs font-medium tracking-wide text-muted uppercase">
        {label}
      </label>
      {children}
    </div>
  )
}

const inputBase =
  'w-full rounded-sm border border-panel-line bg-ink px-3 py-2 text-sm text-flap placeholder:text-slate focus:border-amber focus:outline-none focus:ring-1 focus:ring-amber/40'

export function TextInput(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={`${inputBase} ${props.className ?? ''}`} />
}

export function Select(props: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select {...props} className={`${inputBase} ${props.className ?? ''}`} />
}

export function Button({
  variant = 'primary',
  className = '',
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: 'primary' | 'secondary' | 'danger' }) {
  const tones = {
    primary: 'bg-amber text-ink hover:bg-amber/90 disabled:bg-amber/40',
    secondary: 'border border-panel-line text-flap hover:border-amber/60 hover:text-amber disabled:opacity-40',
    danger: 'border border-rust/50 text-rust hover:bg-rust/10 disabled:opacity-40',
  }
  return (
    <button
      {...props}
      className={`inline-flex items-center justify-center gap-1.5 rounded-sm px-3.5 py-2 text-sm font-semibold tracking-wide transition-colors disabled:cursor-not-allowed ${tones[variant]} ${className}`}
    />
  )
}

export function ErrorBanner({ message }: { message: string | null }) {
  if (!message) return null
  return (
    <div className="rounded-sm border border-rust/50 bg-rust/10 px-3 py-2 text-sm text-rust" role="alert">
      {message}
    </div>
  )
}

export function EmptyState({ children }: { children: ReactNode }) {
  return <p className="px-4 py-10 text-center text-sm text-muted">{children}</p>
}

export function LabelText(props: LabelHTMLAttributes<HTMLLabelElement>) {
  return <label {...props} className={`text-xs text-muted ${props.className ?? ''}`} />
}
