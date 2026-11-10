'use client';

import { Navbar } from '@/components/layout/Navbar';
import { HoldDetails } from '@/components/admission/HoldDetails';
import { useHold } from '@/hooks/useHold';
import { useClasses } from '@/hooks/useClasses';
import { HoldSkeleton } from '@/components/admission/skeletons';
import { notFound } from 'next/navigation';

export default function ApplicationDetailsPage({
  params,
}: {
  params: { holdId: string };
}) {
  const { holdId } = params;
  const { data: hold, isPending, error } = useHold(holdId);
  const { data: classes } = useClasses();

  if (!isPending && !hold) {
    notFound();
  }

  const className = hold ? (classes?.find((c) => c.id === hold.classId)?.name || hold.classId) : undefined;

  return (
    <div className="container mx-auto px-4 py-8">
      <Navbar />
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Application Details</h1>
      </div>

      {isPending ? (
        <HoldSkeleton />
      ) : error ? (
        <div className="rounded-xl border border-destructive/50 bg-destructive/10 p-4 text-destructive">
          Failed to load application. {error.message}
        </div>
      ) : hold ? (
        <HoldDetails hold={hold} className={className} onStatusChange={() => {}} />
      ) : null}
    </div>
  );
}
