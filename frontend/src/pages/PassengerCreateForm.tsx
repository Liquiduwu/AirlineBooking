import { useState, type FormEvent } from 'react'
import { passengersApi } from '@/api/endpoints'
import { ApiError } from '@/api/client'
import type { PassengerDto } from '@/api/types'
import { Button, ErrorBanner, Field, TextInput } from '@/components/ui'

export function PassengerCreateForm({
  onCreated,
  onCancel,
}: {
  onCreated: (passenger: PassengerDto) => void
  onCancel: () => void
}) {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [passportNumber, setPassportNumber] = useState('')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      const passenger = await passengersApi.create({
        firstName,
        lastName,
        email,
        phoneNumber: phoneNumber || null,
        passportNumber,
        dateOfBirth,
      })
      onCreated(passenger)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not create that passenger.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="mt-3 flex flex-col gap-3">
      <div className="grid grid-cols-2 gap-3">
        <Field label="First name" htmlFor="firstName">
          <TextInput id="firstName" value={firstName} onChange={(e) => setFirstName(e.target.value)} required />
        </Field>
        <Field label="Last name" htmlFor="lastName">
          <TextInput id="lastName" value={lastName} onChange={(e) => setLastName(e.target.value)} required />
        </Field>
      </div>
      <Field label="Email" htmlFor="email">
        <TextInput id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      </Field>
      <div className="grid grid-cols-2 gap-3">
        <Field label="Phone (optional)" htmlFor="phoneNumber">
          <TextInput id="phoneNumber" value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} />
        </Field>
        <Field label="Passport number" htmlFor="passportNumber">
          <TextInput
            id="passportNumber"
            value={passportNumber}
            onChange={(e) => setPassportNumber(e.target.value)}
            required
          />
        </Field>
      </div>
      <Field label="Date of birth" htmlFor="dateOfBirth">
        <TextInput
          id="dateOfBirth"
          type="date"
          value={dateOfBirth}
          onChange={(e) => setDateOfBirth(e.target.value)}
          required
        />
      </Field>
      <ErrorBanner message={error} />
      <div className="flex gap-2">
        <Button type="submit" disabled={submitting} className="flex-1">
          {submitting ? 'Creating…' : 'Create passenger'}
        </Button>
        <Button type="button" variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
      </div>
    </form>
  )
}
