import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { ShiftsPage } from "@/features/shifts/pages/shifts-page";
export default function ShiftsRoute() {
  return (
    <AuthenticatedGuard>
      <ShiftsPage />
    </AuthenticatedGuard>
  );
}
