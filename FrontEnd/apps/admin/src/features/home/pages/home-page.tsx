import { Pressable, StyleSheet, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useAuthenticationStore } from "@/stores/authentication.store";

export function HomePage() {
  const { logout } = useAuthenticationStore();
  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.card}>
        <Text style={styles.title}>Administracija</Text>
        <Text style={styles.description}>Uspješno ste prijavljeni.</Text>
        <Pressable style={styles.button} onPress={logout}>
          <Text style={styles.buttonText}>Odjava</Text>
        </Pressable>
      </View>
    </SafeAreaView>
  );
}
const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: "center",
    padding: 24,
    backgroundColor: "#f5f7fb",
  },
  card: {
    alignSelf: "center",
    width: "100%",
    maxWidth: 440,
    gap: 14,
    padding: 28,
    borderRadius: 20,
    backgroundColor: "#fff",
  },
  title: { color: "#172033", fontSize: 30, fontWeight: "700" },
  description: { color: "#5e6b82", fontSize: 16 },
  button: {
    alignItems: "center",
    borderRadius: 10,
    backgroundColor: "#1d4ed8",
    paddingVertical: 14,
  },
  buttonText: { color: "#fff", fontWeight: "700" },
});
