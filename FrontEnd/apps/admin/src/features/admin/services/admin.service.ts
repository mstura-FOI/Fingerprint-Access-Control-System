import {
  AccountCreateDto,
  AttendanceExitDto,
  useAccountApiService,
  useAttendanceApiService,
  useDeviceApiService,
  useRoomApiService,
  useRoomShiftApiService,
} from "@/packages";
export function useAdminService() {
  const a = useAccountApiService(),
    t = useAttendanceApiService(),
    d = useDeviceApiService(),
    r = useRoomApiService(),
    s = useRoomShiftApiService();
  return {
    accounts: async () => (await a.accountGET2(1, 100, 0)).items ?? [],
    create: (x: any) => a.accountPOST(new AccountCreateDto(x)),
    toggle: (id: string, on: boolean) =>
      on ? a.activate(id) : a.deactivate(id),
    rooms: async () => (await r.room2(1, 100, 0)).items ?? [],
    shifts: async () => (await s.roomShiftGET(1, 100, 0)).items ?? [],
    devices: async () => (await d.deviceGET(1, 100, 0)).items ?? [],
    attendance: async () => (await t.attendance2(1, 100, 0)).items ?? [],
    checkout: (userId: string, roomId: string) =>
      t.checkOut(new AttendanceExitDto({ applicationUserId: userId, roomId })),
  };
}
