import { Redirect } from "expo-router";
import { View } from "react-native";
import { useAuthenticationStore } from "@/stores/authentication.store";
import { mobileRoutes } from "@/router/mobile-routes";

export default function IndexRoute() {
  const { isAuthenticated, isReady } = useAuthenticationStore();
  if (!isReady) return <View style={{ flex: 1 }} />;
  return <Redirect href={isAuthenticated ? mobileRoutes[0].href : "/login"} />;
}
