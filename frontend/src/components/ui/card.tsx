'use client';

import * as React from 'react';
import { cn } from '@/lib/utils';

export interface CardProps extends React.HTMLAttributes<HTMLDivElement> {}

function Card({ className, ...props }: CardProps) {
  return (
    <div className={cn('rounded-xl border bg-card text-card-foreground shadow-sm', className)} {...props} />
  );
}
Card.displayName = 'Card';

function CardHeader(props: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('flex flex-col space-y-1.5 p-6', props.className)} {...props} />;
}
CardHeader.displayName = 'CardHeader';

function CardTitle(props: React.HTMLAttributes<HTMLHeadingElement>) {
  return (
    <h3 className={cn('text-2xl font-semibold leading-none tracking-tight', props.className)} {...props} />
  );
}
CardTitle.displayName = 'CardTitle';

function CardContent(props: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('p-6 pt-0', props.className)} {...props} />;
}
CardContent.displayName = 'CardContent';

function CardFooter(props: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('flex items-center p-6 pt-0', props.className)} {...props} />;
}
CardFooter.displayName = 'CardFooter';

export { Card, CardHeader, CardTitle, CardContent, CardFooter };
