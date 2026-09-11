import { apiRequest } from './client'
import type {
  AircraftDto,
  BookingDto,
  CreateAircraftRequest,
  CreateBookingRequest,
  CreateFareClassRequest,
  CreateFlightRequest,
  CreatePassengerRequest,
  FareClassDto,
  FareQuoteDto,
  FlightSummaryDto,
  LoginRequest,
  LoginResponse,
  PassengerDto,
  RegisterStaffRequest,
  SeatMapResponse,
  StaffUserDto,
  UpdatePassengerRequest,
} from './types'

export const authApi = {
  login: (request: LoginRequest) => apiRequest<LoginResponse>('/auth/login', { method: 'POST', body: request }),
  register: (request: RegisterStaffRequest) =>
    apiRequest<StaffUserDto>('/auth/register', { method: 'POST', body: request }),
}

export const flightsApi = {
  search: (params: { originAirportCode?: string; destinationAirportCode?: string; departureDate?: string }) =>
    apiRequest<FlightSummaryDto[]>('/flights', { query: params }),
  getById: (id: number) => apiRequest<FlightSummaryDto>(`/flights/${id}`),
  create: (request: CreateFlightRequest) => apiRequest<FlightSummaryDto>('/flights', { method: 'POST', body: request }),
  getSeatMap: (id: number) => apiRequest<SeatMapResponse>(`/flights/${id}/seats`),
  getFareQuote: (id: number, fareClassId: number) =>
    apiRequest<FareQuoteDto>(`/flights/${id}/fare-quote`, { query: { fareClassId } }),
}

export const bookingsApi = {
  getAll: () => apiRequest<BookingDto[]>('/bookings'),
  getByReference: (reference: string) => apiRequest<BookingDto>(`/bookings/${reference}`),
  createHold: (request: CreateBookingRequest) => apiRequest<BookingDto>('/bookings', { method: 'POST', body: request }),
  confirm: (reference: string) => apiRequest<BookingDto>(`/bookings/${reference}/confirm`, { method: 'POST' }),
  cancel: (reference: string) => apiRequest<BookingDto>(`/bookings/${reference}/cancel`, { method: 'POST' }),
}

export const passengersApi = {
  getAll: () => apiRequest<PassengerDto[]>('/passengers'),
  getById: (id: number) => apiRequest<PassengerDto>(`/passengers/${id}`),
  create: (request: CreatePassengerRequest) => apiRequest<PassengerDto>('/passengers', { method: 'POST', body: request }),
  update: (id: number, request: UpdatePassengerRequest) =>
    apiRequest<PassengerDto>(`/passengers/${id}`, { method: 'PUT', body: request }),
}

export const aircraftApi = {
  getAll: () => apiRequest<AircraftDto[]>('/aircraft'),
  create: (request: CreateAircraftRequest) => apiRequest<AircraftDto>('/aircraft', { method: 'POST', body: request }),
}

export const fareClassesApi = {
  getAll: () => apiRequest<FareClassDto[]>('/fare-classes'),
  create: (request: CreateFareClassRequest) =>
    apiRequest<FareClassDto>('/fare-classes', { method: 'POST', body: request }),
}
