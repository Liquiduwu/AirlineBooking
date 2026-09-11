// Mirrors the DTOs in AirlineBooking.Application/Features/**/*Dtos.cs. System.Text.Json
// serializes with camelCase property names and string enum values (see Program.cs's
// JsonStringEnumConverter registration), so these line up field-for-field with the wire shape.

export type StaffRole = 'Agent' | 'Admin'

export type FlightStatus = 'Scheduled' | 'Delayed' | 'Departed' | 'Arrived' | 'Cancelled'

export type BookingStatus = 'Held' | 'Confirmed' | 'Cancelled' | 'Expired'

export type CabinClass = 'Economy' | 'Business'

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  expiresAtUtc: string
  username: string
  role: StaffRole
}

export interface RegisterStaffRequest {
  username: string
  email: string
  password: string
  role: StaffRole
}

export interface StaffUserDto {
  id: number
  username: string
  email: string
  role: StaffRole
  isActive: boolean
}

export interface FlightSummaryDto {
  id: number
  flightNumber: string
  originAirportCode: string
  destinationAirportCode: string
  departureUtc: string
  arrivalUtc: string
  aircraftModel: string
  baseFare: number
  status: FlightStatus
}

export interface CreateFlightRequest {
  flightNumber: string
  originAirportCode: string
  destinationAirportCode: string
  departureUtc: string
  arrivalUtc: string
  aircraftId: number
  baseFare: number
}

export interface SeatAvailabilityDto {
  seatId: number
  seatNumber: string
  cabinClass: CabinClass
  isExitRow: boolean
  isAvailable: boolean
}

export interface SeatMapResponse {
  flightId: number
  seats: SeatAvailabilityDto[]
}

export interface FareQuoteDto {
  baseFare: number
  priceMultiplier: number
  loadFactorSurcharge: number
  totalFare: number
}

export interface PassengerDto {
  id: number
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
  passportNumber: string
  dateOfBirth: string
}

export interface CreatePassengerRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
  passportNumber: string
  dateOfBirth: string
}

export interface UpdatePassengerRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
}

export interface BookingDto {
  id: number
  bookingReference: string
  passengerId: number
  passengerName: string
  flightId: number
  flightNumber: string
  seatId: number
  seatNumber: string
  fareClassId: number
  fareClassCode: string
  status: BookingStatus
  fareAmount: number
  heldUntilUtc: string | null
  confirmedUtc: string | null
  cancelledUtc: string | null
  createdUtc: string
}

export interface CreateBookingRequest {
  passengerId: number
  flightId: number
  seatId: number
  fareClassId: number
}

export interface AircraftDto {
  id: number
  registrationNumber: string
  model: string
  totalSeats: number
}

export interface CreateAircraftRequest {
  registrationNumber: string
  model: string
  totalSeats: number
}

export interface FareClassDto {
  id: number
  code: string
  name: string
  cabinClass: CabinClass
  priceMultiplier: number
  isRefundable: boolean
}

export interface CreateFareClassRequest {
  code: string
  name: string
  cabinClass: CabinClass
  priceMultiplier: number
  isRefundable: boolean
}
