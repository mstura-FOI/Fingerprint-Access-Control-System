import { useMemo } from "react";
import {
  AccountCreateDto,
  AccountDeleteDto,
  AccountUpdateDto,
  useAccountApiService,
} from "@/packages";

const isCreatedResponse = (error: unknown) =>
  (error as { status?: number }).status === 201;

export function useAccountsService() {
  const api = useAccountApiService();
  return useMemo(
    () => ({
      getList: (page: number, pageSize: number) =>
        api.accountGET2(page, pageSize, (page - 1) * pageSize),
      getById: (id: string) => api.accountGET(id),
      getAll: async () => (await api.accountGET2(1, 100, 0)).items ?? [],
      async create(input: {
        email: string;
        firstName: string;
        lastName: string;
        password: string;
      }) {
        try {
          return await api.accountPOST(new AccountCreateDto(input));
        } catch (error) {
          if (isCreatedResponse(error)) return undefined;
          throw error;
        }
      },
      update: (x: any) => api.accountPUT(new AccountUpdateDto(x)),
      remove: (id: string) => api.accountDELETE(new AccountDeleteDto({ id })),
      activate: (id: string) => api.activate(id),
      deactivate: (id: string) => api.deactivate(id),
      resetPassword: (id: string, x: any) => api.resetPassword(id, x),
    }),
    [api],
  );
}
