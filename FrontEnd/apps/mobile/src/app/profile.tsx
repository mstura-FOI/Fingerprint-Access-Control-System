import { AuthenticatedGuard } from "@/guards/authenticated.guard";
import { ProfilePage } from "@/features/profile/pages/profile-page";
export default function Route() {
  return (
    <AuthenticatedGuard>
      <ProfilePage />
    </AuthenticatedGuard>
  );
}
