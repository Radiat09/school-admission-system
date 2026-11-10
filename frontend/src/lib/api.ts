import 'client-only';

const API_BASE_URL = (typeof process !== 'undefined' && process.env.NEXT_PUBLIC_API_BASE_URL)
  ? process.env.NEXT_PUBLIC_API_BASE_URL
  : 'http://localhost:8080';

let storedStudentId: string | null = null;

export function setStudentId(id: string | null) {
  storedStudentId = id;
  if (typeof window !== 'undefined') {
    if (id) {
      localStorage.setItem('school_admission_student_id', id);
    } else {
      localStorage.removeItem('school_admission_student_id');
    }
  }
}

export function getStoredStudentId(): string | null {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('school_admission_student_id');
  }
  return storedStudentId;
}

export function getStudentId(): string | null {
  return storedStudentId ?? getStoredStudentId();
}

export function initializeStudentId(): string | null {
  const id = getStoredStudentId();
  if (id) storedStudentId = id;
  return storedStudentId;
}

async function request(input: string, init: RequestInit = {}): Promise<Response> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(init.headers as Record<string, string> || {}),
  };
  const id = getStudentId();
  if (id) {
    headers['X-Student-Id'] = id;
  }
  return fetch(`${API_BASE_URL}${input}`, { ...init, headers });
}

async function json<T>(input: string, init: RequestInit = {}): Promise<T> {
  const res = await request(input, init);
  if (res.status === 204) {
    return undefined as unknown as T;
  }
  if (!res.ok) {
    let message = 'Request failed';
    try {
      const body = (await res.json()) as { error?: string; message?: string };
      message = body?.message || body?.error || message;
    } catch {
      // ignore parse errors
    }
    const err: any = new Error(message);
    err.status = res.status;
    throw err;
  }
  return (await res.json()) as T;
}

export const admissionApi = {
  createClass: (name: string, totalSeats: number) =>
    json<{ id: string; name: string; totalSeats: number }>('/classes', {
      method: 'POST',
      body: JSON.stringify({ name, totalSeats }),
    }),
  getClasses: () => json<ClassDto[]>('/classes'),
  getClass: (classId: string) => json<ClassDto>(`/classes/${encodeURIComponent(classId)}`),
  apply: (classId: string) =>
    json<CreateHoldResponse>(`/classes/${encodeURIComponent(classId)}/holds`, {
      method: 'POST',
    }),
  getHold: (holdId: string) => json<HoldDto>(`/holds/${encodeURIComponent(holdId)}`),
  confirmHold: (holdId: string) =>
    json<ConfirmResponse>(`/holds/${encodeURIComponent(holdId)}/confirm`, {
      method: 'POST',
    }),
  cancelHold: (holdId: string) => json<void>(`/holds/${encodeURIComponent(holdId)}`, { method: 'DELETE' }),
  getMyHolds: () => json<MyHoldDto[]>('/me/holds'),
};

export type HoldStatus = 'HELD' | 'CONFIRMED' | 'EXPIRED' | 'CANCELLED';

export interface ClassDto {
  id: string;
  name: string;
  totalSeats: number;
  heldSeats: number;
  confirmedSeats: number;
  availableSeats: number;
}

export interface CreateHoldResponse {
  holdId: string;
  classId: string;
  studentId: string;
  status: HoldStatus;
  expiresAt: string;
}

export interface HoldDto {
  holdId: string;
  classId: string;
  status: HoldStatus;
  expiresAt: string;
}

export interface ConfirmResponse {
  holdId: string;
  status: HoldStatus;
}

export interface MyHoldDto {
  holdId: string;
  classId: string;
  className: string;
  status: HoldStatus;
  expiresAt: string;
}

export interface ErrorBody {
  error: string;
  message?: string;
}

export interface AppError extends Error {
  status?: number;
}
