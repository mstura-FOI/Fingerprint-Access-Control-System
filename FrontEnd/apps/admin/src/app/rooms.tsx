import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { RoomsPage } from "@/features/rooms/pages/rooms-page";
export default function Route() {
  return (
    <AuthenticatedGuard>
      <RoomsPage />
    </AuthenticatedGuard>
  );
}
