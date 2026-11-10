'use client';

import { useQuery } from '@tanstack/react-query';
import { admissionApi } from '@/lib/api';
import type { ClassDto } from '@/lib/api';

export function useClasses() {
  return useQuery({
    queryKey: ['classes'],
    queryFn: admissionApi.getClasses,
    refetchInterval: 3000,
    refetchIntervalInBackground: true,
    staleTime: 1000,
  });
}

export function useClass(classId: string | undefined) {
  return useQuery({
    queryKey: ['class', classId],
    queryFn: () => (classId ? admissionApi.getClass(classId) : null),
    enabled: !!classId,
    refetchInterval: 3000,
    refetchIntervalInBackground: true,
    staleTime: 1000,
  });
}
