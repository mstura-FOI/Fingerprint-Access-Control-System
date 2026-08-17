import { useMemo } from "react";
import { useAccessCodeApiService } from "@/packages";
export function useAccessCodeService() {
  const api = useAccessCodeApiService();
  return useMemo(() => ({ generate: () => api.generate() }), [api]);
}
