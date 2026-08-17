import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { SecurityPage } from "@/features/security/pages/security-page";
export default function SecurityRoute() {
  return (
    <AuthenticatedGuard>
      <SecurityPage />
    </AuthenticatedGuard>
  );
}
