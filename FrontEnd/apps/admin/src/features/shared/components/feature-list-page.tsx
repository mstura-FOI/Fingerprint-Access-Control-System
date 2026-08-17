import { useEffect, useState, type ReactNode } from "react";
import {
  ActivityIndicator,
  Modal,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { DataTable } from "@/components/ui/data-table";
import {
  ApiErrorNotice,
  getApiErrorMessage,
} from "@/features/shared/components/api-error-notice";
import { MainLayout } from "@/layouts/main-layout";

type PagedResult<T> = {
  items?: T[];
  page?: number;
  totalCount?: number;
  totalPages?: number;
};
type Props<T> = {
  title: string;
  headers: string[];
  load: (page: number, pageSize: number) => Promise<PagedResult<T>>;
  map: (item: T) => ReactNode[];
  remove?: (item: T) => Promise<unknown>;
  getItemName?: (item: T) => string;
  topContent?: ReactNode | ((refresh: () => void) => ReactNode);
  actions?: (item: T, refresh: () => void) => ReactNode;
  getSortValue?: (
    item: T,
    column: number,
  ) => string | number | Date | undefined;
  refreshKey?: number;
};
const pageSize = 10;

export function FeatureListPage<T>({
  title,
  headers,
  load,
  map,
  remove,
  getItemName,
  topContent,
  actions,
  getSortValue,
  refreshKey,
}: Props<T>) {
  const [items, setItems] = useState<T[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [error, setError] = useState<string>();
  const [pendingDelete, setPendingDelete] = useState<T>();
  const [deleting, setDeleting] = useState(false);
  const [sortColumn, setSortColumn] = useState<number>();
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  async function fetchPage(targetPage: number) {
    setLoading(true);
    setError(undefined);
    try {
      const result = await load(targetPage, pageSize);
      setItems(result.items ?? []);
      setPage(result.page ?? targetPage);
      setTotalPages(Math.max(1, result.totalPages ?? 1));
      setTotalCount(result.totalCount ?? 0);
    } catch (error) {
      setError(getApiErrorMessage(error));
    } finally {
      setLoading(false);
    }
  }
  useEffect(() => {
    void fetchPage(1);
  }, [refreshKey]);
  async function confirmDelete() {
    if (!remove || !pendingDelete) return;
    setDeleting(true);
    try {
      await remove(pendingDelete);
      setPendingDelete(undefined);
      await fetchPage(items.length === 1 && page > 1 ? page - 1 : page);
    } catch (error) {
      setPendingDelete(undefined);
      setError(getApiErrorMessage(error));
    } finally {
      setDeleting(false);
    }
  }
  function handleSort(column: number) {
    if (sortColumn === column) {
      setSortDirection((direction) => (direction === "asc" ? "desc" : "asc"));
    } else {
      setSortColumn(column);
      setSortDirection("asc");
    }
  }
  const sortedItems = [...items].sort((left, right) => {
    if (sortColumn === undefined) return 0;
    const value = (item: T) =>
      getSortValue?.(item, sortColumn) ?? map(item)[sortColumn];
    const first = value(left);
    const second = value(right);
    const comparable = (item: unknown) =>
      item instanceof Date ? item.getTime() : item;
    const leftValue = comparable(first);
    const rightValue = comparable(second);
    const result =
      typeof leftValue === "number" && typeof rightValue === "number"
        ? leftValue - rightValue
        : String(leftValue ?? "").localeCompare(String(rightValue ?? ""), "hr");
    return sortDirection === "asc" ? result : -result;
  });
  const hasActions = Boolean(remove || actions);
  const displayedHeaders = hasActions ? [...headers, "Akcije"] : headers;
  const displayedRows = sortedItems.map((item) => {
    const row = map(item);
    if (hasActions)
      row.push(
        <View key="actions" style={styles.rowActions}>
          {actions?.(item, () => void fetchPage(1))}
          {remove && (
            <Pressable
              style={styles.deleteButton}
              onPress={() => setPendingDelete(item)}
            >
              <Text style={styles.deleteText}>Obrisi</Text>
            </Pressable>
          )}
        </View>,
      );
    return row;
  });
  const content =
    typeof topContent === "function"
      ? topContent(() => void fetchPage(1))
      : topContent;
  return (
    <MainLayout>
      <ScrollView contentContainerStyle={styles.page}>
        <View style={styles.header}>
          <Text style={styles.title}>{title}</Text>
          <Text style={styles.summary}>Ukupno: {totalCount}</Text>
        </View>
        {content}
        <ApiErrorNotice message={error} onDismiss={() => setError(undefined)} />
        {loading ? (
          <ActivityIndicator />
        ) : (
          <DataTable
            headers={displayedHeaders}
            rows={displayedRows}
            sortColumn={sortColumn}
            sortDirection={sortDirection}
            sortableColumns={headers.map((_, index) => index)}
            onSort={handleSort}
          />
        )}
        <View style={styles.pager}>
          <Pressable
            disabled={loading || page <= 1}
            style={[
              styles.pagerButton,
              (loading || page <= 1) && styles.disabled,
            ]}
            onPress={() => void fetchPage(page - 1)}
          >
            <Text style={styles.pagerText}>Prethodna</Text>
          </Pressable>
          <Text style={styles.pageInfo}>
            Stranica {page} od {totalPages}
          </Text>
          <Pressable
            disabled={loading || page >= totalPages}
            style={[
              styles.pagerButton,
              (loading || page >= totalPages) && styles.disabled,
            ]}
            onPress={() => void fetchPage(page + 1)}
          >
            <Text style={styles.pagerText}>Sljedeca</Text>
          </Pressable>
        </View>
      </ScrollView>
      <Modal
        transparent
        visible={Boolean(pendingDelete)}
        animationType="fade"
        onRequestClose={() => !deleting && setPendingDelete(undefined)}
      >
        <View style={styles.overlay}>
          <View style={styles.dialog}>
            <Text style={styles.dialogTitle}>Obrisis?</Text>
            <Text style={styles.dialogText}>
              Zapis
              {pendingDelete && getItemName
                ? ` "${getItemName(pendingDelete)}"`
                : ""}{" "}
              bit ce trajno obrisan.
            </Text>
            <View style={styles.dialogActions}>
              <Pressable
                disabled={deleting}
                style={styles.cancelButton}
                onPress={() => setPendingDelete(undefined)}
              >
                <Text style={styles.cancelText}>Odustani</Text>
              </Pressable>
              <Pressable
                disabled={deleting}
                style={[styles.confirmButton, deleting && styles.disabled]}
                onPress={() => void confirmDelete()}
              >
                <Text style={styles.confirmText}>
                  {deleting ? "Brisanje..." : "Obrisi"}
                </Text>
              </Pressable>
            </View>
          </View>
        </View>
      </Modal>
    </MainLayout>
  );
}

const styles = StyleSheet.create({
  page: { padding: 28, gap: 18 },
  header: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
  },
  title: { fontSize: 30, fontWeight: "800", color: "#102a43" },
  summary: { color: "#486581" },
  pager: {
    flexDirection: "row",
    gap: 12,
    alignItems: "center",
    justifyContent: "center",
  },
  pagerButton: {
    borderWidth: 1,
    borderColor: "#2563eb",
    paddingHorizontal: 12,
    paddingVertical: 9,
    borderRadius: 8,
  },
  pagerText: { color: "#1d4ed8", fontWeight: "700" },
  pageInfo: { color: "#334e68" },
  disabled: { opacity: 0.45 },
  rowActions: { flexDirection: "row", alignItems: "center", gap: 8 },
  deleteButton: {
    alignSelf: "flex-start",
    borderWidth: 1,
    borderColor: "#dc2626",
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 7,
  },
  deleteText: { color: "#b91c1c", fontWeight: "700" },
  overlay: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "rgba(15, 23, 42, 0.45)",
    padding: 24,
  },
  dialog: {
    width: "100%",
    maxWidth: 420,
    backgroundColor: "#fff",
    borderRadius: 14,
    padding: 20,
    gap: 12,
  },
  dialogTitle: { fontSize: 20, fontWeight: "800", color: "#102a43" },
  dialogText: { color: "#486581" },
  dialogActions: { flexDirection: "row", justifyContent: "flex-end", gap: 10 },
  cancelButton: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#94a3b8",
  },
  cancelText: { color: "#334e68", fontWeight: "700" },
  confirmButton: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: 8,
    backgroundColor: "#dc2626",
  },
  confirmText: { color: "#fff", fontWeight: "700" },
});
