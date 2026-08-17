import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { AccessCodePage } from "@/features/access-code/pages/access-code-page";
export default function Route() {
  return (
    <AuthenticatedGuard>
      <AccessCodePage />
    </AuthenticatedGuard>
  );
}
