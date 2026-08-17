import { Redirect } from "expo-router";
import type { PropsWithChildren } from "react";
import { ActivityIndicator, StyleSheet, View } from "react-native";
import { useAuthenticationStore } from "@/stores/authentication.store";

export function AuthenticatedGuard({ children }: PropsWithChildren) {
  const { isAuthenticated, isReady } = useAuthenticationStore();

  if (!isReady) {
    return (
      <View style={styles.loading}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  return isAuthenticated ? <>{children}</> : <Redirect href="/login" />;
}

const styles = StyleSheet.create({
  loading: { flex: 1, alignItems: "center", justifyContent: "center" },
});
