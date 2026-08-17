import { Pressable, StyleSheet, Text, View } from "react-native";
import { router } from "expo-router";
import type { Href } from "expo-router";
import { adminRoutes } from "@/router/admin-routes";
import { useAuthenticationStore } from "@/stores/authentication.store";
export function AdminSidebar() {
  const { logout } = useAuthenticationStore();
  return (
    <View style={s.sidebar}>
      <Text style={s.brand}>FP ACCESS</Text>
      {adminRoutes.map((route) => (
        <Pressable
          key={route.href}
          onPress={() => router.replace(route.href as Href)}
          style={s.link}
        >
          <Text style={s.text}>{route.label}</Text>
        </Pressable>
      ))}
      <Pressable onPress={logout} style={s.logout}>
        <Text style={s.logoutText}>Odjava</Text>
      </Pressable>
    </View>
  );
}
const s = StyleSheet.create({
  sidebar: { width: 210, padding: 18, backgroundColor: "#102a43" },
  brand: { marginBottom: 25, color: "#fff", fontSize: 20, fontWeight: "800" },
  link: { padding: 12, borderRadius: 8 },
  text: { color: "#dbeafe", fontWeight: "600" },
  logout: { marginTop: 28, padding: 12 },
  logoutText: { color: "#fda4af" },
});
