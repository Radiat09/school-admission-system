import { describe, it, expect } from 'vitest';
import { studentIdSchema, createClassSchema } from './validations';

describe('studentIdSchema', () => {
  it('accepts a valid 17-digit student ID', () => {
    expect(studentIdSchema.safeParse('12345678901234567').success).toBe(true);
  });

  it('rejects a non-17-digit ID', () => {
    expect(studentIdSchema.safeParse('12345').success).toBe(false);
  });

  it('rejects non-digit characters', () => {
    expect(studentIdSchema.safeParse('1234567890123456a').success).toBe(false);
  });
});

describe('createClassSchema', () => {
  it('accepts valid input', () => {
    expect(createClassSchema.safeParse({ name: 'Math 101', totalSeats: '30' }).success).toBe(true);
  });

  it('rejects empty class name', () => {
    expect(createClassSchema.safeParse({ name: '', totalSeats: '30' }).success).toBe(false);
  });

  it('rejects non-positive seats', () => {
    expect(createClassSchema.safeParse({ name: 'Math 101', totalSeats: '0' }).success).toBe(false);
  });
});
