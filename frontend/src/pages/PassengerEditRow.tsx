import { useState } from 'react'
import { passengersApi } from '@/api/endpoints'
import { ApiError } from '@/api/client'
import type { PassengerDto } from '@/api/types'
import { TextInput } from '@/components/ui'
import { formatDate } from '@/lib/format'

export function PassengerEditRow({
  passenger,
  onSaved,
  onCancel,
}: {
  passenger: PassengerDto
  onSaved: (passenger: PassengerDto) => void
  onCancel: () => void
}) {
  const [firstName, setFirstName] = useState(passenger.firstName)
  const [lastName, setLastName] = useState(passenger.lastName)
  const [email, setEmail] = useState(passenger.email)
  const [phoneNumber, setPhoneNumber] = useState(passenger.phoneNumber ?? '')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  async function handleSave() {
    setSaving(true)
    setError(null)
    try {
      const updated = await passengersApi.update(passenger.id, {
        firstName,
        lastName,
        email,
        phoneNumber: phoneNumber || null,
      })
      onSaved(updated)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not save changes.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <tr className="border-b border-panel-line/60 bg-ink-raised last:border-0">
      <td className="px-4 py-2.5">
        <div className="flex gap-1.5">
          <TextInput value={firstName} onChange={(e) => setFirstName(e.target.value)} className="!py-1" />
          <TextInput value={lastName} onChange={(e) => setLastName(e.target.value)} className="!py-1" />
        </div>
      </td>
      <td className="px-4 py-2.5">
        <TextInput value={email} onChange={(e) => setEmail(e.target.value)} className="!py-1" />
      </td>
      <td className="px-4 py-2.5">
        <TextInput value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} className="!py-1" />
      </td>
      <td className="px-4 py-2.5 font-mono text-xs text-muted">{passenger.passportNumber}</td>
      <td className="px-4 py-2.5 text-xs text-muted">{formatDate(passenger.dateOfBirth)}</td>
      <td className="px-4 py-2.5 text-right whitespace-nowrap">
        {error && <span className="mr-2 text-xs text-rust">{error}</span>}
        <button className="mr-3 text-xs text-amber hover:underline" onClick={handleSave} disabled={saving}>
          {saving ? 'Saving…' : 'Save'}
        </button>
        <button className="text-xs text-muted hover:text-rust" onClick={onCancel}>
          Cancel
        </button>
      </td>
    </tr>
  )
}
