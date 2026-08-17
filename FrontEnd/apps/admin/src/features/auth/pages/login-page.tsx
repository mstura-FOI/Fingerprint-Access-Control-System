import { SharedLoginPage } from "@/packages/ui/login-page";
import { useAuthenticationStore } from "@/stores/authentication.store";

export function LoginPage() {
  const { login, loginWithTotp } = useAuthenticationStore();

  return (
    <SharedLoginPage
      accentColor="#1d4ed8"
      description="Prijavite se administratorskim računom."
      onLogin={login}
      onVerifyTotp={loginWithTotp}
      title="Admin prijava"
    />
  );
}
