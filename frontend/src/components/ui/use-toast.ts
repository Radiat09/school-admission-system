import { toast } from 'sonner';

export function useToast() {
  return {
    toast: (o: { title?: string; description?: string }) =>
      toast(o.title || 'Admission', { description: o.description }),
    error: (o: { title?: string; description?: string }) =>
      toast.error(o.description ?? o.title ?? 'Error'),
    success: (o: { title?: string; description?: string }) =>
      toast.success(o.description ?? o.title ?? 'Success'),
  };
}
