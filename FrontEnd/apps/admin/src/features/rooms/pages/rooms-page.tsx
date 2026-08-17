import { FeatureListPage } from "@/features/shared/components/feature-list-page";
import { useRoomsService } from "@/features/rooms/services/rooms.service";
export function RoomsPage() {
  const s = useRoomsService();
  return (
    <FeatureListPage
      title="Sobe"
      headers={["Naziv", "Kreirano"]}
      load={s.getList}
      getSortValue={(x: any, column) => (column === 1 ? x.createdAt : x.name)}
      map={(x: any) => [x.name ?? "", x.createdAt?.toLocaleDateString() ?? ""]}
    />
  );
}
