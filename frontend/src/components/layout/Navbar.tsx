'use client';

import Link from 'next/link';
import { useStudentId } from '@/hooks/useStudentId';
import { setStudentId } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { studentIdSchema } from '@/lib/validations';
import { useToast } from '@/components/ui/use-toast';
import { Badge } from '@/components/ui/badge';

export function Navbar() {
  const { studentId, setStudentId: setId, isInitialized, currentId } = useStudentId();
  const { toast } = useToast();

  const form = useForm<{ studentId: string }>({
    resolver: zodResolver(studentIdSchema),
    defaultValues: { studentId: currentId || '' },
  });

  const onSubmit = (values: { studentId: string }) => {
    setId(values.studentId);
    toast({ description: 'Student ID saved.' });
  };

  if (!isInitialized) {
    return (
      <nav className="border-b bg-background/95 backdrop-blur supports-[backdrop-blur]:bg-background/80 sticky top-0 z-50">
        <div className="container mx-auto px-4 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Link href="/" className="text-xl font-bold tracking-tight hover:opacity-80 transition-opacity">
              ONNOROKOM SCHOOL
            </Link>
          </div>
          <div className="flex items-center gap-3 animate-pulse">
            <span className="text-sm text-muted-foreground">Loading…</span>
          </div>
        </div>
      </nav>
    );
  }

  return (
    <nav className="border-b bg-background/95 backdrop-blur supports-[backdrop-blur]:bg-background/80 sticky top-0 z-50">
      <div className="container mx-auto px-4 h-16 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Link href="/" className="text-xl font-bold tracking-tight hover:opacity-80 transition-opacity">
            ONNOROKOM SCHOOL
          </Link>
          <span className="text-xs font-mono text-muted-foreground">Admissions</span>
        </div>
        <div className="flex items-center gap-3">
          <form onSubmit={form.handleSubmit(onSubmit)} className="flex items-center gap-2 w-[260px]">
            <Label className="sr-only">Student birth registration number</Label>
            <input
              {...form.register('studentId')}
              placeholder="17-digit birth registration no."
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
            />
            {form.formState.errors.studentId && (
              <span className="text-xs text-destructive">{form.formState.errors.studentId.message}</span>
            )}
            <Button type="submit" size="sm" disabled={form.formState.isSubmitting}>
              Save ID
            </Button>
          </form>
          {studentId && (
            <Badge variant="outline">ID: {studentId}</Badge>
          )}
          <Link href="/admin" className="text-sm font-medium hover:underline">
            Admin
          </Link>
        </div>
      </div>
    </nav>
  );
}
