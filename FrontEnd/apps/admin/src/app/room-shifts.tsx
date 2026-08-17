import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { RoomShiftsPage } from "@/features/room-shifts/pages/room-shifts-page";
export default function Route() {
  return (
    <AuthenticatedGuard>
      <RoomShiftsPage />
    </AuthenticatedGuard>
  );
}
