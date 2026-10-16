using System;
using System.Linq;
using Random = UnityEngine.Random;

// Odaya gelen kişinin sorusu. Doğru cevap masadaki bilgilerle bulunabilir.
public class VisitorRequest
{
    public string question;
    public EvidenceSource source;          // Cevap hangi cihazdan bulunur
    public Func<bool> yesIsCorrect;        // Dünyanın şu anki haline göre "Evet" doğru mu?
    public Func<bool, string> explain;     // (cevap Evet mi) -> açıklama
    public Action onYesCorrect;            // Doğru "Evet"in dünyaya etkisi (örn. bölgeye su dağıtımı eklenir)
    public string badLuckMessage;          // Doluysa, doğru "Evet"te küçük bir ihtimalle aksilik olur
}

// İstek şablonları. Yeni soru türü eklemek için buraya bir fonksiyon daha yazmanız yeterli.
public static class VisitorRequestGenerator
{
    public static VisitorRequest Create(WorldState w, bool wantYes)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            VisitorRequest r;
            switch (Random.Range(0, 3))
            {
                case 0:  r = Medicine(w, wantYes); break;
                case 1:  r = Team(w, wantYes); break;
                default: r = Supply(w, wantYes); break;
            }
            if (r != null) return r;
        }
        return null;
    }

    // "İlaçları X yolundan gönderelim mi?" -> Laptop: yol açıksa Evet
    static VisitorRequest Medicine(WorldState w, bool wantYes)
    {
        var roads = WorldState.AllRoads.Where(r => w.roadOpen[r] == wantYes).ToList();
        if (roads.Count == 0) return null;

        var road = Utils.Pick(roads);
        string roadName = GameNames.Of(road);
        string regionName = GameNames.Of(Utils.Pick(WorldState.AllRegions));

        return new VisitorRequest
        {
            question = $"{regionName} bölgesindeki sahra hastanesine ilaç lazım. İlaçları {roadName} üzerinden gönderelim mi?",
            source = EvidenceSource.Laptop,
            yesIsCorrect = () => w.roadOpen[road],
            explain = yes => w.roadOpen[road]
                ? (yes ? $"{roadName} açık, ilaçlar yola çıktı." : $"{roadName} açıktı, ilaçlar gönderilebilirdi.")
                : (yes ? $"{roadName} kapalıydı, ilaçlar yolda kaldı." : $"Doğru, {roadName} kapalı."),
            badLuckMessage = "İlaçlar yolda yağmalandı! Her şey planlandığı gibi gitmeyebilir."
        };
    }

    // "X bölgesine ekip yönlendirelim mi?" -> Rapor: orada bu tür ekip yoksa Evet
    static VisitorRequest Team(WorldState w, bool wantYes)
    {
        var type = Random.value < 0.5f ? TeamType.Rescue : TeamType.Fire;
        var regions = w.RegionsWithTeam(type, !wantYes);
        if (regions.Count == 0) return null;

        var region = Utils.Pick(regions);
        string regionName = GameNames.Of(region);
        string teamName = GameNames.Of(type);

        string question = type == TeamType.Rescue
            ? $"{regionName} bölgesinde enkaz altında insanlar var! Kurtarma ekibi yönlendirelim mi?"
            : $"{regionName} bölgesinde yangın çıktı! İtfaiye ekibi yönlendirelim mi?";

        return new VisitorRequest
        {
            question = question,
            source = EvidenceSource.Report,
            yesIsCorrect = () => !w.HasTeam(region, type),
            explain = yes => w.HasTeam(region, type)
                ? (yes ? $"Raporda {regionName} bölgesinde zaten {teamName} var, ekip boşa gönderildi."
                       : $"Doğru, {regionName} bölgesinde zaten {teamName} çalışıyor.")
                : (yes ? $"{teamName} {regionName} bölgesine yönlendirildi."
                       : $"{regionName} bölgesinde {teamName} yoktu, yardım gecikti."),
            onYesCorrect = () => w.AddTeamToRegion(type, region)
        };
    }

    // "X bölgesine su/yemek/çadır gönderelim mi?" -> Harita: orada yoksa Evet
    static VisitorRequest Supply(WorldState w, bool wantYes)
    {
        var service = Utils.Pick(WorldState.AllServices);
        var regions = w.RegionsWithService(service, !wantYes);
        if (regions.Count == 0) return null;

        var region = Utils.Pick(regions);
        string regionName = GameNames.Of(region);
        string serviceName = GameNames.Of(service);

        string question;
        switch (service)
        {
            case ServiceType.Water: question = $"{regionName} bölgesinde halk susuz kaldı. Su tankeri gönderelim mi?"; break;
            case ServiceType.Food:  question = $"{regionName} bölgesinde insanlar aç. Yemek gönderelim mi?"; break;
            default:                question = $"{regionName} bölgesinde insanlar sokakta kalıyor. Çadır gönderelim mi?"; break;
        }

        return new VisitorRequest
        {
            question = question,
            source = EvidenceSource.Map,
            yesIsCorrect = () => !w.HasService(region, service),
            explain = yes => w.HasService(region, service)
                ? (yes ? $"Haritada {regionName} bölgesinde zaten {serviceName} var, kaynak boşa harcandı."
                       : $"Doğru, {regionName} bölgesinde zaten {serviceName} var.")
                : (yes ? $"{regionName} bölgesinde {serviceName} başladı."
                       : $"{regionName} bölgesinde {serviceName} yoktu, halk yardımsız kaldı."),
            onYesCorrect = () => w.SetService(region, service, true)
        };
    }
}
