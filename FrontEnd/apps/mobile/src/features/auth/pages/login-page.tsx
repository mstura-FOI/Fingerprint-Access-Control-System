import { SharedLoginPage } from "@/packages/ui/login-page";
import { useAuthenticationStore } from "@/stores/authentication.store";

export function LoginPage() {
  const { login, loginWithTotp } = useAuthenticationStore();

  return (
    <SharedLoginPage
      accentColor="#047857"
      description="Unesite svoje podatke za nastavak."
      onLogin={login}
      onVerifyTotp={loginWithTotp}
      title="Prijava"
    />
  );
}

