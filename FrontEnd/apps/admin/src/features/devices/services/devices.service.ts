import { useMemo } from "react";
import { DeviceCreateDto, useDeviceApiService } from "@/packages";
export function useDevicesService() {
  const api = useDeviceApiService();
  return useMemo(
    () => ({
      getList: (page: number, pageSize: number) =>
        api.deviceGET(page, pageSize, (page - 1) * pageSize),
      getById: (id: string) => api.deviceGET2(id),
      create: (x: any) => api.devicePOST(new DeviceCreateDto(x)),
    }),
    [api],
  );
}
