import { Redirect } from "expo-router";
import { View } from "react-native";
import { useAuthenticationStore } from "@/stores/authentication.store";

export default function IndexRoute() {
  const { isAuthenticated, isReady } = useAuthenticationStore();
  if (!isReady) return <View style={{ flex: 1 }} />;
  return <Redirect href={isAuthenticated ? "/home" : "/login"} />;
}
