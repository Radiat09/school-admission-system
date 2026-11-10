'use client';

import { useQuery } from '@tanstack/react-query';
import { admissionApi } from '@/lib/api';
import type { HoldDto, MyHoldDto } from '@/lib/api';

export function useHold(holdId: string | undefined) {
  return useQuery({
    queryKey: ['hold', holdId],
    queryFn: () => (holdId ? admissionApi.getHold(holdId) : null),
    enabled: !!holdId,
    refetchInterval: 3000,
    refetchIntervalInBackground: true,
    staleTime: 1000,
  });
}

export function useMyHolds(studentId: string | null) {
  const isValid = !!studentId && /^\d{17}$/.test(studentId);
  return useQuery({
    queryKey: ['myHolds', studentId],
    queryFn: admissionApi.getMyHolds,
    enabled: isValid,
    refetchInterval: 5000,
    refetchIntervalInBackground: true,
    staleTime: 1000,
  });
}
