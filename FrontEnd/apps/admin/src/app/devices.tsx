import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { DevicesPage } from "@/features/devices/pages/devices-page";
export default function Route() {
  return (
    <AuthenticatedGuard>
      <DevicesPage />
    </AuthenticatedGuard>
  );
}
