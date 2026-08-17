import { useMemo } from "react";
import { useRoomApiService } from "@/packages";
export function useRoomsService() {
  const api = useRoomApiService();
  return useMemo(
    () => ({
      getList: (page: number, pageSize: number) =>
        api.room2(page, pageSize, (page - 1) * pageSize),
      getAll: async () => (await api.room2(1, 100, 0)).items ?? [],
      getById: (id: string) => api.room(id),
    }),
    [api],
  );
}
