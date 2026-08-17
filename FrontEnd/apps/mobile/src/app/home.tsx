import { HomePage } from "@/features/home/pages/home-page";
import { AuthenticatedGuard } from "@/guards/authenticated.guard";

export default function HomeRoute() {
  return (
    <AuthenticatedGuard>
      <HomePage />
    </AuthenticatedGuard>
  );
}
