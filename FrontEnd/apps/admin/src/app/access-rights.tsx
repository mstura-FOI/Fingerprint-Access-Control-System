import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { AccessRightsPage } from "@/features/access-rights/pages/access-rights-page";
export default function Route() {
  return (
    <AuthenticatedGuard>
      <AccessRightsPage />
    </AuthenticatedGuard>
  );
}
