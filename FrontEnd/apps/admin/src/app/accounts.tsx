import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { AccountsPage } from "@/features/accounts/pages/accounts-page";
export default function Route() {
  return (
    <AuthenticatedGuard>
      <AccountsPage />
    </AuthenticatedGuard>
  );
}
