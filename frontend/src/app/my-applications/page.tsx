'use client';

import { Navbar } from '@/components/layout/Navbar';
import { useMyHolds } from '@/hooks/useHold';
import { useStudentId } from '@/hooks/useStudentId';
import { Card } from '@/components/ui/card';
import Link from 'next/link';
import { HoldSkeleton } from '@/components/admission/skeletons';
import type { MyHoldDto } from '@/lib/api';

export default function MyApplicationsPage() {
  const { studentId, isInitialized } = useStudentId();
  const { data: holds, isPending, error } = useMyHolds(studentId);

  if (!isInitialized) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Navbar />
        <div className="text-center py-20 text-muted-foreground">Loading…</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <Navbar />
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">My Applications</h1>
        <p className="text-muted-foreground">
          Applications for student ID{' '}
          <span className="font-mono font-semibold">{studentId || '—'}</span>.
          Status updates automatically.
        </p>
      </div>

      {!studentId && (
        <div className="rounded-xl border border-amber/50 bg-amber/10 p-4 text-amber-900">
          Please enter your 17-digit birth registration number on the home page to view your applications.
        </div>
      )}

      {isPending ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3].map((i) => (
            <div key={i}>
              <HoldSkeleton />
            </div>
          ))}
        </div>
      ) : error ? (
        <div className="rounded-xl border border-destructive/50 bg-destructive/10 p-4 text-destructive">
          Failed to load applications. {error.message}
        </div>
      ) : holds && holds.length === 0 ? (
        <div className="rounded-xl border bg-muted/50 p-12 text-center">
          <p className="text-sm text-muted-foreground">No applications found.</p>
          <p className="text-sm text-muted-foreground mt-2">
            Apply for a class on the home page to get started.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {(holds as MyHoldDto[]).map((h: MyHoldDto) => (
            <Card key={h.holdId} className="border p-5">
              <div className="flex justify-between items-start mb-2">
                <h3 className="font-semibold">{h.className}</h3>
                <span className="text-xs font-mono text-muted-foreground">#{h.holdId.slice(-6)}</span>
              </div>
              <div className="text-sm text-muted-foreground mb-1">Class: {h.classId}</div>
              <div className="flex items-center gap-2 mb-3">
                <span className={`px-2 py-0.5 rounded text-xs font-medium border ${statusClass(h.status)}`}>
                  {h.status}
                </span>
                <span className="text-xs text-muted-foreground ml-auto">
                  Expires {new Date(h.expiresAt).toLocaleString()}
                </span>
              </div>
              <Link href={`/applications/${h.holdId}`} className="text-sm text-primary underline-offset-4 hover:underline">
                View details
              </Link>
            </Card>
          ))}
        </div>
      )}
    </div>
  );

  function statusClass(status: string) {
    switch (status) {
      case 'HELD':
        return 'border-amber-200 text-amber-800 bg-amber-50';
      case 'CONFIRMED':
        return 'border-green-200 text-green-800 bg-green-50';
      case 'EXPIRED':
        return 'border-red-200 text-red-800 bg-red-50';
      case 'CANCELLED':
        return 'border-gray-200 text-gray-800 bg-gray-50';
      default:
        return 'border-gray-200 text-gray-800 bg-gray-50';
    }
  }
}
