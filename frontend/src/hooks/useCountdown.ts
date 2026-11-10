'use client';

import { useEffect, useRef, useState } from 'react';

export function useCountdown(expiresAt: string | null) {
  const [remaining, setRemaining] = useState(0);
  const [expired, setExpired] = useState(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    if (!expiresAt) {
      setRemaining(0);
      setExpired(true);
      return;
    }

    const target = new Date(expiresAt).getTime();

    const tick = () => {
      const now = Date.now();
      const diff = Math.max(0, target - now);
      setRemaining(diff);
      setExpired(diff <= 0);
    };

    tick();
    intervalRef.current = setInterval(tick, 1000);
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [expiresAt]);

  const formatted = (() => {
    const totalSeconds = Math.floor(remaining / 1000);
    const m = Math.floor(totalSeconds / 60);
    const s = totalSeconds % 60;
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  })();

  return { remaining, expired, formatted };
}
