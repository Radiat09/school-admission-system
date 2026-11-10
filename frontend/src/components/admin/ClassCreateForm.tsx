'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createClassSchema } from '@/lib/validations';
import { admissionApi } from '@/lib/api';
import { useToast } from '@/components/ui/use-toast';
import { useClasses } from '@/hooks/useClasses';

export function ClassCreateForm({ onCreated }: { onCreated: () => void }) {
  const { error, success } = useToast();
  const { refetch } = useClasses();

  const form = useForm<{ name: string; totalSeats: string }>({
    resolver: zodResolver(createClassSchema),
    defaultValues: { name: '', totalSeats: '' },
  });

  const onSubmit = async (values: { name: string; totalSeats: string }) => {
    try {
      await admissionApi.createClass(values.name, parseInt(values.totalSeats, 10));
      success({ title: 'Class created', description: `${values.name} has been created.` });
      form.reset();
      refetch();
      onCreated();
    } catch (err: any) {
      error({ title: 'Failed to create class', description: err.message || 'Could not create the class.' });
    }
  };

  return (
    <Card className="border">
      <CardHeader>
        <CardTitle>Create Admission Class</CardTitle>
        <p className="text-sm text-muted-foreground">
          One class record represents one class &amp; shift combination (e.g. Class 6 - Morning Shift).
        </p>
      </CardHeader>
      <CardContent className="space-y-4">
        <form onSubmit={form.handleSubmit(onSubmit)} className="grid grid-cols-1 md:grid-cols-2 gap-4 items-end">
          <div className="flex flex-col gap-2">
            <Label htmlFor="name">Class Name</Label>
            <Input
              id="name"
              placeholder="e.g. Class 6 - Morning Shift"
              {...form.register('name')}
            />
            {form.formState.errors.name && (
              <span className="text-sm text-destructive">{form.formState.errors.name.message}</span>
            )}
          </div>
          <div className="flex flex-col gap-2">
            <Label htmlFor="totalSeats">Total Seats</Label>
            <Input id="totalSeats" type="number" {...form.register('totalSeats')} />
            {form.formState.errors.totalSeats && (
              <span className="text-sm text-destructive">{form.formState.errors.totalSeats.message}</span>
            )}
          </div>
          <Button type="submit" className="w-full md:col-span-2">
            Create Class
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
