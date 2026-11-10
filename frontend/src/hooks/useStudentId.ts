'use client';

import { useCallback, useEffect, useState } from 'react';
import { initializeStudentId, setStudentId, getStudentId } from '@/lib/api';

export function useStudentId() {
  const [studentId, setStudentIdState] = useState<string | null>(null);
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    const id = initializeStudentId();
    setStudentIdState(id);
    setIsInitialized(true);
  }, []);

  const setId = useCallback(
    (id: string | null) => {
      setStudentId(id);
      setStudentIdState(id);
    },
    [setStudentId]
  );

  const clear = useCallback(() => {
    setStudentId(null);
    setStudentIdState(null);
  }, []);

  return { studentId, setStudentId: setId, clearStudentId: clear, isInitialized, currentId: getStudentId() };
}
