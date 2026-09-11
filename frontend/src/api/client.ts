const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5178/api'

let authToken: string | null = null
let onUnauthorized: (() => void) | null = null

/** Called once from AuthProvider so the client can attach the current token and react to 401s. */
export function configureApiClient(options: { token: string | null; onUnauthorized: () => void }) {
  authToken = options.token
  onUnauthorized = options.onUnauthorized
}

/** Shape of the RFC 7807 ProblemDetails / ValidationProblemDetails GlobalExceptionHandler returns. */
export class ApiError extends Error {
  status: number
  fieldErrors: Record<string, string[]> | null

  constructor(status: number, title: string, fieldErrors: Record<string, string[]> | null) {
    super(title)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  body?: unknown
  query?: Record<string, string | number | boolean | undefined | null>
}

function buildQuery(query?: RequestOptions['query']): string {
  if (!query) return ''
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, String(value))
    }
  }
  const str = params.toString()
  return str ? `?${str}` : ''
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  if (authToken) {
    headers.Authorization = `Bearer ${authToken}`
  }

  const response = await fetch(`${API_BASE_URL}${path}${buildQuery(options.query)}`, {
    method: options.method ?? 'GET',
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  })

  if (response.status === 401) {
    onUnauthorized?.()
    throw new ApiError(401, 'Session expired', null)
  }

  if (response.status === 204) {
    return undefined as T
  }

  const isJson = response.headers.get('content-type')?.includes('application/json')
  const data = isJson ? await response.json() : null

  if (!response.ok) {
    const title = data?.title ?? `Request failed (${response.status})`
    const detail = data?.detail ? `${title}: ${data.detail}` : title
    throw new ApiError(response.status, detail, data?.errors ?? null)
  }

  return data as T
}
