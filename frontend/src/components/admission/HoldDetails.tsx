'use client';

import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { useCountdown } from '@/hooks/useCountdown';
import { admissionApi } from '@/lib/api';
import { useToast } from '@/components/ui/use-toast';
import { Badge } from '@/components/ui/badge';
import { useRouter } from 'next/navigation';

export type HoldStatus = 'HELD' | 'CONFIRMED' | 'EXPIRED' | 'CANCELLED';

interface HoldDetailsProps {
  hold: { holdId: string; classId: string; status: HoldStatus; expiresAt: string };
  className?: string;
  onStatusChange: () => void;
}

export function HoldDetails({ hold, className, onStatusChange }: HoldDetailsProps) {
  const router = useRouter();
  const { error, success } = useToast();
  const { remaining, expired, formatted } = useCountdown(hold.expiresAt);

  const handleConfirm = async () => {
    try {
      await admissionApi.confirmHold(hold.holdId);
      success({ title: 'Admission confirmed', description: 'Your seat is now confirmed.' });
      onStatusChange();
      router.refresh();
    } catch (err: any) {
      if (err.status === 409) {
        error({ title: 'Cannot confirm', description: err.message || 'This reservation cannot be confirmed.' });
      } else {
        error({ title: 'Confirmation failed', description: 'Please try again later.' });
      }
    }
  };

  const handleCancel = async () => {
    if (!window.confirm('Are you sure you want to cancel this application? The seat will be released.')) {
      return;
    }
    try {
      await admissionApi.cancelHold(hold.holdId);
      success({ title: 'Application cancelled', description: 'Your seat has been released.' });
      onStatusChange();
      router.refresh();
    } catch (err: any) {
      error({ title: 'Cancellation failed', description: err.message || 'Could not cancel the application.' });
    }
  };

  const isActive = hold.status === 'HELD' && !expired;

  return (
    <Card className="border">
      <CardHeader>
        <CardTitle className="text-xl">{className ?? hold.classId}</CardTitle>
        <div className="flex items-center gap-2 mt-2">
          <Badge variant={statusVariant(hold.status)}>{hold.status}</Badge>
          <span className="text-sm text-muted-foreground">Hold #{hold.holdId.slice(-6)}</span>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div className="bg-muted/50 rounded p-3">
            <div className="text-xs text-muted-foreground">Class code</div>
            <div className="font-mono font-semibold">{hold.classId}</div>
          </div>          <div className="bg-muted/50 rounded p-3">
            <div className="text-xs text-muted-foreground">Expires at</div>
            <div className="font-mono font-semibold">{new Date(hold.expiresAt).toLocaleString()}</div>
          </div>
        </div>
        {isActive && (
          <div className="bg-amber-50 border border-amber-200 rounded p-3">
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm font-medium text-amber-800">Time remaining</span>
              <span className="text-xs text-amber-700">{formatted}</span>
            </div>
            <div className="w-full bg-amber-200 rounded-full h-2">
              <div
                className="bg-amber-600 h-2 rounded-full"
                style={{ width: Math.max(0, Math.min(100, (remaining / 600000) * 100)) }}
              />
            </div>
          </div>
        )}
        <div className="flex items-center gap-3">
          <Button
            onClick={handleConfirm}
            disabled={!isActive}
            className="w-full"
          >
            {hold.status === 'CONFIRMED' ? 'Confirmed' : expired ? 'Expired' : 'Pay Admission Fee'}
          </Button>
          <Button
            variant="outline"
            onClick={handleCancel}
            disabled={hold.status !== 'HELD'}
            className="w-full"
          >
            Cancel Application
          </Button>
        </div>
        {hold.status === 'EXPIRED' && (
          <div className="text-sm text-destructive">
            Your reservation has expired. The seat has been released. You may apply again if seats remain.
          </div>
        )}
        {hold.status === 'CANCELLED' && (
          <div className="text-sm text-muted-foreground">
            Your application was cancelled. The seat has been released.
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function statusVariant(status: HoldStatus) {
  switch (status) {
    case 'HELD': return 'secondary';
    case 'CONFIRMED': return 'default';
    case 'EXPIRED': return 'destructive';
    case 'CANCELLED': return 'outline';
  }
}
