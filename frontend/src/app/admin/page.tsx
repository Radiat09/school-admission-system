'use client';

import { Navbar } from '@/components/layout/Navbar';
import { ClassCreateForm } from '@/components/admin/ClassCreateForm';
import { AdminClassList } from '@/components/admin/AdminClassList';
import { useClasses } from '@/hooks/useClasses';
import { useStudentId } from '@/hooks/useStudentId';

export default function AdminPage() {
  const { data: classes, isPending } = useClasses();
  const { studentId } = useStudentId();

  return (
    <div className="container mx-auto px-4 py-8">
      <Navbar />
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Administration</h1>
        <p className="text-muted-foreground">
          Create admission classes. Each class/shift combination is an independent record with its own seat capacity.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2">
          <ClassCreateForm onCreated={() => {}} />
        </div>
        <div>
          <AdminClassList classes={isPending ? [] : classes ?? []} />
        </div>
      </div>

      <div className="mt-8 text-xs text-muted-foreground">
        No login is required. Student ID: {studentId || '(not set)'}.
      </div>
    </div>
  );
}
