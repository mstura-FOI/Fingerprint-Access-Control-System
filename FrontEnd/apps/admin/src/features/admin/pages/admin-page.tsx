import { ScrollView, StyleSheet, Text, Pressable, View } from "react-native";
import { router } from "expo-router";
import type { Href } from "expo-router";
import { MainLayout } from "@/layouts/main-layout";
import { adminRoutes } from "@/router/admin-routes";
export function AdminPage() {
  return (
    <MainLayout>
      <ScrollView contentContainerStyle={s.page}>
        <Text style={s.title}>Nadzorna ploča</Text>
        <Text style={s.subtitle}>Odaberite administrativnu funkciju.</Text>
        <View style={s.grid}>
          {adminRoutes.slice(1).map((route) => (
            <Pressable
              key={route.href}
              onPress={() => router.push(route.href as Href)}
              style={s.card}
            >
              <Text style={s.cardTitle}>{route.label}</Text>
              <Text style={s.cardText}>Otvori pregled</Text>
            </Pressable>
          ))}
        </View>
      </ScrollView>
    </MainLayout>
  );
}
const s = StyleSheet.create({
  page: { padding: 30, gap: 12 },
  title: { fontSize: 30, fontWeight: "800", color: "#102a43" },
  subtitle: { color: "#64748b" },
  grid: { flexDirection: "row", flexWrap: "wrap", gap: 16, marginTop: 18 },
  card: { width: 220, padding: 22, borderRadius: 14, backgroundColor: "#fff" },
  cardTitle: { fontSize: 18, fontWeight: "700", color: "#102a43" },
  cardText: { marginTop: 8, color: "#2563eb" },
});
