'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ClassDto } from '@/lib/api';

export function AdminClassList({ classes }: { classes: ClassDto[] }) {
  return (
    <Card className="border">
      <CardHeader>
        <CardTitle>Existing Classes</CardTitle>
        <p className="text-sm text-muted-foreground">
          Seat counts update automatically every few seconds.
        </p>
      </CardHeader>
      <CardContent className="space-y-3">
        {classes.length === 0 ? (
          <div className="text-sm text-muted-foreground text-center py-8">No classes created yet.</div>
        ) : (
          <div className="grid grid-cols-1 gap-4">
            {classes.map((c) => (
              <Card key={c.id} className="border p-5 min-w-0">
                <CardTitle className="text-base font-semibold mb-2">{c.name}</CardTitle>
                <div className="flex flex-wrap justify-between gap-2 mt-2 min-w-0">
                  <div className="bg-muted/50 rounded-lg p-3 min-w-[80px]">
                    <div className="text-[10px] text-muted-foreground uppercase tracking-wider truncate">Total</div>
                    <div className="font-mono font-semibold text-lg truncate">{c.totalSeats}</div>
                  </div>
                  <div className="bg-amber-50 rounded-lg p-3 border border-amber-100 min-w-[80px]">
                    <div className="text-[10px] text-amber-900 uppercase tracking-wider truncate">Held</div>
                    <div className="font-mono font-semibold text-lg text-amber-800 truncate">{c.heldSeats}</div>
                  </div>
                  <div className="bg-green-50 rounded-lg p-3 border border-green-100 min-w-[80px]">
                    <div className="text-[10px] text-green-900 uppercase tracking-wider truncate">Confirmed</div>
                    <div className="font-mono font-semibold text-lg text-green-800 truncate">{c.confirmedSeats}</div>
                  </div>
                  <div className="bg-blue-50 rounded-lg p-3 border border-blue-100 min-w-[80px]">
                    <div className="text-[10px] text-blue-900 uppercase tracking-wider truncate">Available</div>
                    <div className="font-mono font-semibold text-lg text-blue-800 truncate">{c.availableSeats}</div>
                  </div>
                </div>
              </Card>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
