import { useMemo } from "react";
import {
  RoomShiftCreateDto,
  RoomShiftDeleteDto,
  RoomShiftUpdateDto,
  useRoomShiftApiService,
} from "@/packages";

const isCreatedResponse = (error: unknown) =>
  (error as { status?: number }).status === 201;

export function useRoomShiftsService() {
  const api = useRoomShiftApiService();
  return useMemo(
    () => ({
      getList: (page: number, pageSize: number) =>
        api.roomShiftGET(page, pageSize, (page - 1) * pageSize),
      getById: (id: string) => api.roomShiftGET2(id),
      getAll: async () => (await api.roomShiftGET(1, 100, 0)).items ?? [],
      async create(input: {
        roomId: string;
        shiftId: string;
        managerId: string;
      }) {
        try {
          return await api.roomShiftPOST(new RoomShiftCreateDto(input));
        } catch (error) {
          if (isCreatedResponse(error)) return undefined;
          throw error;
        }
      },
      update: (x: any) => api.roomShiftPUT(new RoomShiftUpdateDto(x)),
      remove: (id: string) =>
        api.roomShiftDELETE(new RoomShiftDeleteDto({ id })),
    }),
    [api],
  );
}
