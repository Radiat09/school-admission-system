'use client';

import { Card, CardContent, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { useStudentId } from '@/hooks/useStudentId';
import { admissionApi } from '@/lib/api';
import { useToast } from '@/components/ui/use-toast';
import type { ClassDto } from '@/lib/api';
export function ClassCard({ classItem, onApply }: { classItem: ClassDto; onApply: (holdId: string) => void }) {
  const { studentId } = useStudentId();
  const { error, success } = useToast();

  const handleApply = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!studentId) {
      error({ title: 'Student ID required', description: 'Enter your 17-digit birth registration number to apply.' });
      return;
    }
    try {
      const hold = await admissionApi.apply(classItem.id);
      onApply(hold.holdId);
      success({ title: 'Seat reserved', description: `Your seat in ${classItem.name} is reserved.` });
    } catch (err: any) {
      if (err.status === 409) {
        error({ title: 'No seats available', description: 'This class is currently full.' });
      } else if (err.status === 400) {
        error({ title: 'Invalid request', description: err.message || 'Please check your student ID.' });
      } else {
        error({ title: 'Application failed', description: 'Could not reserve a seat. Please try again.' });
      }
    }
  };

  return (
    <Card className="border min-w-0">
      <CardContent className="p-5 md:p-6">
        <div className="flex flex-col gap-4 md:flex-row md:items-start min-w-0">
          <div className="min-w-0">
            <CardTitle className="text-lg font-bold">{classItem.name}</CardTitle>
            <p className="text-sm text-muted-foreground truncate">Class code: {classItem.id}</p>
          </div>
          <div className="flex flex-col gap-3 min-w-0">
            <div className="flex flex-wrap justify-between gap-2 min-w-0 w-full">
              <SeatChip label="Total" value={classItem.totalSeats} variant="bg-muted/50 text-foreground" />
              <SeatChip label="Held" value={classItem.heldSeats} variant="bg-amber-50 text-amber-800 border-amber-200" />
              <SeatChip label="Confirmed" value={classItem.confirmedSeats} variant="bg-green-50 text-green-800 border-green-200" />
              <SeatChip label="Available" value={classItem.availableSeats} variant="bg-blue-50 text-blue-800 border-blue-200" />
            </div>
            <form onSubmit={handleApply} className="flex w-full md:w-auto items-center gap-2 min-w-0">
              <Button
                type="submit"
                disabled={classItem.availableSeats <= 0 || !studentId}
                className="w-full md:w-auto"
              >
                {classItem.availableSeats <= 0 ? 'Full' : 'Apply Now'}
              </Button>
            </form>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

function SeatChip({ label, value, variant }: { label: string; value: number; variant: string }) {
  return (
    <div className={`rounded-lg p-2 border min-w-[70px] text-center ${variant}`}>
      <div className="text-[10px] text-muted-foreground uppercase tracking-wider truncate">{label}</div>
      <div className="font-mono font-semibold text-sm truncate">{value}</div>
    </div>
  );
}
