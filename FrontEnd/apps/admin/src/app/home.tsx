import { AdminPage } from "@/features/admin/pages/admin-page";
import { AuthenticatedGuard } from "@/guards/authenticated.guard";
export default function Home() {
  return (
    <AuthenticatedGuard>
      <AdminPage />
    </AuthenticatedGuard>
  );
}
[];
