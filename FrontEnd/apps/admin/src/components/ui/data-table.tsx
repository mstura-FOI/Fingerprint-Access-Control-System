import type { ReactNode } from "react";
import { Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

export type SortDirection = "asc" | "desc";

type Props = {
  headers: string[];
  rows: ReactNode[][];
  sortColumn?: number;
  sortDirection?: SortDirection;
  sortableColumns?: number[];
  onSort?: (column: number) => void;
};

export function DataTable({
  headers,
  rows,
  sortColumn,
  sortDirection,
  sortableColumns = [],
  onSort,
}: Props) {
  const sortable = new Set(sortableColumns);

  return (
    <ScrollView
      horizontal
      style={s.scroll}
      contentContainerStyle={s.scrollContent}
    >
      <View>
        <View style={[s.row, s.head]}>
          {headers.map((header, index) => {
            const canSort = sortable.has(index);
            const direction =
              sortColumn === index
                ? sortDirection === "asc"
                  ? " od"
                  : " do"
                : "";
            return canSort ? (
              <Pressable
                key={header}
                style={s.cell}
                onPress={() => onSort?.(index)}
              >
                <Text style={[s.headerText, s.sortable]}>
                  {header}
                  {direction}
                </Text>
              </Pressable>
            ) : (
              <Text key={header} style={[s.cell, s.headerText]}>
                {header}
              </Text>
            );
          })}
        </View>
        {rows.map((row, rowIndex) => (
          <View key={String(rowIndex)} style={s.row}>
            {row.map((value, columnIndex) =>
              typeof value === "string" || typeof value === "number" ? (
                <Text key={String(columnIndex)} style={s.cell}>
                  {value}
                </Text>
              ) : (
                <View key={String(columnIndex)} style={s.cell}>
                  {value}
                </View>
              ),
            )}
          </View>
        ))}
      </View>
    </ScrollView>
  );
}

const s = StyleSheet.create({
  scroll: { width: "100%" },
  scrollContent: { flexGrow: 1, justifyContent: "center" },
  row: {
    flexDirection: "row",
    minWidth: 800,
    borderBottomWidth: 1,
    borderColor: "#e2e8f0",
  },
  head: { backgroundColor: "#dbeafe" },
  cell: { width: 160, padding: 12, color: "#1e293b" },
  headerText: { color: "#102a43", fontWeight: "700" },
  sortable: { textDecorationLine: "underline" },
});
