using UnityEngine;

// Bir kararın insanlar üzerindeki etkisi: "Vesperia Hollow: 120 kişi temiz suya ulaştı." gibi.
// Cümleleri buradan değiştirebilirsiniz: {0} = bölge / yol adı, {1} = kişi sayısı.
public struct ImpactResult
{
    public bool good;      // İnsanlara yardım etti mi, zarar mı verdi?
    public int people;
    public string text;
}

public static class Impact
{
    // ---------- Kişi sayısı aralıkları (en az, en çok) ----------
    static readonly Vector2Int serviceRange   = new Vector2Int(60, 300);
    static readonly Vector2Int rescueRange    = new Vector2Int(3, 14);
    static readonly Vector2Int fireRange      = new Vector2Int(8, 40);
    static readonly Vector2Int roadRange      = new Vector2Int(10, 60);
    static readonly Vector2Int knowledgeRange = new Vector2Int(200, 900);

    // shared: oyuncu paylaştı mı (zaman dolup haber kendiliğinden yayıldıysa da true)
    // isTrue: haber gerçekten doğru muydu?
    public static ImpactResult Evaluate(NewsItem n, bool shared, bool isTrue)
    {
        bool good = shared == isTrue;
        string place = n.place ?? "";

        switch (n.kind)
        {
            case NewsKind.Service:   return Make(good, place, serviceRange, ServiceText(n.service, shared, isTrue));
            case NewsKind.Team:      return Make(good, place, n.team == TeamType.Rescue ? rescueRange : fireRange, TeamText(n.team, shared, isTrue));
            case NewsKind.Road:      return Make(good, place, roadRange, RoadText(n.claimOpen, shared, isTrue));
            default:                 return Make(good, place, knowledgeRange, KnowledgeText(shared, isTrue));
        }
    }

    // ---------- Dağıtım haberleri (su / yemek / çadır) ----------
    static string ServiceText(ServiceType s, bool shared, bool isTrue)
    {
        if (shared && isTrue)
        {
            switch (s)
            {
                case ServiceType.Water: return "{0}: {1} kişi temiz suya ulaştı.";
                case ServiceType.Food:  return "{0}: {1} kişiye sıcak yemek ulaştı.";
                default:                return "{0}: {1} kişi geceyi çadırda, güvende geçirdi.";
            }
        }
        if (!shared && !isTrue)
            return "Yanlış haber durduruldu: {1} kişi {0} bölgesine boşuna yola çıkmadı.";
        if (shared && !isTrue)
        {
            switch (s)
            {
                case ServiceType.Water: return "{1} kişi su için {0} bölgesine gitti, ama orada su yoktu.";
                case ServiceType.Food:  return "{1} kişi yemek için {0} bölgesine gitti, eli boş döndü.";
                default:                return "{1} kişi çadır için {0} bölgesine gitti, geceyi açıkta geçirdi.";
            }
        }
        // Doğru haber silindi
        switch (s)
        {
            case ServiceType.Water: return "Doğru haber silindi: {0} bölgesindeki {1} kişi sudan habersiz kaldı.";
            case ServiceType.Food:  return "Doğru haber silindi: {0} bölgesindeki {1} kişi yemekten habersiz kaldı.";
            default:                return "Doğru haber silindi: {0} bölgesindeki {1} kişi çadır kampını bilmeden açıkta kaldı.";
        }
    }

    // ---------- Ekip haberleri (kurtarma / itfaiye) ----------
    static string TeamText(TeamType t, bool shared, bool isTrue)
    {
        bool rescue = t == TeamType.Rescue;
        if (shared && isTrue)
            return rescue
                ? "{0}: aileler ekibe yönlendirildi, enkazdan {1} kişi kurtarıldı."
                : "{0}: ihbarlar itfaiyeye ulaştı, {1} kişi yangından tahliye edildi.";
        if (!shared && !isTrue)
            return rescue
                ? "Yanlış haber durduruldu: {0} bölgesindeki aileler başka ekiplere ulaştı, {1} kişi kurtarıldı."
                : "Yanlış haber durduruldu: {0} bölgesindeki yangın ihbar edildi, {1} kişi tahliye edildi.";
        if (shared && !isTrue)
            return rescue
                ? "Herkes {0} bölgesine ekip geldi sandı; enkaz altındaki {1} kişi için yardım çağrılmadı."
                : "Herkes {0} bölgesine itfaiye geldi sandı; yangın ihbar edilmedi, {1} kişi evsiz kaldı.";
        return rescue
            ? "Doğru haber silindi: {0} bölgesindeki ekibe kimse ulaşamadı, {1} kişiye zamanında yetişilemedi."
            : "Doğru haber silindi: {0} bölgesinde {1} kişi tahliye noktasını öğrenemedi.";
    }

    // ---------- Yol haberleri ----------
    static string RoadText(bool claimOpen, bool shared, bool isTrue)
    {
        bool reallyOpen = claimOpen == isTrue;

        if (shared && isTrue)
            return claimOpen
                ? "{0} üzerinden {1} yaralı hastaneye zamanında ulaştı."
                : "Uyarı yayıldı: {1} kişi kapalı {0} yerine güvenli yoldan gitti.";
        if (!shared && !isTrue)
            return reallyOpen
                ? "Yanlış haber durduruldu: {0} açık kaldı, ambulanslar {1} yaralı taşıdı."
                : "Yanlış haber durduruldu: {1} kişi kapalı {0} yoluna girmedi.";
        if (shared && !isTrue)
            return reallyOpen
                ? "Açık olan {0} boş kaldı; {1} yaralı uzun yoldan, geç taşındı."
                : "{1} kişi kapalı {0} yolunda mahsur kaldı, ambulanslar geçemedi.";
        return claimOpen
            ? "Doğru haber silindi: {0} açık ama kimse bilmiyor, {1} yaralı geç taşındı."
            : "Doğru uyarı silindi: {1} kişi kapalı {0} yolunda mahsur kaldı.";
    }

    // ---------- Genel bilgi haberleri (dezenformasyon / resmi uyarı) ----------
    static string KnowledgeText(bool shared, bool isTrue)
    {
        if (shared && isTrue)   return "Hayati uyarı yayıldı: {1} kişi doğru bilgiye zamanında ulaştı.";
        if (!shared && !isTrue) return "Söylenti durduruldu: {1} kişi paniğe kapılmadı.";
        if (shared && !isTrue)  return "Söylenti yayıldı: {1} kişi paniğe kapıldı, hatlar ve yollar tıkandı.";
        return "Hayati uyarı silindi: {1} kişi bu bilgiye ulaşamadı.";
    }

    // ---------- Yardımcılar ----------
    static ImpactResult Make(bool good, string place, Vector2Int range, string template)
    {
        int people = RoundNice(Random.Range(range.x, range.y + 1));
        return new ImpactResult
        {
            good = good,
            people = people,
            text = string.Format(template, place, FormatNumber(people))
        };
    }

    // Büyük sayılar yuvarlak görünsün (137 yerine 140)
    static int RoundNice(int n) => n >= 50 ? Mathf.RoundToInt(n / 10f) * 10 : n;

    // 1250 → "1.250" (Türkçe binlik ayırıcı)
    public static string FormatNumber(int n) =>
        n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.');
}
