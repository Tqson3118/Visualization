import { toast } from '@/lib/toast';

/** useToast — G-F1b: delegate sang vue-sonner (giữ nguyên API cũ). */
export function useToast() {
  return {
    show: toast.show,
    success: toast.success,
    error: toast.error,
    warning: toast.warning,
    info: toast.info,
    dismiss: toast.dismiss,
  };
}
