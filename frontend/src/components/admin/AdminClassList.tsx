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
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {classes.map((c) => (
              <Card key={c.id} className="border p-4">
                <CardTitle className="text-base">{c.name}</CardTitle>
                <div className="grid grid-cols-4 gap-2 text-xs mt-2">
                  <div className="bg-muted/50 rounded p-2">
                    <div className="text-muted-foreground">Total</div>
                    <div className="font-mono font-semibold">{c.totalSeats}</div>
                  </div>
                  <div className="bg-amber-50 rounded p-2">
                    <div className="text-amber-900">Held</div>
                    <div className="font-mono font-semibold text-amber-800">{c.heldSeats}</div>
                  </div>
                  <div className="bg-green-50 rounded p-2">
                    <div className="text-green-900">Confirmed</div>
                    <div className="font-mono font-semibold text-green-800">{c.confirmedSeats}</div>
                  </div>
                  <div className="bg-blue-50 rounded p-2">
                    <div className="text-blue-900">Available</div>
                    <div className="font-mono font-semibold text-blue-800">{c.availableSeats}</div>
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
