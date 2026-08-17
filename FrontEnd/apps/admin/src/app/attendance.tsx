import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { AttendancePage } from "@/features/attendance/pages/attendance-page";
export default function Route() {
  return (
    <AuthenticatedGuard>
      <AttendancePage />
    </AuthenticatedGuard>
  );
}
