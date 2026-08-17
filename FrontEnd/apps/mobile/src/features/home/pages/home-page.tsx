import { Pressable, StyleSheet, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { router } from "expo-router";
import { useAuthenticationStore } from "@/stores/authentication.store";
import { mobileRoutes } from "@/router/mobile-routes";
export function HomePage() {
  const { logout } = useAuthenticationStore();
  return (
    <SafeAreaView style={s.page}>
      <View style={s.card}>
        <Text style={s.title}>Dobrodošli</Text>
        <Text style={s.text}>
          Upravljajte svojim pristupom na jednom mjestu.
        </Text>
        <Pressable
          style={s.primary}
          onPress={() => router.push(mobileRoutes[1].href)}
        >
          <Text style={s.primaryText}>Generiraj pristupni kod</Text>
        </Pressable>
        <Pressable
          style={s.secondary}
          onPress={() => router.push(mobileRoutes[2].href)}
        >
          <Text style={s.secondaryText}>Moj profil</Text>
        </Pressable>
        <Pressable style={s.logout} onPress={() => void logout()}>
          <Text style={s.logoutText}>Odjava</Text>
        </Pressable>
      </View>
    </SafeAreaView>
  );
}
const s = StyleSheet.create({
  page: {
    flex: 1,
    justifyContent: "center",
    padding: 24,
    backgroundColor: "#f5f7fb",
  },
  card: { gap: 14, padding: 28, borderRadius: 20, backgroundColor: "#fff" },
  title: { fontSize: 30, fontWeight: "800", color: "#172033" },
  text: { color: "#5e6b82", marginBottom: 8 },
  primary: {
    alignItems: "center",
    borderRadius: 10,
    backgroundColor: "#047857",
    padding: 14,
  },
  primaryText: { color: "#fff", fontWeight: "700" },
  secondary: {
    alignItems: "center",
    borderRadius: 10,
    borderWidth: 1,
    borderColor: "#047857",
    padding: 14,
  },
  secondaryText: { color: "#047857", fontWeight: "700" },
  logout: { alignItems: "center", padding: 12 },
  logoutText: { color: "#b91c1c", fontWeight: "700" },
});
