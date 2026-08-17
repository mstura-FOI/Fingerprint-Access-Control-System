import { useMemo } from "react";
import {
  ShiftCreateDto,
  ShiftDeleteDto,
  ShiftUpdateDto,
  useShiftApiService,
} from "@/packages";
import type { ShiftModel } from "../schemas/shift-model.schema";

const toApiTime = (value: string) =>
  value.length === 5 ? `${value}:00` : value;
const isCreatedResponse = (error: unknown) =>
  (error as { status?: number }).status === 201;

export function useShiftsService() {
  const api = useShiftApiService();
  return useMemo(
    () => ({
      getList: (page: number, pageSize: number) =>
        api.shiftGET(page, pageSize, (page - 1) * pageSize),
      getById: (id: string) => api.shiftGET2(id),
      getAll: async () => (await api.shiftGET(1, 100, 0)).items ?? [],
      async create(input: ShiftModel) {
        try {
          return await api.shiftPOST(
            new ShiftCreateDto({
              ...input,
              startTime: toApiTime(input.startTime),
              endTime: toApiTime(input.endTime),
            }),
          );
        } catch (error) {
          if (isCreatedResponse(error)) return undefined;
          throw error;
        }
      },
      update: (input: { id: string } & ShiftModel) =>
        api.shiftPUT(
          new ShiftUpdateDto({
            ...input,
            startTime: toApiTime(input.startTime),
            endTime: toApiTime(input.endTime),
          }),
        ),
      remove: (id: string) => api.shiftDELETE(new ShiftDeleteDto({ id })),
    }),
    [api],
  );
}
