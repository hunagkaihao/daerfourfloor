using FourFloor.Consolidation.Models.Planning;

namespace FourFloor.Consolidation.Planning;

public sealed class PalletGroupBuilder
{
    public IReadOnlyDictionary<string, IReadOnlyList<PalletSnapshot>> Build(WarehouseSnapshot snapshot) =>
        snapshot.Pallets.Values
            .Where(pallet => !string.IsNullOrWhiteSpace(pallet.GroupBarcode))
            .GroupBy(pallet => pallet.GroupBarcode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<PalletSnapshot>)group.ToList(),
                StringComparer.OrdinalIgnoreCase);
}
