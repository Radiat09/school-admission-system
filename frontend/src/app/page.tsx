'use client';

import { Navbar } from '@/components/layout/Navbar';
import { ClassesListSkeleton, ClassesEmpty } from '@/components/admission/skeletons';
import { ClassCard } from '@/components/admission/ClassCard';
import { useClasses } from '@/hooks/useClasses';
import { useRouter } from 'next/navigation';
import { useStudentId } from '@/hooks/useStudentId';

export default function HomePage() {
  const router = useRouter();
  const { data: classes, isPending, error } = useClasses();
  const { studentId, isInitialized } = useStudentId();

  if (!isInitialized) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Navbar />
        <div className="text-center py-20 text-muted-foreground">Loading…</div>
      </div>
    );
  }

  const handleApply = (holdId: string) => {
    router.push(`/applications/${holdId}`);
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <Navbar />
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Admission Portal</h1>
        <p className="text-muted-foreground">
          First come, first served. Each class/shift combination has independent seat capacity.
        </p>
      </div>

      <div className="mb-6 flex items-center gap-2 text-sm text-muted-foreground">
        <span className="w-2 h-2 rounded-full bg-green-500" />
        <span>Live seat counts — refresh automatically every few seconds</span>
      </div>

      {isPending ? (
        <ClassesListSkeleton />
      ) : error ? (
        <div className="rounded-xl border border-destructive/50 bg-destructive/10 p-4 text-destructive">
          Failed to load classes. {error.message}
        </div>
      ) : classes && classes.length === 0 ? (
        <ClassesEmpty />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {classes.map((c) => (
            <ClassCard key={c.id} classItem={c} onApply={handleApply} />
          ))}
        </div>
      )}

      <div className="mt-10 flex items-center gap-2 text-sm text-muted-foreground">
        <span className="w-2 h-2 rounded-full bg-blue-500" />
        <span>
          Enter your student ID above to apply. Your ID is saved in your browser and persists across reloads.
        </span>
      </div>
    </div>
  );
}
