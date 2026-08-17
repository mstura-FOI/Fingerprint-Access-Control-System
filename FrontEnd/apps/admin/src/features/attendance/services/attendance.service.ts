import { useMemo } from "react";
import { AttendanceExitDto, useAttendanceApiService } from "@/packages";

export function useAttendanceService() {
  const api = useAttendanceApiService();

  return useMemo(
    () => ({
      getList: (page: number, pageSize: number) =>
        api.attendance2(page, pageSize, (page - 1) * pageSize),
      async getAll() {
        const pageSize = 100;
        const firstPage = await api.attendance2(1, pageSize, 0);
        const items = [...(firstPage.items ?? [])];
        const totalPages = firstPage.totalPages ?? 1;

        for (let page = 2; page <= totalPages; page += 1) {
          const result = await api.attendance2(
            page,
            pageSize,
            (page - 1) * pageSize,
          );
          items.push(...(result.items ?? []));
        }

        return items;
      },
      getById: (id: string) => api.attendance(id),
      checkout: (applicationUserId: string, roomId: string) =>
        api.checkOut(new AttendanceExitDto({ applicationUserId, roomId })),
    }),
    [api],
  );
}
