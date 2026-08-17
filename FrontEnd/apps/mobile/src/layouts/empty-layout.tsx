import type { PropsWithChildren } from "react";
import { View } from "react-native";

export function EmptyLayout({ children }: PropsWithChildren) {
  return <View style={{ flex: 1 }}>{children}</View>;
}
