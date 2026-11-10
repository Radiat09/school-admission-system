'use client';

import { Skeleton } from '@/components/ui/skeleton';
import { ClassDto } from '@/lib/api';

export function ClassSkeleton() {
  return (
    <div className="animate-pulse rounded-xl border bg-muted p-5">
      <div className="h-4 w-3/4 mb-3 rounded bg-muted"></div>
      <div className="h-4 w-1/2 mb-2 rounded bg-muted"></div>
      <div className="grid grid-cols-4 gap-2">
        {[0, 1, 2, 3].map((i) => (
          <div key={i} className="h-10 rounded bg-muted" />
        ))}
      </div>
      <div className="mt-4 h-10 w-3/4 rounded bg-muted" />
    </div>
  );
}

export function ClassesListSkeleton() {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {[1, 2, 3].map((i) => (
        <div key={i} className="col-span-1">
          <ClassSkeleton />
        </div>
      ))}
    </div>
  );
}

export function ClassesEmpty() {
  return (
    <div className="rounded-xl border bg-muted/50 p-12 text-center">
      <p className="text-sm text-muted-foreground">No admission classes available at the moment.</p>
    </div>
  );
}

export function HoldSkeleton() {
  return (
    <div className="animate-pulse rounded-xl border bg-muted p-6">
      <div className="h-5 w-1/3 mb-2 rounded bg-muted"></div>
      <div className="h-4 w-full mb-4 rounded bg-muted"></div>
      <div className="h-10 w-1/2 rounded bg-muted" />
      <div className="h-10 w-1/2 rounded bg-muted" />
    </div>
  );
}
