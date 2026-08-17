import { useMemo } from "react";
import {
  AccessRightCreateDto,
  AccessRightDeleteDto,
  AccessRightUpdateDto,
  useAccessRightApiService,
} from "@/packages";
const created = (e: unknown) => (e as { status?: number }).status === 201;
export function useAccessRightsService() {
  const api = useAccessRightApiService();
  return useMemo(
    () => ({
      getList: (page: number, pageSize: number) =>
        api.accessRightGET(page, pageSize, (page - 1) * pageSize),
      async create(input: { applicationUserId: string; roomId: string }) {
        try {
          return await api.accessRightPOST(new AccessRightCreateDto(input));
        } catch (e) {
          if (created(e)) return;
          throw e;
        }
      },
      update: (input: {
        id: string;
        applicationUserId: string;
        roomId: string;
      }) => api.accessRightPUT(new AccessRightUpdateDto(input)),
      remove: (id: string) =>
        api.accessRightDELETE(new AccessRightDeleteDto({ id })),
    }),
    [api],
  );
}
