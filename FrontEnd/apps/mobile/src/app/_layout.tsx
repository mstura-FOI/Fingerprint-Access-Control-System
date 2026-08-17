import { Stack } from "expo-router";
import { AuthenticationProvider } from "@/stores/authentication.store";

export default function RootLayout() {
  return (
    <AuthenticationProvider>
      <Stack screenOptions={{ headerShown: false }} />
    </AuthenticationProvider>
  );
}
