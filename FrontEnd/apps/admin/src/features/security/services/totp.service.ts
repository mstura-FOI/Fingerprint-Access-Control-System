import { TotpVerifyDto, useAuthApiService } from "@/packages";

export function useTotpService() {
  const api = useAuthApiService();
  return {
    beginSetup: () => api.beginSetup(),
    verifySetup: (code: string) => api.verifySetup(new TotpVerifyDto({ code })),
  };
}
