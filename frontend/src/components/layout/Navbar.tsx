'use client';

import Link from 'next/link';
import { useStudentId } from '@/hooks/useStudentId';
import { setStudentId } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
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
            <span className="text-xl font-bold tracking-tight">ONNOROKOM SCHOOL</span>
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
          <span className="text-xl font-bold tracking-tight">ONNOROKOM SCHOOL</span>
          <span className="text-xs font-mono text-muted-foreground">Admissions</span>
        </div>
        <div className="flex items-center gap-3">
          <form onSubmit={form.handleSubmit(onSubmit)} className="flex items-center gap-2 w-[260px]">
            <Label className="sr-only">Student birth registration number</Label>
            <Input
              placeholder="17-digit birth registration no."
              value={form.getValues('studentId')}
              onChange={(e) => {
                form.setValue('studentId', e.target.value);
              }}
              onBlur={() => form.trigger('studentId')}
              className="w-full"
            />
            <Button type="submit" size="sm" disabled={!form.formState.isSubmitting && !form.getValues('studentId')}>
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
