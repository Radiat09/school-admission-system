import { z } from 'zod';

export const studentIdSchema = z
  .string()
  .length(17, 'Student ID must be exactly 17 digits.')
  .regex(/^\d{17}$/, 'Student ID must contain only digits.');

export const createClassSchema = z.object({
  name: z.string().min(1, 'Class name is required.').max(200, 'Class name is too long.'),
  totalSeats: z
    .string()
    .min(1, 'Total seats is required.')
    .refine((val) => /^\d+$/.test(val), 'Total seats must be a positive integer.')
    .refine((val) => parseInt(val, 10) > 0, 'Total seats must be greater than zero.'),
});

export type CreateClassInput = z.infer<typeof createClassSchema>;
