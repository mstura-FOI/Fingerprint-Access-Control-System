import { Redirect } from "expo-router";
import { View } from "react-native";
import { LoginPage } from "@/features/auth/pages/login-page";
import { EmptyLayout } from "@/layouts/empty-layout";
import { useAuthenticationStore } from "@/stores/authentication.store";
import { mobileRoutes } from "@/router/mobile-routes";

export default function LoginRoute() {
  const { isAuthenticated, isReady } = useAuthenticationStore();
  if (!isReady) return <View style={{ flex: 1 }} />;
  if (isAuthenticated) return <Redirect href={mobileRoutes[0].href} />;
  return (
    <EmptyLayout>
      <LoginPage />
    </EmptyLayout>
  );
}
