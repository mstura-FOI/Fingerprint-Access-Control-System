import { FeatureListPage } from "@/features/shared/components/feature-list-page";
import { useDevicesService } from "@/features/devices/services/devices.service";
export function DevicesPage() {
  const s = useDevicesService();
  return (
    <FeatureListPage
      title="Uređaji"
      headers={["Naziv", "Serijski broj", "IP", "Soba"]}
      load={s.getList}
      map={(x: any) => [
        x.name ?? "",
        x.serialNumber ?? "",
        x.ipAddress ?? "",
        x.roomName ?? "",
      ]}
    />
  );
}
