using System.Linq;
using System.Text;
using TMPro;

// Görevlendirme raporu: her ekip ve çalıştığı bölgeler.
//
// KURTARMA EKİBİ 1 ..... Vesperia Hollow, Solis Ridge
// KURTARMA EKİBİ 2 ..... Ironroot March
// İTFAİYE EKİBİ 1 ...... Duskwater Fen
// İTFAİYE EKİBİ 2 ...... Aethelgard Reach, Frostmere Expanse
public class ReportWindow : DeskWindow
{
    public TMP_Text reportText;
    public string title = "GÖREVLENDİRME RAPORU";

    public override void Setup(WorldState world)
    {
        var sb = new StringBuilder();
        sb.AppendLine(title);
        sb.AppendLine();

        foreach (var team in world.teams.OrderBy(t => t.type).ThenBy(t => t.number))
        {
            string regions = string.Join(", ", team.regions.Select(r => GameNames.Of(r)));
            sb.AppendLine($"{team.DisplayName.ToUpperInvariant()}");
            sb.AppendLine($"  > {regions}");
            sb.AppendLine();
        }

        reportText.text = sb.ToString();
    }
}
