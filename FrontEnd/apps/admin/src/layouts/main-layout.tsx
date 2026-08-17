import type { PropsWithChildren } from "react";
import { StyleSheet, View } from "react-native";
import { AdminSidebar } from "@/components/admin/admin-sidebar";
export function MainLayout({ children }: PropsWithChildren) {
  return (
    <View style={s.root}>
      <AdminSidebar />
      <View style={s.content}>{children}</View>
    </View>
  );
}
const s = StyleSheet.create({
  root: { flex: 1, flexDirection: "row", backgroundColor: "#f8fafc" },
  content: { flex: 1 },
});
