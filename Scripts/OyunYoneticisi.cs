using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class OyunYoneticisi : MonoBehaviour, SahneBaglamaServisi.IBaglamaHedefi, IDonusAkisBaglami, IOyunUIGuncellemeBaglami, IScatterEfektBaglami, ITumbleAkisBaglami, ICokmeAkisBaglami, IIzgaraBaslatmaBaglami, IOyunBootstrapBaglami, ICarpanYerlestirmeBaglami, IZorlukBaglami, IOyunKorumaBaglami
{
    
    
// === LOG / İSTATİSTİK (otomatik kayıt) ===
    private LogServisi _logServisi;
    private EkonomiServisi _ekonomiServisi;
    private UIServisi _uiServisi;
    private SenaryoServisi _senaryoServisi;
    private DonusServisi _donusServisi;
    private IzgaraServisi _izgaraServisi;
    private TumbleServisi _tumbleServisi;
    private CarpanServisi _carpanServisi;
    private AnimasyonServisi _animasyonServisi;
    private CarpanOverlayServisi _carpanOverlayServisi;
    private KorutinServisi _korutinServisi;
    private BonusUIServisi _bonusUIServisi;
    private HizVeSesServisi _hizVeSesServisi;
    private AdminAyarUIServisi _adminAyarUIServisi;
    private SahneBaglamaServisi _sahneBaglamaServisi;
    private OdemeServisi _odemeServisi;
    private DonusAkisServisi _donusAkisServisi;
    private OyunUIGuncellemeServisi _oyunUIGuncellemeServisi;
    private ScatterEfektServisi _scatterEfektServisi;
    private TumbleAkisServisi _tumbleAkisServisi;
    private CokmeAkisServisi _cokmeAkisServisi;
    private IzgaraBaslatmaServisi _izgaraBaslatmaServisi;
    private OyunBootstrapServisi _oyunBootstrapServisi;
    private CarpanYerlestirmeServisi _carpanYerlestirmeServisi;
    private ZorlukServisi _zorlukServisi;
    private BonusAyarlari _bonusAyarlari;
    private int _spinPrevBakiye = 0;
    private int _spinBahisTL = 0;

    public TextMeshProUGUI bakiyeYukleUyariText; // Input altındaki sonuç yazısı
    public TextMeshProUGUI paraCekUyariText;     // Input altındaki sonuç yazısı

    private void CloseMoneyPanels()
    {
        if (bakiyeYuklePanel != null) bakiyeYuklePanel.SetActive(false);
        if (paraCekPanel != null) paraCekPanel.SetActive(false);
    }

    // Inspector OnClick için PUBLIC wrapper'lar (Unity sadece public metotları listeler)
    public void ParaCek_OnayButton()
    {
        _ekonomiServisi?.OnParaCekOnay();
    }

    public void ParaCek_IptalButton()
    {
        _uiServisi?.HideParaCekPanel();
    }

    public void BakiyeYukle_OnayButton()
    {
        _ekonomiServisi?.OnBakiyeYukleOnay();
    }

    public void BakiyeYukle_IptalButton()
    {
        _uiServisi?.HideBakiyeYuklePanel();
    }

    public void ShowParaCekPanel()
    {
        _uiServisi?.ResolveMoneyUIRefsIfMissing();
        _uiServisi?.WireParaCekUI();

        if (paraCekUyariText != null) paraCekUyariText.text = "";

        _uiServisi?.CloseMoneyPanels();
        Debug.Log("[UI] ParaCekButon tıklandı. Panel=" + (paraCekPanel != null ? paraCekPanel.name : "NULL"));

        if (paraCekUyariText != null) paraCekUyariText.text = "";
        if (paraCekInput != null) paraCekInput.text = "";

        if (paraCekPanel != null)
            paraCekPanel.SetActive(true);
        SenaryoYoneticisi.I?.LogEkle(SenaryoOlayKaydi.OlayTipi_ParaCekEkraniAcildi, "Para çek ekranı açıldı. Mevcut bakiye: " + (_ekonomiServisi != null ? _ekonomiServisi.Bakiye.ToString("N0") : "—") + " TL.");
    }

    public void HideParaCekPanel()
    {
        if (paraCekPanel != null)
            paraCekPanel.SetActive(false);
    }



    public void SetZorluk(float deger)
    {
        _zorlukServisi?.ZorlukUygula(deger);
    }

    void IZorlukBaglami.SetZorlukSliderDegeri(int v)
    {
        _zorlukSliderDegeri = v;
        zorlukSeviyesi = v; // Panel / MevcutAyarlarMetni bu değeri okur; gerçekten uygulanan zorluk ile senkron olsun.
    }
    void IZorlukBaglami.SetMinClusterSize(int value) => minClusterSize = value;
    void IZorlukBaglami.SetEasyBias01(float value) => _easyBias01 = value;
    void IZorlukBaglami.SetHardBias01(float value) => _hardBias01 = value;
    void IZorlukBaglami.SetScatterChanceNormal(float value) => scatterChanceNormal = value;
    void IZorlukBaglami.ZorlukUIMetinVeLogGuncelle(int v)
    {
        if (zorlukValueText != null)
            zorlukValueText.text = $"Zorluk: {v}";
        Debug.Log($"[ADMIN] Zorluk={v} | tumbleEsiği(SABİT)={minClusterSize} | easyBias={_easyBias01:0.00} | hardBias={_hardBias01:0.00} | scatterChanceNormal={scatterChanceNormal:0.000}");
    }

    int IOyunKorumaBaglami.GetMaxTumbleTur() => OyunKorumaServisi.MAX_TUMBLE_TUR;
    int IOyunKorumaBaglami.GetTumbleSabitEsik() => OyunKorumaServisi.TUMBLE_SABIT_ESIK;

public TMPro.TextMeshProUGUI zorlukValueText;

    // Inspector / AdminPanel bağlantıları için wrapper (asıl binding AdminAyarUIServisi.BindAllAndRefresh)
    public void OnZorlukSliderChanged(float value) => _adminAyarUIServisi?.ApplyZorluk(value);
    public void OnScatterSliderChanged(float value) => _adminAyarUIServisi?.ApplyScatter(value);
    public void OnCarpanOlasilikSliderChanged(float value) => _adminAyarUIServisi?.ApplyCarpanOlasilik(value);
    public void OnCarpanMaxAdetSliderChanged(float value) => _adminAyarUIServisi?.ApplyCarpanMaxAdet(value);

    public TMPro.TextMeshProUGUI carpanOlasilikValueText;
    public TMPro.TextMeshProUGUI carpanMaxAdetValueText;

    private void EnsurePayTablesInitialized()
{
    int n = (sembolSpriteListesi != null) ? sembolSpriteListesi.Count : 0;
    if (n <= 0) return;

    // TumbleAyarlari'ndaki PayTable'ı kullan (ScatterIndex tek kaynak TumbleAyarlari'da)
    if (tumbleAyarlari != null)
        tumbleAyarlari.EnsurePayTablesInitialized(n);
}
private float _lastTumblePopTime = -999f;
    private float _lastTumbleDropTime = -999f;
    /// <summary>Bakiye ≥ 50.000 TL görüldüğünde 20 spin boyunca tumble kapalı; kalan spin sayısı.</summary>
    private int _bakiye50KUstundeTumbleKapaliKalanSpin = 0;
    private int spinKazancHam = 0;   // tumble patlamalarından gelen ham toplam (bu spin)
    private int oturumKazanc = 0;    // oturum boyunca biriken toplam kazanç
    private bool _spinKazanciOturumaEklendi = false; // bonus'ta double sayma önler
    public TextMeshProUGUI oturumKazancText; // (senin OturumKazancText)

    public void SetCarpanOlasilikYuzde(float yuzde)
    {
        yuzde = Mathf.Clamp(yuzde, 0f, 100f);
        carpanUretimOlasiligi = yuzde / 100f;

        if (carpanOlasilikValueText != null)
            carpanOlasilikValueText.text = $"{Mathf.RoundToInt(yuzde)}%";

        Debug.Log($"[ADMIN] Çarpan olasılığı set edildi: %{yuzde} (0-1={carpanUretimOlasiligi})");
    }

    public void SetCarpanMaxAdet(float adet)
    {
        int v = Mathf.RoundToInt(adet);
        v = Mathf.Clamp(v, 1, 10);
        maxCarpanAdedi = v;

        if (carpanMaxAdetValueText != null)
            carpanMaxAdetValueText.text = v.ToString();

        Debug.Log($"[ADMIN] Max çarpan adedi set edildi: {maxCarpanAdedi}");
    }


    


    // ==========================
    // PARA CEK UI
    // ==========================
    [Header("PARA CEK UI")]
	[HideInInspector]
    public Button paraCekButon;               // "ParaCekButon"
	[HideInInspector]
    public GameObject paraCekPanel;           // "ParaCekPanel"
	[HideInInspector]
    public TMP_InputField paraCekInput;       // "ParaCekInput"
	[HideInInspector]
    public Button paraCekOnayButon;           // paneldeki "CEK" butonu
	[HideInInspector]
    public Button paraCekIptalButon;          // paneldeki "KAPAT" butonu
   

    // ==========================
    // BAKIYE YÜKLE UI
    // ==========================
    [Header("BAKIYE YUKLE UI")]
	    [HideInInspector] public Button bakiyeYukleButon;              // "BakiyeYukleButon"
	    [HideInInspector] public GameObject bakiyeYuklePanel;          // "BakiyeYuklePanel"
	    [HideInInspector] public TMP_InputField bakiyeYukleInput;      // "BakiyeYukleInput"
	    [HideInInspector] public Button bakiyeYukleOnayButon;          // "OnayButon"
	    [HideInInspector] public Button bakiyeYukleIptalButon;         // "IptalButon"
   

    [Header("BONUS BUDGET (Ödül Havuzu Koruma)")]
    [HideInInspector] public bool bonusBudgetAktif = true;

    [Range(0f, 1f)]
    [Tooltip("Bonus başında ödül havuzunun ne kadarını bu bonus oturumuna ayıracağız? 0.20 = %20")]
    [HideInInspector] public float bonusBudgetHavuzOran = 0.25f;

    [Tooltip("Bonus için minimum bütçe (TL). Havuz çok azsa bile en az bu kadar ayır.")]
    [HideInInspector] public int bonusBudgetMinTL = 0;

    [Tooltip("Bonus için maksimum bütçe (TL). Havuz çok doluysa bile tavan.")]
    [HideInInspector] public int bonusBudgetMaxTL = 20000;

    private int _bonusBudgetKalanTL = 0;
    private int _sonBonusSatinAlindiMaliyet = 0;
    /// <summary>Senaryo 1'de satın alınan bonus: ödenebilir tutar tavanı uygulanmasın.</summary>
    private bool _bonusSatınAlindiSenaryo1 = false;
    /// <summary>Senaryo bonusu (scatter veya satın al): havuz/ödenebilir tavanı uygulanmasın, sadece hesaplanan cap.</summary>
    private bool _senaryo1BonusAktif = false;
    /// <summary>3. yükleme sonrası ilk bonusta 2,5x tavan uygulandı mı (Aşama 5, tek seferlik).</summary>
    private bool _ucuncuYuklemeSonrasiIlkBonusUygulandi = false;
    /// <summary>Şu an oynanan bonus, senaryo 5 zirve bonusu mu (50 spin, 2.5x; bitişte animasyon tetiklenir).</summary>
    private bool _buBonusZirveBonusuMu = false;
    private int _senaryoOdenebilirKalanTL = -1;
    private int _bonusOturumOdenenToplamTL = 0;


    [Header("Senaryo 5 - Zirve bonusu (tek seferlik yüksek etki)")]
    [Tooltip("3. yükleme sonrası ilk bonus kaç spin sürsün (50 = yüksek etkili sahne).")]
    public int senaryo5_zirveBonusSpinSayisi = 50;
    [Tooltip("Zirve bonusu başladığında tetiklenir; başka script'te dinleyip animasyon/efekt başlatabilirsin.")]
    public System.Action OnZirveBonusBasladi;
    [Tooltip("Zirve bonusu bittiğinde tetiklenir (kazanç TL); yüksek kazanç ekranı/animasyon için kullan.")]
    public System.Action<int> OnZirveBonusBitti;

    [Header("Bonus Ödeme Limiti")]
    [Range(0f, 1f)]
    [Tooltip("Bonus başladığında ödül havuzunun bu oranı kadar (örn 0.10 = %10) maksimum toplam ödeme yapılır. Bu limite ulaşıldıktan sonra bonus devam edebilir ama ek ödeme yapılmaz.")]
    [HideInInspector] public float bonusMaxOdemeHavuzOrani = 0.10f;

    private long _bonusBaslangicHavuzTL = 0;
    private int _bonusMaxOdemeTL = int.MaxValue;
    private int _bonusOdenenTL = 0;

    
    private int _bonusPendingOdemeTL = 0; // Bonus boyunca havuzdan düşülecek tutarı biriktir (bonus bitince tek seferde düş)
    private int _bonusZorlaCarpanBirikenTL = 0; // Bonus içinde zorla çarpan kazançları; bakiye sadece 10 hak bitince güncellenir

    private int _otomatikSpinKalan = 0;
    private int _otomatikSpinSecilenAdet = 1; // 1 = tek spin; dropdown veya Inspector ile 10, 50, 100 seçilebilir

    [Header("Kasa Bazlı Kazan/Kaybet")]
    [HideInInspector] public bool kasaBazliDengeAktif = true;

    [Tooltip("Ödül havuzu boşken tumble neredeyse imkansız olsun")]
    [HideInInspector] public int minClusterSize_HavuzBos = 999;

    [Tooltip("Ödül havuzu çok doluyken tumble daha kolay")]
    [HideInInspector] public int minClusterSize_HavuzDolu = 6;

    [Tooltip("Ödül havuzu çok azken tumble daha zor")]
    [HideInInspector] public int minClusterSize_HavuzAz = 12;

    [Range(0f, 1f)]
    [Tooltip("Bu oranın altı 'havuz az' sayılır")]
    [HideInInspector] public float havuzAzEsik01 = 0.15f;

    [Range(0f, 1f)]
    [Tooltip("Bu oranın üstü 'havuz dolu' sayılır")]
    [HideInInspector] public float havuzDoluEsik01 = 0.70f;

    // zorluk slider'ın elle verdiği değer (4-12) halen dursun istiyorsan taban olarak kullanırız
    private int _zorlukSliderDegeri = 8;

    [Header("BONUS Otomatik Zorluk")]
	[HideInInspector] public bool bonusOtoZorlukAktif = true;

    [Tooltip("Bonus başında minClusterSize (kolaylık)")]
	[HideInInspector] public int bonusMinCluster_Easy = 6;

    [Tooltip("Budget biterken minClusterSize (zorluk)")]
	[HideInInspector] public int bonusMinCluster_Hard = 14;


    [Header("Kasa Sistemi")]
    [HideInInspector] public KasaYoneticisi kasa; // (LEGACY) Ayar/bağlantılar taşındı. İnspector kalabalığını azaltmak için gizli.

    [Header("Admin - Max Çarpan Slider UI")]
    [HideInInspector] public Slider carpanMaxAdetSlider;            // (LEGACY)
    [HideInInspector] public TextMeshProUGUI carpanMaxAdetText;     // (LEGACY)

    [Header("Zorluk Ayarı (8 = Sweet Bonanza referans)")]
    public int zorlukSeviyesi = 8;

    [Header("BONUS SATIN AL UI OBJESI")]
    [HideInInspector] public GameObject bonusSatinAlRoot; // (LEGACY)

    [Header("Tumble Kazancı UI")]
    [HideInInspector] public TextMeshProUGUI tumbleToplamText; // (LEGACY)
    private int tumbleToplamKazanc = 0;
    [Header("Admin - Çarpan Slider UI")]
    [HideInInspector] public Slider carpanOlasilikSlider;                 // (LEGACY)
    [HideInInspector] public TextMeshProUGUI carpanOlasilikText;          // (LEGACY)


    [Header("BONUS SATIN AL ONAY UI")]
    [HideInInspector] public GameObject bonusBuyConfirmPanel;
    [HideInInspector] public CanvasGroup bonusBuyConfirmCanvasGroup; // (LEGACY)
    [HideInInspector] public TMP_Text bonusBuyConfirmCostText; // (LEGACY)
    [HideInInspector] public Button bonusBuyYesButton;
    [HideInInspector] public Button bonusBuyNoButton;


    [Header("BONUS SATIN AL")]
    [HideInInspector] public Button bonusSatinAlButon;          // (LEGACY)
    [HideInInspector] public TextMeshProUGUI bonusSatinAlText;  // (LEGACY)
    [HideInInspector] public int bonusSatinAlCarpani = 100;     // (LEGACY)


    // === BAHİS +/- KONTROL ===
    [Header("Bahis Kontrol")]
    [HideInInspector] public int bahisMin = 1;
    [HideInInspector] public int bahisMax = 500;
    [HideInInspector] public int bahisAdim = 1;
    [HideInInspector] public SpinIconRotate spinIcon;   // (LEGACY)
    [HideInInspector] public Button bahisArttirButon; // (LEGACY)
    [HideInInspector] public Button bahisAzaltButon;  // (LEGACY)

    [Header("TUMBLE SES")]
    [HideInInspector] public AudioSource tumbleSfxSource;          // (LEGACY)
    [HideInInspector] public AudioClip tumblePopClip;              // (LEGACY)
    [HideInInspector] public AudioClip tumbleDropClip;             // (LEGACY)
    [HideInInspector] public float tumblePopMinInterval = 0.06f;   // (LEGACY)
    [HideInInspector] public float tumbleDropMinInterval = 0.12f;  // (LEGACY)

    [Header("BONUS END MUZIK")]
    [HideInInspector] public AudioSource bonusEndMusicAudio;   // (LEGACY)
    [Header("BONUS END SES")]
    [HideInInspector] public AudioSource bonusEndSfxSource;   // (LEGACY)
    [HideInInspector] public AudioClip bonusEndApplauseClip;  // (LEGACY)
    [Header("NORMAL OYUN MUZIK")]
    [HideInInspector] public AudioSource normalOyunMusic;   // (LEGACY)

    [Header("Grid Ayarları")]
    [HideInInspector] public int sutun = 6;
    [HideInInspector] public int satir = 5;
    [Header("SPIN DROP ANIM")]
    [HideInInspector] public float dropStartYOffset = 700f;   // (LEGACY)
    [HideInInspector] public float dropDuration = 0.25f;      // (LEGACY)
    [HideInInspector] public float dropStagger = 0.005f;      // (LEGACY)



    [Header("Semboller")]
    [HideInInspector] public List<Sprite> sembolSpriteListesi = new List<Sprite>();

    /// <summary>Scatter index tek kaynak: TumbleAyarlari.ScatterIndex. Start'ta oradan okunup _scatterIndexCache'e yazılır.</summary>
    private int _scatterIndexCache = 7;

    [Header("BONUS SCATTER EFEKT")]
    [HideInInspector] public float scatterScaleUp = 1.6f;        // (LEGACY)
    [HideInInspector] public float scatterAnimDuration = 0.6f;   // (LEGACY)
    [HideInInspector] public AudioSource bonusBellAudio;         // (LEGACY)

    [Header("Scatter Ayarları (Normal / Bonus)")]
    [HideInInspector] [Range(0f, 1f)] public float scatterChanceNormal = 0.005f;
    [HideInInspector] [Range(0f, 1f)] public float scatterChanceBonus = 0f; // Bonus oyununda scatter üretilmez
    [HideInInspector] public int scatterEsik = 4;
    public Slider scatterSliderUI;
    public TextMeshProUGUI scatterSliderText;
    public int maxScatterPerSpin = 5;

    [Header("Tumble / Eşleşme")]
    [Tooltip("En az kaç komşu aynı sembol gelirse patlasın (4-yön komşuluk).")]
    [HideInInspector] public int minClusterSize = 8;

    // Zorluk slider'ı (4-12) bunu ayarlar: 0(kolay) .. 1(zor)
    // KURAL DEĞİL; sadece "8'e tamamlayacak sembollerin" gelmesini baskılar.
    private float _easyBias01 = 0f; // zorluk 4..8 (kolaylaştırma)
    private float _hardBias01 = 0f; // zorluk 8..12 (zorlaştırma)

    // PayTable'ler TumbleAyarlari'ndan alınır

    [Header("Animasyon Hızları")]
    [HideInInspector] public float popDuration = 0.75f;
    [HideInInspector] public float fallDuration = 0.45f;
    [HideInInspector] public float betweenStepsDelay = 0.18f;
    [HideInInspector] public float spawnFromTopOffset = 240f;
    [Header("Bonus Hızları (Override)")]
    [HideInInspector] public bool bonusYavasMod = true;
    [HideInInspector] public float bonusPopDuration = 0.70f;
    [HideInInspector] public float bonusFallDuration = 0.80f;
    [HideInInspector] public float bonusBetweenStepsDelay = 0.35f;
    [HideInInspector] public float bonusSpinBeklemeOverride = 1.10f;


    [Header("Efekt (Opsiyonel)")]
    [HideInInspector] public ParticleSystem popParticlePrefab;

    [Header("Ekonomi / Bahis")]

    [Header("Bonus (Free Spin)")]
[HideInInspector] public int bonusHakBaslangic = 10;      // (LEGACY) BonusAyarlari.cs'ye tasindi.
[HideInInspector] public float bonusSpinBekleme = 0.70f;  // (LEGACY)

    [Header("Ayar Sistemleri (Yeni)")]
    [Tooltip("Sahnede Oyun_Sistemleri/TumbleAyarlari objesinde duran ayarlar. Simdilik sadece referans; mantik tasimayi sonra yapacagiz.")]
    public TumbleAyarlari tumbleAyarlari;

    [Tooltip("Sahnede Oyun_Sistemleri/CarpanAyarlari objesinde duran ayarlar. Simdilik sadece referans.")]
    public CarpanAyarlari carpanAyarlari;

    [Tooltip("Zorla çarpan (5x/10x/50x/100x) seçiliyken: tumble olsun. Kapalıyken: tumble olmasın (çarpan düşer ama kullanılmaz).")]
    public Toggle carpanAktifToggle;


    private void UygulaCarpanAyarlari()
    {
        if (carpanAyarlari == null) return;

        // CarpanAyarlari -> OyunYoneticisi (eski alanlara kopyala)
        carpanUretimiAktif = carpanAyarlari.CarpanUretimiAktif;
        carpanSadeceBonus = carpanAyarlari.CarpanSadeceBonus;
        carpanUretimOlasiligi = Mathf.Clamp01(carpanAyarlari.CarpanUretimOlasiligi);
        maxCarpanAdedi = Mathf.Max(0, carpanAyarlari.MaxCarpanAdedi);
        carpanHavuzu = Mathf.Max(0, carpanAyarlari.CarpanHavuzu);
        yuksekCarpanOrani = Mathf.Clamp01(carpanAyarlari.YuksekCarpanOrani);
        zorlaSiradakiCarpan = Mathf.Max(0, carpanAyarlari.ZorlaSiradakiCarpan);

        carpanSembolSprite = carpanAyarlari.CarpanSembolSprite;
        carpanOverlaySize = carpanAyarlari.CarpanOverlaySize;
        carpanOverlayFontSize = Mathf.Max(1, carpanAyarlari.CarpanOverlayFontSize);

        carpanYaziRengi = carpanAyarlari.CarpanYaziRengi;
        carpanYaziDisCizgiRengi = carpanAyarlari.CarpanYaziDisCizgiRengi;
        carpanYaziDisCizgiKalinlik = Mathf.Clamp01(carpanAyarlari.CarpanYaziDisCizgiKalinlik);
        carpanYaziKalin = carpanAyarlari.CarpanYaziKalin;

        carpanYaziGolge = carpanAyarlari.CarpanYaziGolge;
        carpanYaziGolgeRengi = carpanAyarlari.CarpanYaziGolgeRengi;
        carpanYaziGolgeOffset = carpanAyarlari.CarpanYaziGolgeOffset;

        carpanOverlayTextOffset = carpanAyarlari.CarpanOverlayTextOffset;
        carpanOverlayDropStartYOffset = carpanAyarlari.CarpanOverlayDropStartYOffset;
        carpanOverlayDropDuration = Mathf.Max(0f, carpanAyarlari.CarpanOverlayDropDuration);

        // Admin slider UI varsa, degerleri senkronla (slider kendi UI textini zaten Start'ta guncelliyor)
        if (carpanOlasilikSlider != null)
        {
            float yuzde = Mathf.Clamp(carpanUretimOlasiligi * 100f, carpanOlasilikSlider.minValue, carpanOlasilikSlider.maxValue);
            carpanOlasilikSlider.value = yuzde;
        }

        if (carpanMaxAdetSlider != null)
        {
            float v = Mathf.Clamp(maxCarpanAdedi, carpanMaxAdetSlider.minValue, carpanMaxAdetSlider.maxValue);
            carpanMaxAdetSlider.value = v;
        }
    }

    [Header("UI Referansları")]
    public Button cevirButon;
    public TMP_Text bakiyeText;
    public TMP_Text bahisText;
    public TMP_Text hakText;
    public TMP_Text kazancText;
    public TMP_Text carpanText;
    [Header("Bonus Uyarı UI")]
    public GameObject bonusStartPanel;
    [Header("Bonus Bitiş UI")]
    public GameObject bonusEndPanel;
    public CanvasGroup bonusEndCanvasGroup;
    public TMP_Text bonusEndTitleTMP;
    public TMP_Text bonusEndWinTMP;
    public Button bonusEndCloseButton;

    [Header("Otomatik Spin")]
    [Tooltip("Tıklanınca panel açar; spin dönerken 'DURDUR' yazar ve tıklanınca durdurur.")]
    public Button otomatikSpinButton;
    [Tooltip("Panel: içinde Dropdown + Baslat + Iptal. Options: 20, 50, 100, 250.")]
    public GameObject otomatikSpinPanel;
    public TMP_Dropdown otomatikSpinDropdown;
    public Button otomatikSpinBaslatButon;
    public Button otomatikSpinIptalButon;
    [Tooltip("Otomatik spin dönerken 'Kalan Spin: x' gösterilir; bitince gizlenir.")]
    public TMP_Text otomatikSpinKalanText;
    [Tooltip("OtomatikSpinButton'da spin dönmezken görünecek metin.")]
    public string otomatikSpinButtonNormalText = "Otomatik Spin";

    [Header("Az Daha (Near-Miss) Paneli")]
    [Tooltip("Aşama 3'te tam 3 scatter (bonus yok) veya bonus içinde büyük çarpan ama tumble yok sonrası açılır. Metin: 'Az daha! Hadi tekrar dene'. Opsiyonel; atanmazsa panel gösterilmez.")]
    public GameObject azDahaPanel;

    [Header("İstatistik / Log")]
    [Tooltip("İstatistik butonu: tıklanınca Log sahnesine geçer; seçili kullanıcının logları gösterilir. Admin ve senaryo sahnelerinde atanabilir.")]
    public Button istatistikButon;

    private bool bonusEndCloseRequested = false;
    private bool _boslukTusuBasiliSpin = false;

    public float bonusEndShowTime = 1.4f;
    public float bonusEndFadeTime = 0.25f;

    public CanvasGroup bonusStartCanvasGroup;
    public TMP_Text bonusStartTMP;
    public float bonusStartShowTime = 1.2f;
    public float bonusStartFadeTime = 0.25f;

    // SahneBaglamaServisi.IBaglamaHedefi — Inspector ref'leri korunur; sadece null olanlar servis tarafından doldurulur
    Button SahneBaglamaServisi.IBaglamaHedefi.CevirButon { get => cevirButon; set => cevirButon = value; }
    TMP_Text SahneBaglamaServisi.IBaglamaHedefi.BakiyeText { get => bakiyeText; set => bakiyeText = value; }
    TMP_Text SahneBaglamaServisi.IBaglamaHedefi.BahisText { get => bahisText; set => bahisText = value; }
    TMP_Text SahneBaglamaServisi.IBaglamaHedefi.HakText { get => hakText; set => hakText = value; }
    TMP_Text SahneBaglamaServisi.IBaglamaHedefi.KazancText { get => kazancText; set => kazancText = value; }
    TMP_Text SahneBaglamaServisi.IBaglamaHedefi.CarpanText { get => carpanText; set => carpanText = value; }
    TMP_Text SahneBaglamaServisi.IBaglamaHedefi.CarpanOlasilikValueText { get => carpanOlasilikValueText; set => carpanOlasilikValueText = value as TextMeshProUGUI; }
    TMP_Text SahneBaglamaServisi.IBaglamaHedefi.CarpanMaxAdetValueText { get => carpanMaxAdetValueText; set => carpanMaxAdetValueText = value as TextMeshProUGUI; }
    Button SahneBaglamaServisi.IBaglamaHedefi.BakiyeYukleButon { get => bakiyeYukleButon; set => bakiyeYukleButon = value; }
    GameObject SahneBaglamaServisi.IBaglamaHedefi.BakiyeYuklePanel { get => bakiyeYuklePanel; set => bakiyeYuklePanel = value; }
    TMP_InputField SahneBaglamaServisi.IBaglamaHedefi.BakiyeYukleInput { get => bakiyeYukleInput; set => bakiyeYukleInput = value; }
    Button SahneBaglamaServisi.IBaglamaHedefi.BakiyeYukleOnayButon { get => bakiyeYukleOnayButon; set => bakiyeYukleOnayButon = value; }
    Button SahneBaglamaServisi.IBaglamaHedefi.BakiyeYukleIptalButon { get => bakiyeYukleIptalButon; set => bakiyeYukleIptalButon = value; }
    Button SahneBaglamaServisi.IBaglamaHedefi.ParaCekButon { get => paraCekButon; set => paraCekButon = value; }
    GameObject SahneBaglamaServisi.IBaglamaHedefi.ParaCekPanel { get => paraCekPanel; set => paraCekPanel = value; }
    TMP_InputField SahneBaglamaServisi.IBaglamaHedefi.ParaCekInput { get => paraCekInput; set => paraCekInput = value; }
    Button SahneBaglamaServisi.IBaglamaHedefi.ParaCekOnayButon { get => paraCekOnayButon; set => paraCekOnayButon = value; }
    Button SahneBaglamaServisi.IBaglamaHedefi.ParaCekIptalButon { get => paraCekIptalButon; set => paraCekIptalButon = value; }
    Button SahneBaglamaServisi.IBaglamaHedefi.BonusSatinAlButon { get => bonusSatinAlButon; set => bonusSatinAlButon = value; }
    GameObject SahneBaglamaServisi.IBaglamaHedefi.BonusBuyConfirmPanel { get => bonusBuyConfirmPanel; set => bonusBuyConfirmPanel = value; }
    TMP_Text SahneBaglamaServisi.IBaglamaHedefi.BonusBuyConfirmCostText { get => bonusBuyConfirmCostText; set => bonusBuyConfirmCostText = value; }
    CanvasGroup SahneBaglamaServisi.IBaglamaHedefi.BonusBuyConfirmCanvasGroup { get => bonusBuyConfirmCanvasGroup; set => bonusBuyConfirmCanvasGroup = value; }
    Button SahneBaglamaServisi.IBaglamaHedefi.BonusBuyYesButton { get => bonusBuyYesButton; set => bonusBuyYesButton = value; }
    Button SahneBaglamaServisi.IBaglamaHedefi.BonusBuyNoButton { get => bonusBuyNoButton; set => bonusBuyNoButton = value; }
    GameObject SahneBaglamaServisi.IBaglamaHedefi.BonusStartPanel { get => bonusStartPanel; set => bonusStartPanel = value; }
    GameObject SahneBaglamaServisi.IBaglamaHedefi.BonusEndPanel { get => bonusEndPanel; set => bonusEndPanel = value; }
    CanvasGroup SahneBaglamaServisi.IBaglamaHedefi.BonusEndCanvasGroup { get => bonusEndCanvasGroup; set => bonusEndCanvasGroup = value; }
    CanvasGroup SahneBaglamaServisi.IBaglamaHedefi.BonusStartCanvasGroup { get => bonusStartCanvasGroup; set => bonusStartCanvasGroup = value; }

    // IDonusAkisBaglami — state ve servis erişimi (arayüz DonusAkisServisi.cs içinde)
    UIServisi IDonusAkisBaglami.UIServisi => _uiServisi;
    IzgaraServisi IDonusAkisBaglami.IzgaraServisi => _izgaraServisi;
    OdemeServisi IDonusAkisBaglami.OdemeServisi => _odemeServisi;
    AnimasyonServisi IDonusAkisBaglami.AnimasyonServisi => _animasyonServisi;
    TumbleServisi IDonusAkisBaglami.TumbleServisi => _tumbleServisi;
    CarpanServisi IDonusAkisBaglami.CarpanServisi => _carpanServisi;
    EkonomiServisi IDonusAkisBaglami.EkonomiServisi => _ekonomiServisi;
    LogServisi IDonusAkisBaglami.DonusKayitServisi => _logServisi;
    SenaryoServisi IDonusAkisBaglami.SenaryoServisi => _senaryoServisi;
    HizVeSesServisi IDonusAkisBaglami.HizVeSesServisi => _hizVeSesServisi;
    bool IDonusAkisBaglami.SpinCalisiyor { get => spinCalisiyor; set => spinCalisiyor = value; }
    bool IDonusAkisBaglami.BonusAktif { get => bonusAktif; set => bonusAktif = value; }
    int IDonusAkisBaglami.BonusHakKalan { get => bonusHakKalan; set => bonusHakKalan = value; }
    int IDonusAkisBaglami.BonusKazanc { get => bonusKazanc; set => bonusKazanc = value; }
    int IDonusAkisBaglami.OturumKazanc { get => oturumKazanc; set => oturumKazanc = value; }
    int IDonusAkisBaglami.BonusPendingOdemeTL { get => _bonusPendingOdemeTL; set => _bonusPendingOdemeTL = value; }
    int IDonusAkisBaglami.BonusZorlaCarpanBirikenTL { get => _bonusZorlaCarpanBirikenTL; set => _bonusZorlaCarpanBirikenTL = value; }
    bool IDonusAkisBaglami.SpinKazanciOturumaEklendi { get => _spinKazanciOturumaEklendi; set => _spinKazanciOturumaEklendi = value; }
    int IDonusAkisBaglami.SpinKazancHam { get => spinKazancHam; set => spinKazancHam = value; }
    int IDonusAkisBaglami.TumbleToplamKazanc { get => tumbleToplamKazanc; set => tumbleToplamKazanc = value; }
    int IDonusAkisBaglami.SonSpinKazanci { get => sonSpinKazanci; set => sonSpinKazanci = value; }
    int IDonusAkisBaglami.SpinPrevBakiye => _spinPrevBakiye;
    int IDonusAkisBaglami.SpinBahisTL => _spinBahisTL;
    float IDonusAkisBaglami.BonusSpinBekleme => bonusSpinBekleme;
    int IDonusAkisBaglami.SonSpinKazancHamGoster { set => sonSpinKazancHamGoster = value; }
    int IDonusAkisBaglami.SonSpinCarpanGoster { set => sonSpinCarpanGoster = value; }
    int IDonusAkisBaglami.SonSpinKazancToplamGoster { set => sonSpinKazancToplamGoster = value; }
    int IDonusAkisBaglami.BonusOturumOdenenToplamTL { get => _bonusOturumOdenenToplamTL; set => _bonusOturumOdenenToplamTL = value; }
    int IDonusAkisBaglami.BonusMaxOdemeTL => _bonusMaxOdemeTL;
    int IDonusAkisBaglami.BonusOdenenTL => _bonusOdenenTL;
    bool IDonusAkisBaglami.BonusBudgetAktif => bonusBudgetAktif;
    int IDonusAkisBaglami.BonusBudgetKalanTL => _bonusBudgetKalanTL;
    int[,] IDonusAkisBaglami.Grid => grid;
    int IDonusAkisBaglami.Satir => satir;
    int IDonusAkisBaglami.Sutun => sutun;
    void IDonusAkisBaglami.UI_CarpanSifirla() => UI_CarpanSifirla();
    void IDonusAkisBaglami.CarpanUretVeBirik() => CarpanUretVeBirik();
    void IDonusAkisBaglami.CarpanlariDoluGriddeUygula() => _carpanYerlestirmeServisi?.CarpanlariDoluGriddeUygula();
    void IDonusAkisBaglami.BaslatBonus() => BaslatBonus();
    IEnumerator IDonusAkisBaglami.ScatterBuyutEfekti() => ScatterBuyutEfekti();
    IEnumerator IDonusAkisBaglami.ShowBonusEndMessage(int bonusToplamKazanc) => ShowBonusEndMessage(bonusToplamKazanc);
    void IDonusAkisBaglami.SetSpinIconRotate(bool rotate) { if (spinIcon != null) spinIcon.SetRotate(rotate); }
    void IDonusAkisBaglami.SetOturumKazancTextActive(bool active) { if (oturumKazancText != null) oturumKazancText.gameObject.SetActive(active); }
    void IDonusAkisBaglami.NormalOyunMusicPlay() { if (normalOyunMusic != null && normalOyunMusic.clip != null && !normalOyunMusic.isPlaying) normalOyunMusic.Play(); }
    void IDonusAkisBaglami.NormalOyunMusicUnPause() { if (normalOyunMusic != null) normalOyunMusic.UnPause(); }
    SpinSimulasyonKaydi IDonusAkisBaglami.SimuleEtVeKaydet(int odenebilirLimit, bool bonusSpin) => SimuleEtVeKaydetImpl(odenebilirLimit, bonusSpin);
    IEnumerator IDonusAkisBaglami.SimulasyonKaydiniOynat(SpinSimulasyonKaydi kayit) => SimulasyonKaydiniOynatImpl(kayit);
    void IDonusAkisBaglami.TryResumeOtomatikSpin() => TryResumeOtomatikSpin();
    IEnumerator IDonusAkisBaglami.ShowAzDahaPanelAndWait() => ShowAzDahaPanelAndWait();

    // IOyunUIGuncellemeBaglami
    TMP_Text IOyunUIGuncellemeBaglami.BakiyeText => bakiyeText;
    TMP_Text IOyunUIGuncellemeBaglami.BahisText => bahisText;
    TMP_Text IOyunUIGuncellemeBaglami.HakText => hakText;
    TMP_Text IOyunUIGuncellemeBaglami.OturumKazancText => oturumKazancText;
    TMP_Text IOyunUIGuncellemeBaglami.KazancText => kazancText;
    TMP_Text IOyunUIGuncellemeBaglami.CarpanText => carpanText;
    TMP_Text IOyunUIGuncellemeBaglami.BonusSatinAlText => bonusSatinAlText;
    Button IOyunUIGuncellemeBaglami.CevirButon => cevirButon;
    Button IOyunUIGuncellemeBaglami.ParaCekButon => paraCekButon;
    Button IOyunUIGuncellemeBaglami.BakiyeYukleButon => bakiyeYukleButon;
    Button IOyunUIGuncellemeBaglami.BahisArttirButon => bahisArttirButon;
    Button IOyunUIGuncellemeBaglami.BahisAzaltButon => bahisAzaltButon;
    Button IOyunUIGuncellemeBaglami.BonusSatinAlButon => bonusSatinAlButon;
    Button IOyunUIGuncellemeBaglami.ParaCekOnayButon => paraCekOnayButon;
    Button IOyunUIGuncellemeBaglami.ParaCekIptalButon => paraCekIptalButon;
    Button IOyunUIGuncellemeBaglami.BakiyeYukleOnayButon => bakiyeYukleOnayButon;
    Button IOyunUIGuncellemeBaglami.BakiyeYukleIptalButon => bakiyeYukleIptalButon;
    GameObject IOyunUIGuncellemeBaglami.ParaCekPanel => paraCekPanel;
    GameObject IOyunUIGuncellemeBaglami.BakiyeYuklePanel => bakiyeYuklePanel;
    GameObject IOyunUIGuncellemeBaglami.BonusSatinAlRoot => bonusSatinAlRoot;
    int IOyunUIGuncellemeBaglami.GetBakiye() => _ekonomiServisi != null ? _ekonomiServisi.Bakiye : 0;
    int IOyunUIGuncellemeBaglami.GetBahis() => _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0;
    int IOyunUIGuncellemeBaglami.GetBahisMin() => bahisMin;
    int IOyunUIGuncellemeBaglami.GetBahisMax() => bahisMax;
    bool IOyunUIGuncellemeBaglami.GetBonusAktif() => bonusAktif;
    bool IOyunUIGuncellemeBaglami.GetSpinCalisiyor() => spinCalisiyor;
    int IOyunUIGuncellemeBaglami.GetBonusHakKalan() => bonusHakKalan;
    int IOyunUIGuncellemeBaglami.GetOturumKazanc() => oturumKazanc;
    int IOyunUIGuncellemeBaglami.GetSonSpinKazanci() => sonSpinKazanci;
    bool IOyunUIGuncellemeBaglami.GetSpinKazanciOturumaEklendi() => _spinKazanciOturumaEklendi;
    int IOyunUIGuncellemeBaglami.GetSonSpinKazancHamGoster() => sonSpinKazancHamGoster;
    int IOyunUIGuncellemeBaglami.GetSonSpinCarpanGoster() => sonSpinCarpanGoster;
    int IOyunUIGuncellemeBaglami.GetSonSpinKazancToplamGoster() => sonSpinKazancToplamGoster;
    int IOyunUIGuncellemeBaglami.GetBonusMaliyeti() => Mathf.Max(0, _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0) * Mathf.Max(1, bonusSatinAlCarpani);
    void IOyunUIGuncellemeBaglami.RefreshCarpanDisplay() => UI_CarpanGuncelle();
    void IOyunUIGuncellemeBaglami.ShowParaCekPanel() => _uiServisi?.ShowParaCekPanel();
    void IOyunUIGuncellemeBaglami.HideParaCekPanel() => _uiServisi?.HideParaCekPanel();
    void IOyunUIGuncellemeBaglami.ShowBakiyeYuklePanel() => _uiServisi?.ShowBakiyeYuklePanel();
    void IOyunUIGuncellemeBaglami.HideBakiyeYuklePanel() => _uiServisi?.HideBakiyeYuklePanel();
    void IOyunUIGuncellemeBaglami.OnParaCekOnay() => _ekonomiServisi?.OnParaCekOnay();
    void IOyunUIGuncellemeBaglami.OnBakiyeYukleOnay() => _ekonomiServisi?.OnBakiyeYukleOnay();
    void IOyunUIGuncellemeBaglami.SyncOtomatikSpinKalanTextVisibility() => OtomatikSpinKalanTextGuncelle();

    // IScatterEfektBaglami
    int[,] IScatterEfektBaglami.Grid => grid;
    int IScatterEfektBaglami.ScatterIndex => _scatterIndexCache;
    int IScatterEfektBaglami.Sutun => sutun;
    int IScatterEfektBaglami.Satir => satir;
    int IScatterEfektBaglami.XYToIndex(int x, int y) => _izgaraServisi != null ? _izgaraServisi.XYToIndex(x, y) : -1;
    Image[] IScatterEfektBaglami.Hucreler => hucreler;
    float IScatterEfektBaglami.ScatterScaleUp => scatterScaleUp;
    float IScatterEfektBaglami.ScatterAnimDuration => scatterAnimDuration;

    // ITumbleAkisBaglami
    int ITumbleAkisBaglami.GetMinClusterSize() => minClusterSize;
    bool ITumbleAkisBaglami.GetBonusAktif() => bonusAktif;
    int ITumbleAkisBaglami.GetBonusRemainingPayableTL() => _senaryoServisi != null ? _senaryoServisi.GetBonusRemainingPayableTL() : int.MaxValue;
    int ITumbleAkisBaglami.GetCurrentMultiplierInt() => _carpanServisi != null ? _carpanServisi.GetCurrentMultiplierInt() : 1;
    long ITumbleAkisBaglami.GetCurrentMultiplier() => _carpanServisi != null ? _carpanServisi.GetCurrentMultiplier() : 1L;
    int ITumbleAkisBaglami.GetSpinKazancHam() => spinKazancHam;
    void ITumbleAkisBaglami.AddSpinKazancHam(int delta) { spinKazancHam += delta; }
    int ITumbleAkisBaglami.CalculateWinForRemoved(List<Vector2Int> removed) => _tumbleServisi != null ? _tumbleServisi.CalculateWinForRemoved(removed) : 0;
    List<Vector2Int> ITumbleAkisBaglami.FindClustersToRemove(int minSize) => _tumbleServisi != null ? _tumbleServisi.FindClustersToRemove(minSize) : new List<Vector2Int>();
    void ITumbleAkisBaglami.CarpanUretVeBirik() => CarpanUretVeBirik();
    void ITumbleAkisBaglami.AddTumbleToplamKazanc(int delta) { tumbleToplamKazanc += delta; }
    void ITumbleAkisBaglami.SetSonSpinKazancHamGoster(int value) { sonSpinKazancHamGoster = value; }
    void ITumbleAkisBaglami.SetSonSpinCarpanGoster(int value) { sonSpinCarpanGoster = value; }
    void ITumbleAkisBaglami.SetSonSpinKazancToplamGoster(int value) { sonSpinKazancToplamGoster = value; }
    void ITumbleAkisBaglami.SetSonSpinKazanci(int value) { sonSpinKazanci = value; }
    int ITumbleAkisBaglami.MulClampInt(int ham, long multiplier) => _carpanServisi != null ? _carpanServisi.MulClampInt(ham, multiplier) : ham;
    void ITumbleAkisBaglami.UI_Guncelle() => _uiServisi?.UI_Guncelle();
    void ITumbleAkisBaglami.PlayTumbleSfx() => _hizVeSesServisi?.PlayTumbleSfx(tumblePopClip, ref _lastTumblePopTime, tumblePopMinInterval, 1f);
    void ITumbleAkisBaglami.ClearGridCells(List<Vector2Int> toRemove)
    {
        if (toRemove == null || grid == null || carpanDegerGrid == null) return;
        for (int i = 0; i < toRemove.Count; i++)
        {
            int x = toRemove[i].x, y = toRemove[i].y;
            grid[x, y] = -1;
            carpanDegerGrid[x, y] = 0;
            int ridx = _izgaraServisi != null ? _izgaraServisi.XYToIndex(x, y) : -1;
            if (carpanDegerByCellIndex != null && ridx >= 0 && ridx < carpanDegerByCellIndex.Length)
                carpanDegerByCellIndex[ridx] = 0;
        }
    }
    IEnumerator ITumbleAkisBaglami.AnimateCarpanSisme() => _animasyonServisi != null ? _animasyonServisi.AnimateCarpanSisme() : null;
    IEnumerator ITumbleAkisBaglami.AnimatePop(List<Vector2Int> cells) => _tumbleServisi != null ? _tumbleServisi.AnimatePop(cells) : null;
    IEnumerator ITumbleAkisBaglami.CollapseRefillAndAnimate() => _cokmeAkisServisi != null ? _cokmeAkisServisi.CokmeDoldurVeCanlandir() : null;
    float ITumbleAkisBaglami.GetBetweenStepsDelay() => betweenStepsDelay;
    Coroutine ITumbleAkisBaglami.RunCoroutine(IEnumerator enumerator) => enumerator != null ? StartCoroutine(enumerator) : null;

    int ICokmeAkisBaglami.GetSutun() => sutun;
    int ICokmeAkisBaglami.GetSatir() => satir;
    int[,] ICokmeAkisBaglami.GetGrid() => grid;
    int[,] ICokmeAkisBaglami.GetCarpanDegerGrid() => carpanDegerGrid;
    Vector2[] ICokmeAkisBaglami.GetCellPos() => cellPos;
    RectTransform[] ICokmeAkisBaglami.GetCellRT() => cellRT;
    float ICokmeAkisBaglami.GetSpawnFromTopOffset() => spawnFromTopOffset;
    float ICokmeAkisBaglami.GetFallDuration() => fallDuration;
    bool ICokmeAkisBaglami.GetBonusAktif() => bonusAktif;
    int ICokmeAkisBaglami.GetCarpanSembol() => CARPAN_SEMBOL;
    IzgaraServisi ICokmeAkisBaglami.GetIzgaraServisi() => _izgaraServisi;
    TumbleServisi ICokmeAkisBaglami.GetTumbleServisi() => _tumbleServisi;
    CarpanServisi ICokmeAkisBaglami.GetCarpanServisi() => _carpanServisi;
    SenaryoServisi ICokmeAkisBaglami.GetSenaryoServisi() => _senaryoServisi;
    void ICokmeAkisBaglami.ApplyNewGridAndSync(int[,] newGrid, int[,] newCarpanGrid) => ApplyNewGridAndSync(newGrid, newCarpanGrid);

    private void ApplyNewGridAndSync(int[,] newGrid, int[,] newCarpanGrid)
    {
        grid = newGrid;
        carpanDegerGrid = newCarpanGrid;
        _tumbleServisi?.SetGrid(grid);
        _izgaraServisi?.SetGrid(grid);
        _izgaraServisi?.SetCarpanDegerGrid(carpanDegerGrid);
        if (carpanDegerByCellIndex == null || carpanDegerByCellIndex.Length != sutun * satir)
            carpanDegerByCellIndex = new int[sutun * satir];
        for (int yy = 0; yy < satir; yy++)
        {
            for (int xx = 0; xx < sutun; xx++)
            {
                int cidx2 = _izgaraServisi != null ? _izgaraServisi.XYToIndex(xx, yy) : 0;
                if (cidx2 < 0 || cidx2 >= carpanDegerByCellIndex.Length) continue;
                carpanDegerByCellIndex[cidx2] = (grid[xx, yy] == CARPAN_SEMBOL) ? carpanDegerGrid[xx, yy] : 0;
            }
        }
    }

    [Header("Hücreler (SlotGrid altındaki 30 Image)")]
    [Tooltip("Boş bırakırsan slotGridRoot altından otomatik toplanır.")]
    public Image[] hucreler;

    [Tooltip("SlotGrid objesini buraya ver (GridLayoutGroup bunun üstünde).")]
    public Transform slotGridRoot;

    [Header("Çarpan Ayarları")]
    public bool carpanUretimiAktif = true;
    public bool carpanSadeceBonus = false;
    [Range(0f, 1f)] public float carpanUretimOlasiligi = 0.15f;
    [Range(1, 10)] public int maxCarpanAdedi = 2;
    [Range(1, 10)] public int carpanHavuzu = 10; // havuz büyüklüğü
    [Tooltip("Rastgele çarpan seçilirken 100x/250x/500x gelme olasılığı (0-1). Yüksek = büyük çarpanlar daha sık düşer.")]
    [Range(0f, 1f)] public float yuksekCarpanOrani = 0.30f;
    public int zorlaSiradakiCarpan = 0;

[Header("Çarpan Görseli (Sweet Bonanza tarzı)")]
[Tooltip("Çarpan jeton/bomba sprite'ını buraya ver (arka plan transparan).")]
public Sprite carpanSembolSprite;

[Tooltip("Çarpan overlay boyutu (px).")]
public Vector2 carpanOverlaySize = new Vector2(110f, 110f);

[Tooltip("Overlay üzerindeki x2/x5 yazı boyutu.")]
public int carpanOverlayFontSize = 36;

    [Header("Çarpan Yazı Görünümü")]
    public Color carpanYaziRengi = Color.white;
    public Color carpanYaziDisCizgiRengi = new Color(0f, 0f, 0f, 1f);
    [Range(0f, 1f)] public float carpanYaziDisCizgiKalinlik = 0.35f;
    public bool carpanYaziKalin = true;
    public bool carpanYaziGolge = true;
    public Color carpanYaziGolgeRengi = new Color(0f, 0f, 0f, 0.85f);
    public Vector2 carpanYaziGolgeOffset = new Vector2(2f, -2f);

[Tooltip("Yazının konum offseti.")]
public Vector2 carpanOverlayTextOffset = new Vector2(0f, -6f);

[Tooltip("Overlay düşme animasyonu başlangıç Y offset.")]
public float carpanOverlayDropStartYOffset = 250f;

[Tooltip("Overlay düşme animasyonu süresi.")]
public float carpanOverlayDropDuration = 0.18f;


    // -------------------------
    // internal state
    // -------------------------
    private int[,] grid;
    // Çarpan sembolü artık grid içinde gerçek bir sembol gibi durur (meyve yerine düşer).
    // grid hücresinde -1 = boş, -2 = çarpan sembolü, 0..N-1 = normal semboller
    private const int CARPAN_SEMBOL = -2;
    private int[,] carpanDegerGrid;
    // Çarpan değerleri bazen (özellikle tumble olmadan) grid yeniden render edilirken sıfırlanabiliyor.
    // Bu yüzden hücre index bazlı yedek tutuyoruz; render sırasında 0 görürsek buradan geri yükleriz.
    private int[] carpanDegerByCellIndex;
 // CARPAN_SEMBOL olan hücrelerin çarpan değeri (x2, x5...)
    private TextMeshProUGUI[] carpanHücreTextleri; // her hücre için x2 yazısı

    private bool spinCalisiyor = false;
    private bool bonusAktif = false;
    private int bonusHakKalan = 0;

    private int normalKazanc = 0;
    private int bonusKazanc = 0;
    private int sonSpinKazanci = 0; // ekranda gösterilecek: son spin kazancı
    private int sonSpinKazancHamGoster = 0;
    private int sonSpinCarpanGoster = 1;
    private int sonSpinKazancToplamGoster = 0;

    // UI pos cache
    private Vector2[] cellPos;
    private RectTransform[] cellRT;
    private Behaviour layoutToDisable;

    /// <summary>Tek kaynak: Sahnedeki BonusAyarlari / KasaYoneticisi varsa değerleri OY alanlarına kopyala. Bahis limitleri OY default (bahisMin/Max/Adim) ve EkonomiServisi.SetBahisLimits ile verilir.</summary>
    private void SyncFromAyarClassesIfPresent()
    {
        var bonus = FindFirstObjectByType<BonusAyarlari>(FindObjectsInactive.Include);
        _bonusAyarlari = bonus;
        if (bonus != null)
        {
            bonusHakBaslangic = bonus.BonusHakBaslangic;
            bonusSpinBekleme = bonus.BonusSpinBekleme;
            bonusSatinAlCarpani = bonus.BonusSatinAlCarpani;
            bonusBudgetAktif = bonus.BonusBudgetAktif;
            bonusBudgetHavuzOran = bonus.BonusBudgetHavuzOran;
            bonusBudgetMinTL = bonus.BonusBudgetMinTL;
            bonusBudgetMaxTL = bonus.BonusBudgetMaxTL;
            bonusOtoZorlukAktif = bonus.BonusOtoZorlukAktif;
            bonusMinCluster_Easy = bonus.BonusMinCluster_Easy;
            bonusMinCluster_Hard = bonus.BonusMinCluster_Hard;
            // Scatter şansı admin panel slider'dan gelir; config ile üzerine yazma (slider 0 = scatter yok).
            // scatterChanceNormal = bonus.ScatterChanceNormal;
            // scatterChanceBonus = bonus.ScatterChanceBonus;
            scatterEsik = bonus.ScatterEsik;
            scatterScaleUp = bonus.ScatterScaleUp;
            scatterAnimDuration = bonus.ScatterAnimDuration;
        }
        var kasaObj = kasa ?? FindFirstObjectByType<KasaYoneticisi>(FindObjectsInactive.Include);
        if (kasaObj != null)
        {
            bonusBudgetAktif = kasaObj.BonusBudgetAktif;
            bonusBudgetHavuzOran = kasaObj.BonusBudgetHavuzOran;
            bonusBudgetMinTL = kasaObj.BonusBudgetMinTL;
            bonusBudgetMaxTL = kasaObj.BonusBudgetMaxTL;
            bonusMaxOdemeHavuzOrani = kasaObj.BonusMaxOdemeHavuzOrani;
            kasaBazliDengeAktif = kasaObj.KasaBazliDengeAktif;
            minClusterSize_HavuzBos = kasaObj.MinClusterSize_HavuzBos;
            minClusterSize_HavuzDolu = kasaObj.MinClusterSize_HavuzDolu;
            minClusterSize_HavuzAz = kasaObj.MinClusterSize_HavuzAz;
            havuzAzEsik01 = kasaObj.HavuzAzEsik01;
            havuzDoluEsik01 = kasaObj.HavuzDoluEsik01;
            bonusOtoZorlukAktif = kasaObj.BonusOtoZorlukAktif;
            bonusMinCluster_Easy = kasaObj.BonusMinCluster_Easy;
            bonusMinCluster_Hard = kasaObj.BonusMinCluster_Hard;
        }
    }

    void Start()
    {
        SyncFromAyarClassesIfPresent();
        _oyunBootstrapServisi = new OyunBootstrapServisi();
        _oyunBootstrapServisi.SetBaglam(this);
        _oyunBootstrapServisi.Calistir();
    }

    void IOyunBootstrapBaglami.BootstrapMantiginiCalistir()
    {
        _logServisi = new LogServisi();
        _ekonomiServisi = new EkonomiServisi();
        _uiServisi = new UIServisi();
        _zorlukServisi = new ZorlukServisi();
        _zorlukServisi.SetBaglam(this);
        _oyunUIGuncellemeServisi = new OyunUIGuncellemeServisi();
        _oyunUIGuncellemeServisi.SetBaglam(this);
        _scatterEfektServisi = new ScatterEfektServisi();
        _scatterEfektServisi.SetBaglam(this);
        _uiServisi.SetUIGuncelleImpl(() => _oyunUIGuncellemeServisi?.RefreshAllUI());
        _uiServisi.SetButonDurumuImpl(acik => _oyunUIGuncellemeServisi?.SetButtonsInteractable(acik));
        _uiServisi.SetShowParaCekPanelImpl(ShowParaCekPanel);
        _uiServisi.SetHideParaCekPanelImpl(HideParaCekPanel);
        _uiServisi.SetShowBakiyeYuklePanelImpl(() => ShowBakiyeYuklePanel());
        _uiServisi.SetHideBakiyeYuklePanelImpl(HideBakiyeYuklePanel);
        _uiServisi.SetCloseMoneyPanelsImpl(CloseMoneyPanels);
        _uiServisi.SetShowBonusBuyConfirmPanelImpl(ShowBonusBuyConfirmPanel);
        _uiServisi.SetHideBonusBuyConfirmPanelImpl(HideBonusBuyConfirmPanel);
        _sahneBaglamaServisi = new SahneBaglamaServisi();
        _uiServisi.SetUIAutoBaglaGerekirseImpl(() => _sahneBaglamaServisi.BindIfNeeded(transform, this));
        _uiServisi.SetResolveMoneyUIRefsIfMissingImpl(() => _sahneBaglamaServisi.BindIfNeeded(transform, this));
        _uiServisi.SetWireParaCekUIImpl(() => _oyunUIGuncellemeServisi?.WireMoneyPanelsIfNeeded());
        _uiServisi.SetWireBakiyeYukleUIImpl(() => _oyunUIGuncellemeServisi?.WireMoneyPanelsIfNeeded());

        _odemeServisi = new OdemeServisi();
        _odemeServisi.SetGetHavuzTL(() => kasa != null ? kasa.odulHavuzuTL : 0L);
        _odemeServisi.SetParaGirisiBolVeEkle(tl => { if (kasa != null) kasa.ParaGirisi_BolVeEkle(tl); });
        _odemeServisi.SetOdemeYapOdulHavuzundan(istenen => kasa != null ? kasa.OdemeYap_OdulHavuzundan(istenen) : 0);
        _odemeServisi.SetGetOdenebilirLimitDynamic(() => SenaryoYoneticisi.I != null && _senaryoOdenebilirKalanTL >= 0 ? _senaryoOdenebilirKalanTL : -1);

        _senaryoServisi = new SenaryoServisi();
        _senaryoServisi.SetSetZorlukImpl(SetZorluk);
        _senaryoServisi.SetBiasMultiplierImpl(BiasMultiplier);
        _senaryoServisi.SetGetScatterChanceImpl(GetScatterChanceFor);
        _senaryoServisi.SetGetScatterEsikImpl(() => scatterEsik);
        _senaryoServisi.SetGetMaxScatterPerSpinImpl(() => maxScatterPerSpin);
        _senaryoServisi.SetGetCarpanUretimOlasiligiImpl(() => carpanUretimOlasiligi);
        _senaryoServisi.SetGetMaxCarpanAdediImpl(() => maxCarpanAdedi);
        _senaryoServisi.SetIsCarpanSadeceBonusImpl(() => carpanSadeceBonus);
        _senaryoServisi.SetIsCarpanUretimiAktifImpl(() => carpanUretimiAktif);
        _senaryoServisi.SetInitBonusBudgetFromHavuzImpl(InitBonusBudgetFromHavuz);
        _senaryoServisi.SetGetBonusRemainingPayableTLImpl(GetBonusRemainingPayableTL);
        _senaryoServisi.SetRecordBonusPaymentImpl(RecordBonusPayment);

        _donusAkisServisi = new DonusAkisServisi();
        _donusAkisServisi.SetBaglam(this);
        _donusAkisServisi.SetRunCoroutine(StartCoroutine);

        _donusServisi = new DonusServisi();
        _donusServisi.SetSpinButonImpl(SpinButonImpl);
        _donusServisi.SetNormalSpinAkisiImpl(() => _donusAkisServisi.NormalSpinAkisi());
        _donusServisi.SetBaslatBonusImpl(BaslatBonus);
        _donusServisi.SetBonusBaslangicAkisiImpl(BonusBaslangicAkisi);
        _donusServisi.SetBonusDongusuImpl(() => _donusAkisServisi.BonusDongusu());
        _donusServisi.SetShowBonusStartMessageImpl(ShowBonusStartMessage);
        _donusServisi.SetShowBonusEndMessageImpl(ShowBonusEndMessage);

        _izgaraServisi = new IzgaraServisi();
        _izgaraServisi.SetGridDimensions(satir, sutun);
        _scatterIndexCache = (tumbleAyarlari != null) ? tumbleAyarlari.ScatterIndex : 7;
        // Sembol listesinde 9+ sembol varsa silah genelde index 8'dedir; 7 ile sayarsak bonus tetiklenmez.
        if (_scatterIndexCache == 7 && sembolSpriteListesi != null && sembolSpriteListesi.Count > 8)
            _scatterIndexCache = 8;
        _izgaraServisi.SetScatterSpriteIndex(_scatterIndexCache);
        _izgaraServisi.SetSembolSpriteListesi(sembolSpriteListesi);
        _izgaraServisi.SetCarpanSembolSprite(carpanSembolSprite);
        _izgaraServisi.SetSlotGridRoot(slotGridRoot);
        _izgaraServisi.SetCalculateWinForRemoved(removed => _tumbleServisi != null ? _tumbleServisi.CalculateWinForRemoved(removed) : 0);
        _izgaraServisi.SetGetBonusAktif(() => bonusAktif);
        _izgaraServisi.SetGetEffectiveFillLimit(limit => bonusMaxOdemeHavuzOrani <= 0f || !bonusAktif ? limit : Mathf.Min(limit, Mathf.Max(0, _bonusMaxOdemeTL - _bonusOdenenTL)));
        _izgaraServisi.SetGetScatterChance(_senaryoServisi.GetScatterChance);
        _izgaraServisi.SetGetMaxScatterPerSpin(_senaryoServisi.GetMaxScatterPerSpin);
        _izgaraServisi.SetBiasMultiplier(_senaryoServisi.BiasMultiplier);
        _izgaraServisi.SetGetHardBias01(() => _hardBias01);
        _izgaraServisi.SetGetPayTableBase(() => tumbleAyarlari != null ? tumbleAyarlari.PayTable_8_9 : null);

        _cokmeAkisServisi = new CokmeAkisServisi();
        _cokmeAkisServisi.SetBaglam(this);
        _tumbleAkisServisi = new TumbleAkisServisi();
        _tumbleAkisServisi.SetBaglam(this);
        _tumbleServisi = new TumbleServisi();
        _tumbleServisi.SetTumbleLoopImpl(onKazanc => _tumbleAkisServisi.TumbleLoop(onKazanc));
        _tumbleServisi.SetGetBonusAktif(() => bonusAktif);
        _tumbleServisi.SetGetBonusRemainingPayableTL(() => _senaryoServisi.GetBonusRemainingPayableTL());
        _tumbleServisi.SetScatterSpriteIndex(_scatterIndexCache);
        _tumbleServisi.SetGetCurrentBet(() => _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0);
        _tumbleServisi.SetCalculateWithPayTable(removed =>
        {
            int ham = tumbleAyarlari != null ? tumbleAyarlari.CalculateWinWithOwnPayTable(removed, grid, satir, sutun, _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0, minClusterSize) : -1;
            return ham < 0 ? -1 : ZorlukKazancCarpaniUygula(ham);
        });
        _tumbleServisi.SetCollapseRefillAndAnimateImpl(CollapseRefillAndAnimate);
        _animasyonServisi = new AnimasyonServisi();
        _animasyonServisi.SetHucreler(hucreler);
        _animasyonServisi.SetCellPos(cellPos);
        _animasyonServisi.SetDurations(dropDuration, dropStagger, dropStartYOffset, popDuration);
        _animasyonServisi.SetPopParticlePrefab(popParticlePrefab);
        Canvas canvas = GetComponentInParent<Canvas>();
        _animasyonServisi.SetParticleParent(canvas != null ? canvas.transform : transform);
        _animasyonServisi.SetXYToIndex((x, y) => _izgaraServisi != null ? _izgaraServisi.XYToIndex(x, y) : -1);
        _animasyonServisi.SetOnRefreshCarpanTexts(() => _izgaraServisi?.ForceRefreshCarpanTextsFromGrid());
        _korutinServisi = new KorutinServisi();
        _korutinServisi.SetRunner(r => StartCoroutine(r), c => StopCoroutine(c));

        _carpanOverlayServisi = new CarpanOverlayServisi();
        _carpanOverlayServisi.SetCellImages(hucreler);
        _carpanOverlayServisi.SetCarpanSembolSprite(carpanSembolSprite);
        _carpanOverlayServisi.SetOverlaySize(carpanOverlaySize);
        _carpanOverlayServisi.SetOverlayFontSize(carpanOverlayFontSize);
        _carpanOverlayServisi.SetOverlayTextOffset(carpanOverlayTextOffset);
        _carpanOverlayServisi.SetDropStartYOffset(carpanOverlayDropStartYOffset);
        _carpanOverlayServisi.SetDropDuration(carpanOverlayDropDuration);
        _carpanOverlayServisi.SetStartNamedCoroutine((key, coro) => _korutinServisi.StartNamed(key, coro));
        _carpanOverlayServisi.SetStopNamedCoroutine(key => _korutinServisi.StopNamed(key));
        _animasyonServisi.SetGetCarpanOverlays(() =>
        {
            var d = new Dictionary<int, AnimasyonServisi.CarpanOverlayRef>();
            foreach (var kv in _carpanOverlayServisi.AnimasyonIcinOverlayleriAl())
                d[kv.Key] = new AnimasyonServisi.CarpanOverlayRef { rt = kv.Value.rt, tmp = kv.Value.tmp };
            return d;
        });
        _animasyonServisi.SetRunCoroutine(coro => StartCoroutine(coro));
        _tumbleServisi.SetAnimatePopImpl(cells => _animasyonServisi.AnimatePop(cells));

        _izgaraServisi.SetFindClustersToRemove(_tumbleServisi.FindClustersToRemove);

        _carpanServisi = new CarpanServisi();
        _carpanServisi.SetIsCarpanUretimiAktif(() => carpanUretimiAktif);
        _carpanServisi.SetIsCarpanSadeceBonus(() => carpanSadeceBonus);
        _carpanServisi.SetGetCarpanUretimOlasiligi(() => carpanUretimOlasiligi);
        _carpanServisi.SetGetMaxCarpanAdedi(() => maxCarpanAdedi);
        _carpanServisi.SetRollCarpanDegeri(RastgeleCarpan);
        _carpanServisi.SetGetSpinKazancHam(() => spinKazancHam);
        _carpanServisi.SetGetBonusRemainingPayableTL(() => _senaryoServisi.GetBonusRemainingPayableTL());

        _carpanYerlestirmeServisi = new CarpanYerlestirmeServisi();
        _carpanYerlestirmeServisi.SetBaglam(this);

        _ekonomiServisi.SetLogServisi(_logServisi);
        _ekonomiServisi.SetParaCekInput(paraCekInput);
        _ekonomiServisi.SetBakiyeYukleInput(bakiyeYukleInput);
        _ekonomiServisi.SetParaCekUyariText(paraCekUyariText);
        _ekonomiServisi.SetBakiyeYukleUyariText(bakiyeYukleUyariText);
        _ekonomiServisi.SetBahisLimits(bahisMin, bahisMax, bahisAdim);
        _ekonomiServisi.SetCanChangeBet(() => !spinCalisiyor && !bonusAktif);
        _ekonomiServisi.SetOnEconomyChanged(() => { _uiServisi?.UI_Guncelle(); _uiServisi?.ButonDurumu(true); });
        _ekonomiServisi.SetGetCurrentMultiplier(() => _carpanServisi.GetCurrentMultiplier());
        _ekonomiServisi.SetCarpanSifirla(UI_CarpanSifirla);
        _ekonomiServisi.SetOnParaCekildi(miktar =>
        {
            SenaryoYoneticisi.I?.LogEkle(SenaryoOlayKaydi.OlayTipi_ParaCekildi, $"Para çekildi: {OyunFormatServisi.FormatTL(miktar)}. Güncel bakiye: {(_ekonomiServisi != null ? OyunFormatServisi.FormatTL(_ekonomiServisi.Bakiye) : "—")}.");
        });
        _ekonomiServisi.SetOnBakiyeYuklemeReddedildi(() =>
        {
            SenaryoYoneticisi.I?.LogEkle(SenaryoOlayKaydi.OlayTipi_BakiyeYuklemeReddedildi, "Bakiye yükleme reddedildi. Kalan yükleme hakkı: 0.");
        });

        _bonusUIServisi = new BonusUIServisi();
        _bonusUIServisi.SetBonusStartPanel(bonusStartPanel);
        _bonusUIServisi.SetBonusBellAudio(bonusBellAudio);
        _bonusUIServisi.SetBonusStartCanvasGroup(bonusStartCanvasGroup);
        _bonusUIServisi.SetBonusStartTMP(bonusStartTMP);
        _bonusUIServisi.SetGetBonusHakKalan(() => bonusHakKalan);
        _bonusUIServisi.SetBonusStartFadeTime(bonusStartFadeTime);
        _bonusUIServisi.SetBonusStartShowTime(bonusStartShowTime);
        _bonusUIServisi.SetBonusEndPanel(bonusEndPanel);
        _bonusUIServisi.SetGetBonusEndCloseRequested(() => bonusEndCloseRequested);
        _bonusUIServisi.SetSetBonusEndCloseRequested(v => bonusEndCloseRequested = v);
        // Bonus bitiş paneli her zaman 5 sn sayıp kapansın (scatter veya satın alma fark etmez)
        _bonusUIServisi.SetGetBonusEndAutoCloseSeconds(() => 5f);
        _bonusUIServisi.SetBonusEndCloseButtonTextUpdater(kalan =>
        {
            var tmp = bonusEndCloseButton != null ? bonusEndCloseButton.GetComponentInChildren<TMP_Text>(true) : null;
            if (tmp != null)
                tmp.text = kalan >= 0 ? $"TAMAM (5sn) {kalan}" : "TAMAM";
        });
        _bonusUIServisi.SetBonusEndSfx(bonusEndSfxSource, bonusEndApplauseClip);
        _bonusUIServisi.SetBonusEndCanvasGroup(bonusEndCanvasGroup);
        _bonusUIServisi.SetBonusEndTitleTMP(bonusEndTitleTMP);
        _bonusUIServisi.SetBonusEndWinTMP(bonusEndWinTMP);
        _bonusUIServisi.SetFormatTL(OyunFormatServisi.FormatTL);
        _bonusUIServisi.SetBonusEndMusicAudio(bonusEndMusicAudio);

        _hizVeSesServisi = new HizVeSesServisi();
        _hizVeSesServisi.SetGetBonusYavasMod(() => bonusYavasMod);
        _hizVeSesServisi.SetGetDurations(() => (popDuration, fallDuration, betweenStepsDelay, bonusSpinBekleme));
        _hizVeSesServisi.SetSetDurations((p, f, b, w) => { popDuration = p; fallDuration = f; betweenStepsDelay = b; bonusSpinBekleme = w; });
        _hizVeSesServisi.SetGetBonusSpeedOverrides(() => (bonusPopDuration, bonusFallDuration, bonusBetweenStepsDelay, bonusSpinBeklemeOverride));
        _hizVeSesServisi.SetGetUnscaledTime(() => Time.unscaledTime);
        _hizVeSesServisi.SetAudioSource(tumbleSfxSource);

        _bonusUIServisi.SetGetBakiye(() => _ekonomiServisi != null ? _ekonomiServisi.Bakiye : 0);
        _bonusUIServisi.SetGetBonusMaliyeti(() => Mathf.Max(0, _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0) * Mathf.Max(1, bonusSatinAlCarpani));
        _bonusUIServisi.SetGetSpinCalisiyor(() => spinCalisiyor);
        _bonusUIServisi.SetGetBonusAktif(() => bonusAktif);
        _bonusUIServisi.SetShowConfirmPanel(cost =>
        {
            if (_bonusAyarlari == null)
                _bonusAyarlari = FindFirstObjectByType<BonusAyarlari>(FindObjectsInactive.Include);
            if (_bonusAyarlari != null)
            {
                _bonusAyarlari.Goster(cost, _ekonomiServisi.Bakiye, () => _bonusUIServisi.OnYes(), () => _bonusUIServisi.OnNo());
                Debug.Log("[BONUS SATIN AL] Panel BonusAyarlari.Goster ile açıldı.");
                return;
            }
            if (bonusBuyConfirmPanel != null)
            {
                if (bonusBuyConfirmCostText != null) bonusBuyConfirmCostText.text = $"Maliyet: {OyunFormatServisi.FormatTL(cost)} TL";
                bonusBuyConfirmPanel.SetActive(true);
                if (bonusBuyConfirmCanvasGroup != null) bonusBuyConfirmCanvasGroup.alpha = 1f;
                Debug.Log("[BONUS SATIN AL] Panel bonusBuyConfirmPanel referansı ile açıldı.");
                return;
            }
            var panelGO = GameObject.Find("BonusBuyConfirmPanel");
            if (panelGO == null) panelGO = GameObject.Find("BonusSatinAlOnayPanel");
            if (panelGO != null) { panelGO.SetActive(true); Debug.Log("[BONUS SATIN AL] Panel isimle bulundu ve açıldı."); return; }
            Debug.LogWarning("Bonus satın al onay UI bulunamadı. Sahnede BonusAyarlari bileşenli bir panel veya 'BonusBuyConfirmPanel' adlı GameObject ekleyin. Inspector'da OyunYoneticisi.bonusBuyConfirmPanel atayın.");
        });
        _bonusUIServisi.SetHideConfirmPanel(() =>
        {
            _bonusAyarlari?.Kapat();
            if (bonusBuyConfirmPanel != null) bonusBuyConfirmPanel.SetActive(false);
        });
        _bonusUIServisi.SetOnConfirmed(cost =>
        {
            _sonBonusSatinAlindiMaliyet = cost;
            _odemeServisi?.AddBahisToKasa(cost);
            int prevBakiye = _ekonomiServisi.Bakiye;
            _ekonomiServisi.SubtractBakiyeForBonusBuy(cost);
            _uiServisi?.UI_Guncelle();
            _logServisi?.KayitEkonomi("Bonus Satın Alındı", prevBakiye, _ekonomiServisi.Bakiye, cost, 0, "BONUS_BUY", $"Bonus satın alındı. Maliyet: {OyunFormatServisi.FormatTL(cost)}", cost);
            SenaryoYoneticisi.I?.BonusSatinAlindi();
            _donusServisi?.BaslatBonus();
        });

        _logServisi.SetFormatTL(OyunFormatServisi.FormatTL);
        _logServisi.SetOnSpinStart(() =>
        {
            if (GameManager.I != null && GameManager.I.ActivePlayer != null)
                GameManager.I.ActivePlayer.totalSpins += 1;
        });
        _logServisi.SetOnSpinResult((odenen, bahis) =>
        {
            if (GameManager.I != null && GameManager.I.ActivePlayer != null)
            {
                int net = odenen - bahis;
                GameManager.I.ActivePlayer.totalWon += odenen;
                if (net < 0) GameManager.I.ActivePlayer.totalLost += -net;
                GameManager.I.ActivePlayer.totalNet += net;
            }
        });
        _logServisi.SetOnSpinSettled(() => _uiServisi?.UI_Guncelle());

        _adminAyarUIServisi = new AdminAyarUIServisi();
        var zorlukSlider = GetComponentInChildren<Slider>(true);
        if (zorlukSlider != null && zorlukSlider.gameObject.name.ToLower().Contains("zorluk"))
            _adminAyarUIServisi.SetZorlukUI(zorlukSlider, zorlukValueText, v => _senaryoServisi?.SetZorluk(v));
        _adminAyarUIServisi.SetScatterUI(scatterSliderUI, scatterSliderText, v =>
        {
            // Slider 0-100 ise v=56 = %56; 0-1 ise v=0.56 = %56
            int yuzde;
            if (v > 1f)
            {
                yuzde = Mathf.Clamp(Mathf.RoundToInt(v), 0, 100);
                scatterChanceNormal = yuzde / 100f;
            }
            else
            {
                scatterChanceNormal = Mathf.Clamp01(v);
                yuzde = Mathf.RoundToInt(scatterChanceNormal * 100f);
            }
            scatterChanceBonus = 0f;
            if (yuzde >= 100 || scatterChanceNormal >= 0.99f)
                maxScatterPerSpin = 5;
            else if (scatterChanceNormal > 0.0001f)
                maxScatterPerSpin = Mathf.Max(maxScatterPerSpin, scatterEsik);
            UnityEngine.Debug.Log($"[SCATTER] Slider -> %{yuzde} (scatterChanceNormal={scatterChanceNormal:F2}), maxScatterPerSpin={maxScatterPerSpin}, esik={scatterEsik}");
        });
        // CarpanOlasilikValueText / CarpanMaxAdetValueText sabit etiket olarak kalacak; slider değeri yazılmıyor (valueText null).
        _adminAyarUIServisi.SetCarpanOlasilikUI(carpanOlasilikSlider, carpanOlasilikText, null, v =>
        {
            float yuzde = Mathf.Clamp(v, 0f, 100f);
            carpanUretimOlasiligi = yuzde / 100f;
        });
        _adminAyarUIServisi.SetCarpanMaxAdetUI(carpanMaxAdetSlider, carpanMaxAdetText, null, adet =>
        {
            maxCarpanAdedi = Mathf.Clamp(adet, 0, 5);
        });
        // Tek giriş: AdminPanel varsa slider'ları o bağlar; yoksa servis bağlar (çift bağlama yok).
        if (FindObjectOfType<AdminPanel>() == null)
            _adminAyarUIServisi.BindAllAndRefresh();

        // Inspector'u kalabalik yapmadan, bos kalmis UI alanlarini sahneden otomatik bul.
        // (BakiyeYukle / ParaCek / BonusSatinAl tiklanmiyor sorunu genelde referanslar null kaldiginda olur.)
        _uiServisi?.UIAutoBaglaGerekirse();

        // Çarpan ayarları panelindeki sabit etiket metinleri (slider ile değişmesin)
        if (carpanOlasilikValueText != null) carpanOlasilikValueText.text = "Çarpan Düşme Şansı (%)";
        if (carpanMaxAdetValueText != null) carpanMaxAdetValueText.text = "Max Kaç Çarpan Düşsün? (0-5)";

        UygulaCarpanAyarlari();
        if (carpanAktifToggle == null)
        {
            var toggles = FindObjectsByType<Toggle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in toggles)
            {
                if (t != null && t.gameObject.name.IndexOf("CarpanAktifToggle", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    carpanAktifToggle = t;
                    break;
                }
            }
        }
        if (carpanAktifToggle != null)
        {
            carpanAktifToggle.onValueChanged.RemoveAllListeners();
            carpanAktifToggle.onValueChanged.AddListener(_ => _uiServisi?.UI_Guncelle());
        }
        _carpanOverlayServisi?.SetCarpanSembolSprite(carpanSembolSprite);
        _carpanOverlayServisi?.SetOverlaySize(carpanOverlaySize);
        _carpanOverlayServisi?.SetOverlayFontSize(carpanOverlayFontSize);
        _carpanOverlayServisi?.SetOverlayTextOffset(carpanOverlayTextOffset);
        _carpanOverlayServisi?.SetDropStartYOffset(carpanOverlayDropStartYOffset);
        _carpanOverlayServisi?.SetDropDuration(carpanOverlayDropDuration);
        _izgaraServisi?.SetCarpanOverlayFontSize(carpanOverlayFontSize);
        _izgaraServisi?.SetCarpanYaziRengi(carpanYaziRengi);
        _izgaraServisi?.SetCarpanYaziKalin(carpanYaziKalin);
        _izgaraServisi?.SetCarpanYaziDisCizgiRengi(carpanYaziDisCizgiRengi);
        _izgaraServisi?.SetCarpanYaziDisCizgiKalinlik(carpanYaziDisCizgiKalinlik);
        _izgaraServisi?.SetCarpanYaziGolge(carpanYaziGolge);
        _izgaraServisi?.SetCarpanYaziGolgeRengi(carpanYaziGolgeRengi);
        _izgaraServisi?.SetCarpanYaziGolgeOffset(carpanYaziGolgeOffset);

        // BONUS SATIN AL ONAY
        if (bonusBuyYesButton != null)
        {
            bonusBuyYesButton.onClick.RemoveListener(OnBonusBuyYes);
            bonusBuyYesButton.onClick.AddListener(OnBonusBuyYes);
        }
        if (bonusBuyNoButton != null)
        {
            bonusBuyNoButton.onClick.RemoveListener(OnBonusBuyNo);
            bonusBuyNoButton.onClick.AddListener(OnBonusBuyNo);
        }

        // Panel ilk kapalı
        if (bonusBuyConfirmPanel != null)
            bonusBuyConfirmPanel.SetActive(false);

        if (bonusSatinAlButon != null)
        {
            bonusSatinAlButon.onClick.RemoveListener(BonusSatinAl);
            bonusSatinAlButon.onClick.AddListener(BonusSatinAl);
        }


        if (normalOyunMusic != null && !normalOyunMusic.isPlaying)
            normalOyunMusic.Play();

        _izgaraBaslatmaServisi = new IzgaraBaslatmaServisi();
        _izgaraBaslatmaServisi.SetBaglam(this);
        StartCoroutine(InitRoutine());
        if (bonusEndCloseButton != null)
        {
            bonusEndCloseButton.onClick.RemoveAllListeners();
            bonusEndCloseButton.onClick.AddListener(() => bonusEndCloseRequested = true);
        }
        // === BAHİS +/- BUTON BAĞLAMA === (OyunYoneticisi metotları: senaryo paneli + ekonomi güncellenir)
        if (bahisArttirButon != null)
        {
            bahisArttirButon.onClick.RemoveAllListeners();
            bahisArttirButon.onClick.AddListener(BahisArttir);
        }

        if (bahisAzaltButon != null)
        {
            bahisAzaltButon.onClick.RemoveAllListeners();
            bahisAzaltButon.onClick.AddListener(BahisAzalt);
        }
        Debug.Log($"[BAHIS HOOK] ArttirButon={(bahisArttirButon != null)} AzaltButon={(bahisAzaltButon != null)}");
        // Otomatik spin: panel kapalı, dropdown 20/50/100/250, butonlar
        if (otomatikSpinPanel != null)
            otomatikSpinPanel.SetActive(false);
        if (otomatikSpinDropdown != null)
        {
            OnOtomatikSpinDropdownChanged(otomatikSpinDropdown.value);
            otomatikSpinDropdown.onValueChanged.RemoveAllListeners();
            otomatikSpinDropdown.onValueChanged.AddListener(OnOtomatikSpinDropdownChanged);
        }
        if (otomatikSpinButton != null)
            otomatikSpinButton.onClick.AddListener(OnOtomatikSpinButtonClick);
        if (otomatikSpinBaslatButon != null)
            otomatikSpinBaslatButon.onClick.AddListener(OnOtomatikSpinBaslatClick);
        if (otomatikSpinIptalButon != null)
            otomatikSpinIptalButon.onClick.AddListener(OnOtomatikSpinIptalClick);
        if (istatistikButon != null)
            istatistikButon.onClick.AddListener(IstatistikButonTiklandi);
        OtomatikSpinKalanTextGuncelle();
        _uiServisi?.ResolveMoneyUIRefsIfMissing();
        _uiServisi?.WireParaCekUI();
        _uiServisi?.WireBakiyeYukleUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _boslukTusuBasiliSpin = true;
            SpinButon();
        }
        if (Input.GetKeyUp(KeyCode.Space))
            _boslukTusuBasiliSpin = false;
        if (_boslukTusuBasiliSpin && !spinCalisiyor && !bonusAktif && _ekonomiServisi != null && _ekonomiServisi.Bakiye >= _ekonomiServisi.Bahis)
            SpinButon();
    }

    private IEnumerator InitRoutine()
    {
        return _izgaraBaslatmaServisi != null ? _izgaraBaslatmaServisi.InitRoutine() : null;
    }

    int IIzgaraBaslatmaBaglami.GetSutun() => sutun;
    int IIzgaraBaslatmaBaglami.GetSatir() => satir;
    List<Sprite> IIzgaraBaslatmaBaglami.GetSembolSpriteListesi() => sembolSpriteListesi;
    IzgaraServisi IIzgaraBaslatmaBaglami.GetIzgaraServisi() => _izgaraServisi;
    TumbleServisi IIzgaraBaslatmaBaglami.GetTumbleServisi() => _tumbleServisi;
    UIServisi IIzgaraBaslatmaBaglami.GetUIServisi() => _uiServisi;
    EkonomiServisi IIzgaraBaslatmaBaglami.GetEkonomiServisi() => _ekonomiServisi;
    CarpanOverlayServisi IIzgaraBaslatmaBaglami.GetCarpanOverlayServisi() => _carpanOverlayServisi;
    AnimasyonServisi IIzgaraBaslatmaBaglami.GetAnimasyonServisi() => _animasyonServisi;
    Image[] IIzgaraBaslatmaBaglami.GetHucreler() => hucreler;
    void IIzgaraBaslatmaBaglami.SetHucreler(Image[] arr) => hucreler = arr;
    Transform IIzgaraBaslatmaBaglami.GetSlotGridRoot() => slotGridRoot;
    void IIzgaraBaslatmaBaglami.SetGrid(int[,] g) => grid = g;
    void IIzgaraBaslatmaBaglami.SetCarpanDegerGrid(int[,] g) => carpanDegerGrid = g;
    void IIzgaraBaslatmaBaglami.SetCarpanDegerByCellIndex(int[] a) => carpanDegerByCellIndex = a;
    void IIzgaraBaslatmaBaglami.SetCellPos(Vector2[] p) => cellPos = p;
    void IIzgaraBaslatmaBaglami.SetCellRT(RectTransform[] r) => cellRT = r;
    void IIzgaraBaslatmaBaglami.SetCarpanHücreTextleri(TextMeshProUGUI[] t) => carpanHücreTextleri = t;
    /// <param name="yetersizBakiyeUyarisi">true ise "Bakiye yetersiz. Bakiye azalıyor - yükleme yapmak ister misin?" metni kullanılır (spin/bahis için bakiye yetmediğinde).</param>
    public void ShowBakiyeYuklePanel(bool yetersizBakiyeUyarisi = false)
    {
        _uiServisi?.ResolveMoneyUIRefsIfMissing();
        _uiServisi?.WireBakiyeYukleUI();

        int kalanHak = _ekonomiServisi != null ? _ekonomiServisi.GetBakiyeYuklemeKalanHak() : 0;
        if (bakiyeYukleUyariText != null)
        {
            if (kalanHak <= 0)
                bakiyeYukleUyariText.text = "Bakiye yükleme hakkın kalmadı.";
            else if (yetersizBakiyeUyarisi)
                bakiyeYukleUyariText.text = $"Bakiye yetersiz. Bakiye azalıyor — 20.000 TL yükleme yapmak ister misin? (Kalan hak: {kalanHak})";
            else
                bakiyeYukleUyariText.text = $"20.000 TL yükleme yapmak ister misin? (Kalan hak: {kalanHak})";
        }
        _uiServisi?.CloseMoneyPanels();

        if (bakiyeYuklePanel != null)
            bakiyeYuklePanel.SetActive(true);
        SenaryoYoneticisi.I?.LogEkle(SenaryoOlayKaydi.OlayTipi_BakiyeYuklemeEkraniAcildi, "Bakiye yükleme ekranı açıldı. Kalan yükleme hakkı: " + kalanHak + "." + (yetersizBakiyeUyarisi ? " (Yetersiz bakiye uyarısı)" : ""));
    }

    public void HideBakiyeYuklePanel()
    {
        if (bakiyeYuklePanel != null)
            bakiyeYuklePanel.SetActive(false);
    }

    /// <summary>Az daha near-miss panelini gösterir, 2.5 saniye bekler, kapatır. Inspector'da azDahaPanel atanmışsa çalışır.</summary>
    public IEnumerator ShowAzDahaPanelAndWait()
    {
        if (azDahaPanel != null)
            azDahaPanel.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        if (azDahaPanel != null)
            azDahaPanel.SetActive(false);
    }

    public void HideAzDahaPanel()
    {
        if (azDahaPanel != null)
            azDahaPanel.SetActive(false);
    }

    public void BonusSatinAl()
    {
        if (_bonusAyarlari == null)
            _bonusAyarlari = FindFirstObjectByType<BonusAyarlari>(FindObjectsInactive.Include);
        Debug.Log($"[BONUS SATIN AL] Buton tıklandı. BonusAyarlari={(_bonusAyarlari != null)} Panel={(bonusBuyConfirmPanel != null)}");
        _bonusUIServisi?.BonusSatinAlRequested();
    }

    public void BonusSatinAlOnayla() => _bonusUIServisi?.OnYes();
    public void BonusSatinAlIptal() => _bonusUIServisi?.OnNo();

    private void ShowBonusBuyConfirmPanel(int cost) => _bonusUIServisi?.ShowBonusBuyConfirmPanel(cost);
    private void HideBonusBuyConfirmPanel() => _bonusUIServisi?.HideBonusBuyConfirmPanel();

    private void OnBonusBuyYes() => _bonusUIServisi?.OnYes();
    private void OnBonusBuyNo() => _bonusUIServisi?.OnNo();


   

    public void BahisArttir()
    {
        if (_ekonomiServisi == null) return;
        int onceki = _ekonomiServisi.Bahis;
        _ekonomiServisi.BahisArttir();
        if (_ekonomiServisi.Bahis > onceki)
            SenaryoYoneticisi.I?.BahisArtirimiYapildi();
        SenaryoYoneticisi.I?.UI_Guncelle();
    }
    public void BahisAzalt()
    {
        if (_ekonomiServisi == null) return;
        int onceki = _ekonomiServisi.Bahis;
        _ekonomiServisi.BahisAzalt();
        if (_ekonomiServisi.Bahis < onceki)
            SenaryoYoneticisi.I?.BahisArtirimiYapildi();
        SenaryoYoneticisi.I?.UI_Guncelle();
    }

    /// <summary>Senaryolu oyun paneli gibi dış okumalar için: mevcut bahis (TL). Admin panel yok; değer config/sahneden gelir.</summary>
    public int GetMevcutBahis() => _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0;

    /// <summary>Senaryolu oyun paneli için: bu spin için ödenebilir üst limit (havuzun %10'u). Her spin sonrası güncellenebilir.</summary>
    public int GetSpinOdenebilirLimit() => _odemeServisi != null ? _odemeServisi.GetSpinOdenebilirLimit() : 0;

    /// <summary>Override olmadan havuzun %10'u; bonus içinde panelde güncel ödenebilir tutarı göstermek için.</summary>
    public int GetSpinOdenebilirLimitRaw() => _odemeServisi != null ? _odemeServisi.GetSpinOdenebilirLimitRaw() : 0;

    /// <summary>Senaryolu oyunda ödenebilir tutarı sabit TL yap (örn. 100000). null = normale dön (havuz %10).</summary>
    public void SetOdenebilirTutarOverride(int? tl) => _odemeServisi?.SetOdenebilirLimitOverride(tl);

    private const string PP_SENARYO_ODENEBILIR_KALAN = "PP_SENARYO_ODENEBILIR_KALAN_TL";
    private string SenaryoOdenebilirKey()
    {
        string id = GameManager.I?.ActivePlayer?.playerId ?? "";
        return string.IsNullOrEmpty(id) ? "" : (PP_SENARYO_ODENEBILIR_KALAN + "_" + id);
    }

    /// <summary>Senaryo sahnesinde: ödenebilir bütçe 100k ile başlar; ödedikçe azalır, ödemedikçe (bahis eve kalınca) artar.</summary>
    public void SenaryoOdenebilirBütceBaslat(int baslangicTL)
    {
        _senaryoOdenebilirKalanTL = Mathf.Max(0, baslangicTL);
    }

    /// <summary>Kaydedilmiş ödenebilir bütçeyi yükler (kullanıcı bazlı); yoksa veya geçersizse varsayilanTL ile başlatır.</summary>
    public void SenaryoOdenebilirBütceYükleVeyaBaslat(int varsayilanTL)
    {
        string key = SenaryoOdenebilirKey();
        if (!string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key))
        {
            int kayitli = PlayerPrefs.GetInt(key, -1);
            if (kayitli > 0)
            {
                _senaryoOdenebilirKalanTL = kayitli;
                return;
            }
        }
        _senaryoOdenebilirKalanTL = Mathf.Max(0, varsayilanTL);
    }

    /// <summary>Uygulama kapanırken veya sahne değişince senaryo ödenebilir bütçesini kaydet (kullanıcı bazlı).</summary>
    private void OnApplicationQuit() => SenaryoOdenebilirBütceKaydet();
    private void OnDestroy() => SenaryoOdenebilirBütceKaydet();

    private void SenaryoOdenebilirBütceKaydet()
    {
        if (_senaryoOdenebilirKalanTL >= 0)
        {
            string key = SenaryoOdenebilirKey();
            if (!string.IsNullOrEmpty(key))
            {
                PlayerPrefs.SetInt(key, _senaryoOdenebilirKalanTL);
                PlayerPrefs.Save();
            }
        }
    }

    /// <summary>Spin sonrası çağrılır: ödeme yapıldıysa bütçeden düş, yapılmadıysa bahisi bütçeye ekle (sadece senaryo sahnesinde).</summary>
    public void SenaryoOdenebilirGuncelle(int odenen, int bahis)
    {
        if (SenaryoYoneticisi.I == null || _senaryoOdenebilirKalanTL < 0) return;
        if (odenen > 0)
            _senaryoOdenebilirKalanTL -= odenen;
        else if (bahis > 0)
            _senaryoOdenebilirKalanTL += bahis;
        _senaryoOdenebilirKalanTL = Mathf.Max(0, _senaryoOdenebilirKalanTL);
    }

    // ==========================
    // BUTTON / SPIN
    // ==========================
    
    // YENİ: Spin başında ödenebilir tutarı hesapla
    private int _spinOdenebilirLimit = 0;
    
    public void SpinButon()
    {
        _donusServisi?.SpinButon();
    }

    /// <summary>Sentetik oyuncu / bot test katmanının spin ve bonus durumunu okuması için.</summary>
    public bool SpinCalisiyorMu => spinCalisiyor;
    /// <summary>Sentetik oyuncu / bot test katmanının bonus durumunu okuması için.</summary>
    public bool BonusAktifMi => bonusAktif;
    /// <summary>Bot: bakiye (dönüş atılabilir mi kontrolü).</summary>
    public int BotIcinBakiye => _ekonomiServisi != null ? _ekonomiServisi.Bakiye : 0;
    /// <summary>Bot: mevcut bahis.</summary>
    public int BotIcinBahis => _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0;

    private void SpinButonImpl()
    {
        if (bonusAktif) return;
        if (spinCalisiyor) return;

        if (_ekonomiServisi.Bakiye < _ekonomiServisi.Bahis)
        {
            // Bakiye yetersiz: paneli aç, "Bakiye azalıyor, yükleme yapmak ister misin?" uyarısı göster.
            ShowBakiyeYuklePanel(yetersizBakiyeUyarisi: true);
            return;
        }
        spinCalisiyor = true;
        StartCoroutine(BirSpinHazirlaVeAt());
    }

    /// <summary>Tek bir spin için bahis düşümü + kayıt + NormalSpinAkisi. Otomatik spin döngüsü her turda bunu çağırır.</summary>
    private IEnumerator BirSpinHazirlaVeAt()
    {
        _spinOdenebilirLimit = _odemeServisi != null ? _odemeServisi.GetSpinOdenebilirLimit() : int.MaxValue;
        _spinPrevBakiye = _ekonomiServisi.Bakiye;
        _spinBahisTL = _ekonomiServisi.Bahis;
        _odemeServisi?.AddBahisToKasa(_ekonomiServisi.Bahis);
        _ekonomiServisi.DeductBet();
        _logServisi?.RecordSpinStart(_spinPrevBakiye, _ekonomiServisi.Bakiye, _spinBahisTL, _spinOdenebilirLimit);
        _uiServisi?.UI_Guncelle();
        yield return _donusServisi.NormalSpinAkisi();
    }

    private IEnumerator OtomatikSpinDongusu()
    {
        while (_otomatikSpinKalan > 0 && !bonusAktif && _ekonomiServisi != null && _ekonomiServisi.Bakiye >= _ekonomiServisi.Bahis)
        {
            OtomatikSpinKalanTextGuncelle();
            yield return BirSpinHazirlaVeAt();
            _otomatikSpinKalan--;
        }
        // Bonus tetiklenince döngüden çıkıyoruz; kalan sayıyı SIFIRLAMA ki panel 5 sn kapansın ve bonus bitince spin devam etsin.
        if (!bonusAktif)
            _otomatikSpinKalan = 0;
        OtomatikSpinKalanTextGuncelle();
    }

    /// <summary>Otomatik spin sırasında 'Durdur' butonu veya dışarıdan çağrı ile kalan sayıyı sıfırlar; mevcut spin biter, sonra döngü durur.</summary>
    public void OtomatikSpinDurdur()
    {
        _otomatikSpinKalan = 0;
        OtomatikSpinKalanTextGuncelle();
    }

    private void OtomatikSpinKalanTextGuncelle()
    {
        if (otomatikSpinKalanText != null)
        {
            if (_otomatikSpinKalan > 0 && !bonusAktif)
            {
                otomatikSpinKalanText.text = $"Kalan Spin: {_otomatikSpinKalan}";
                otomatikSpinKalanText.gameObject.SetActive(true);
            }
            else
                otomatikSpinKalanText.gameObject.SetActive(false);
        }
        if (otomatikSpinButton != null)
        {
            var tmp = otomatikSpinButton.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
                tmp.text = _otomatikSpinKalan > 0 ? "DURDUR" : (string.IsNullOrEmpty(otomatikSpinButtonNormalText) ? "Otomatik Spin" : otomatikSpinButtonNormalText);
        }
    }

    private void OnOtomatikSpinDropdownChanged(int index)
    {
        int[] secenekler = { 20, 50, 100, 250 };
        _otomatikSpinSecilenAdet = (index >= 0 && index < secenekler.Length) ? secenekler[index] : 20;
    }

    /// <summary>OtomatikSpinButton tıklanınca: spin dönüyorsa durdur, değilse paneli aç.</summary>
    private void OnOtomatikSpinButtonClick()
    {
        if (_otomatikSpinKalan > 0)
        {
            OtomatikSpinDurdur();
            return;
        }
        if (otomatikSpinPanel != null)
        {
            otomatikSpinPanel.SetActive(true);
            // Panel diğer panellerin üstünde görünsün (sıra: en son = en üstte)
            if (otomatikSpinPanel.transform.parent != null)
            {
                otomatikSpinPanel.transform.SetAsLastSibling();
                otomatikSpinPanel.transform.parent.SetAsLastSibling();
            }
        }
    }

    /// <summary>Paneldeki Baslat: seçilen adet ile döngüyü başlat, paneli kapat.</summary>
    private void OnOtomatikSpinBaslatClick()
    {
        if (otomatikSpinPanel != null)
            otomatikSpinPanel.SetActive(false);
        if (bonusAktif || spinCalisiyor) return;
        if (_ekonomiServisi == null || _ekonomiServisi.Bakiye < _ekonomiServisi.Bahis)
        {
            // Bakiye yetersiz: bakiye yükle panelini "bakiye azalıyor, yükleme yapmak ister misin?" ile aç.
            ShowBakiyeYuklePanel(yetersizBakiyeUyarisi: true);
            return;
        }
        if (otomatikSpinDropdown != null)
            OnOtomatikSpinDropdownChanged(otomatikSpinDropdown.value);
        _otomatikSpinKalan = _otomatikSpinSecilenAdet;
        OtomatikSpinKalanTextGuncelle();
        StartCoroutine(OtomatikSpinDongusu());
    }

    /// <summary>Paneldeki İptal: paneli kapat, spin başlatma.</summary>
    private void OnOtomatikSpinIptalClick()
    {
        if (otomatikSpinPanel != null)
            otomatikSpinPanel.SetActive(false);
    }

    private void IstatistikButonTiklandi()
    {
        if (GameManager.I != null)
            GameManager.I.LoadScene("04_LogScane");
    }

    /// <summary>Bonus bittikten sonra DonusAkisServisi tarafından çağrılır; kalan otomatik spin varsa döngüyü yeniden başlatır.</summary>
    private void TryResumeOtomatikSpin()
    {
        if (_otomatikSpinKalan <= 0 || bonusAktif) return;
        if (_ekonomiServisi == null || _ekonomiServisi.Bakiye < _ekonomiServisi.Bahis) return;
        OtomatikSpinKalanTextGuncelle();
        StartCoroutine(OtomatikSpinDongusu());
    }

    private void BaslatBonus()
    {
        // Normal spinden bonusa geçişte input kilidini bırak
        spinCalisiyor = false;

if (spinIcon != null) spinIcon.SetRotate(false);

        if (normalOyunMusic != null && normalOyunMusic.isPlaying)
            normalOyunMusic.Pause();

        if (bonusAktif) return;

        bonusAktif = true;
        SenaryoYoneticisi.I?.BonusGoruldu();
        SenaryoYoneticisi.I?.SetBonusAktif(true);
        int bonusGirisBakiyesi = _ekonomiServisi != null ? _ekonomiServisi.Bakiye : 0;
        SenaryoYoneticisi.I?.LogBonusGirisi(bonusGirisBakiyesi, _sonBonusSatinAlindiMaliyet > 0);
        if (otomatikSpinKalanText != null)
            otomatikSpinKalanText.gameObject.SetActive(false);
        _spinKazanciOturumaEklendi = false;

        _hizVeSesServisi?.ApplyBonusSpeedIfNeeded();
        oturumKazanc = 0;
        _bonusOturumOdenenToplamTL = 0;
        _bonusPendingOdemeTL = 0;
        _bonusZorlaCarpanBirikenTL = 0;

        _senaryoServisi?.InitBonusBudgetFromHavuz(_odemeServisi != null ? _odemeServisi.GetHavuzTL() : 0L);

        _bonusSatınAlindiSenaryo1 = false;
        _senaryo1BonusAktif = false;
        int bahis = _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0;
        int maliyetVarsayilan = Mathf.Max(0, bahis) * Mathf.Max(1, bonusSatinAlCarpani);
        int maliyet = _sonBonusSatinAlindiMaliyet > 0 ? _sonBonusSatinAlindiMaliyet : maliyetVarsayilan;

        if (SenaryoYoneticisi.I != null && maliyet > 0)
        {
            var asama = SenaryoYoneticisi.I.mevcutAsama;
            int yukleme = SenaryoYoneticisi.I.yuklemeSayisi;
            bool satinAlindi = _sonBonusSatinAlindiMaliyet > 0;
            if (satinAlindi) _sonBonusSatinAlindiMaliyet = 0;

            int cap = 0;
            switch (asama)
            {
                case SenaryoYoneticisi.SenaryoAsama.Asama1_IsindirmaUmut:
                    if (satinAlindi) { _bonusSatınAlindiSenaryo1 = true; cap = (int)(maliyet * 1.30f); }
                    else cap = (int)(maliyet * Random.Range(0.10f, 0.30f));
                    break;
                case SenaryoYoneticisi.SenaryoAsama.Asama2_KontrolBende:
                    // Spec: satın alındı → en fazla maliyet + %10; normal spinden tetiklenirse maliyetin %10'unu geçmemeli.
                    if (satinAlindi) cap = (int)(maliyet * 1.10f);
                    else cap = (int)(maliyet * 0.10f);
                    break;
                case SenaryoYoneticisi.SenaryoAsama.Asama3_AzDahaKayipKovalama:
                    if (satinAlindi) cap = (int)(maliyet * 0.80f);
                    else cap = Mathf.Max(100, (int)(bahis * 0.30f));
                    break;
                case SenaryoYoneticisi.SenaryoAsama.Asama4_BakiyeTukenis:
                    if (satinAlindi) cap = (int)(maliyet * 0.50f);
                    else cap = Mathf.Max(100, (int)(bahis * 0.20f));
                    break;
                case SenaryoYoneticisi.SenaryoAsama.Asama5_BonusZirve:
                    if (yukleme >= 3 && !_ucuncuYuklemeSonrasiIlkBonusUygulandi)
                    {
                        cap = (int)(maliyet * 2.5f);
                        _ucuncuYuklemeSonrasiIlkBonusUygulandi = true;
                        _buBonusZirveBonusuMu = true;
                    }
                    else
                        cap = (int)(maliyet * 0.20f);
                    break;
                case SenaryoYoneticisi.SenaryoAsama.Asama6_GercekKayip:
                    // Gerçek kayıp aşamasında bonuslar kurtarıcı değil: büyük kazanç kapalı, çoğu bonus zarar veya maliyetin altında.
                    cap = Mathf.Max(50, (int)(bahis * 0.5f));
                    break;
                case SenaryoYoneticisi.SenaryoAsama.Asama7_Finale:
                    cap = 0;
                    break;
                default:
                    break;
            }
            if (cap >= 0)
            {
                if (cap > 0 && cap < 50) cap = 50;
                _bonusBudgetKalanTL = cap;
                _bonusMaxOdemeTL = cap;
                _senaryo1BonusAktif = true;
            }
        }

        bonusHakKalan = bonusHakBaslangic;
        if (_buBonusZirveBonusuMu && senaryo5_zirveBonusSpinSayisi > 0)
        {
            bonusHakKalan = senaryo5_zirveBonusSpinSayisi;
            OnZirveBonusBasladi?.Invoke();
        }
        bonusKazanc = 0;
        _bonusZorlaCarpanBirikenTL = 0;

        _uiServisi?.ButonDurumu(false);
        _uiServisi?.UI_Guncelle();

        StartCoroutine(_donusServisi.BonusBaslangicAkisi());

    }
    private IEnumerator BonusBaslangicAkisi()
    {
        // Bonus mesajını göster
        yield return StartCoroutine(_donusServisi.ShowBonusStartMessage());

        // Sonra free spin döngüsüne gir
        yield return StartCoroutine(_donusServisi.BonusDongusu());
    }

    private IEnumerator ShowBonusStartMessage()
    {
        yield return StartCoroutine(_bonusUIServisi.ShowBonusStartMessage());
    }

    private IEnumerator ShowBonusEndMessage(int bonusToplamKazanc)
    {
        if (bonusEndPanel == null) yield break;

        if (_buBonusZirveBonusuMu)
        {
            OnZirveBonusBitti?.Invoke(bonusToplamKazanc);
            _buBonusZirveBonusuMu = false;
        }

        // DonusAkisServisi zaten BonusOturumOdenenToplamTL'yi bakiyeye ekledi. Burada sadece zorla çarpan birikimini ekleyip temizliyoruz; PayFromHavuz ile tekrar ekleme yapılmaz (çift ekleme önlenir).
        int prevBakiye = _ekonomiServisi != null ? _ekonomiServisi.Bakiye : 0;
        if (_ekonomiServisi != null && _bonusZorlaCarpanBirikenTL > 0)
        {
            _ekonomiServisi.AddWinnings(_bonusZorlaCarpanBirikenTL, 0);
            _uiServisi?.UI_Guncelle();
        }
        int toplamEklendi = _bonusOturumOdenenToplamTL + _bonusZorlaCarpanBirikenTL;
        _logServisi?.RecordBonusEnd(prevBakiye, _ekonomiServisi != null ? _ekonomiServisi.Bakiye : 0, toplamEklendi);
        _bonusPendingOdemeTL = 0;
        _bonusZorlaCarpanBirikenTL = 0;

        // Panelde gösterilecek değer: bu bonusta bakiyeye eklenen toplam (bonusToplamKazanc = OturumKazanc zaten bu toplama eşit)
        yield return StartCoroutine(_bonusUIServisi.ShowBonusEndMessage(bonusToplamKazanc));
    }



// ==========================
    // TUMBLE
    // ==========================
    private int GetBonusRemainingPayableTL()
    {
        if (!bonusAktif) return int.MaxValue;

        // Cap: bonus başlangıcında havuz snapshot'ının belirli oranı
        int cap = (_bonusMaxOdemeTL > 0) ? _bonusMaxOdemeTL : int.MaxValue;

        long kalan = (long)cap - (long)bonusKazanc; // bonusKazanc = şu ana kadar ÖDENEN toplam (pending dahil sayılır)
        if (kalan < 0) kalan = 0;

        // Budget aktifse onu da dikkate al
        if (bonusBudgetAktif)
        {
            long bk = _bonusBudgetKalanTL;
            if (bk < kalan) kalan = bk;
        }

        // Senaryo 1 bonusunda (scatter/satın al) sadece hesaplanan cap geçerli; havuz ve ödenebilir tavan uygulanmaz
        if (!_senaryo1BonusAktif)
        {
            long havuzSimdi = _odemeServisi != null ? _odemeServisi.GetHavuzTL() : long.MaxValue;
            if (havuzSimdi < long.MaxValue)
            {
                long poolCap = (long)Mathf.Floor((float)havuzSimdi * 0.10f);
                if (poolCap < kalan) kalan = poolCap;
            }
            if (SenaryoYoneticisi.I != null && _senaryoOdenebilirKalanTL >= 0 && _senaryoOdenebilirKalanTL < kalan)
                kalan = _senaryoOdenebilirKalanTL;
        }

        if (kalan > int.MaxValue) return int.MaxValue;
        return (int)kalan;
    }

    /// <summary>SenaryoServisi delegasyonu için: bonus bütçe/cap alanlarını havuz değerine göre başlatır.</summary>
    private void InitBonusBudgetFromHavuz(long odulHavuzuTL)
    {
        if (bonusBudgetAktif)
        {
            long havuz = odulHavuzuTL;
            int hedef = Mathf.RoundToInt((float)havuz * bonusBudgetHavuzOran);
            hedef = Mathf.Clamp(hedef, bonusBudgetMinTL, bonusBudgetMaxTL);
            if (hedef > havuz) hedef = (int)Mathf.Clamp((float)havuz, 0, int.MaxValue);
            _bonusBudgetKalanTL = hedef;
            Debug.Log($"[BONUS BUDGET] Bonus başı bütçe: {_bonusBudgetKalanTL} TL (Havuz={havuz})");
        }
        else
            _bonusBudgetKalanTL = int.MaxValue;

        _bonusBaslangicHavuzTL = odulHavuzuTL;
        if (bonusMaxOdemeHavuzOrani <= 0f)
            _bonusMaxOdemeTL = int.MaxValue;
        else
        {
            float cap = (float)_bonusBaslangicHavuzTL * bonusMaxOdemeHavuzOrani;
            _bonusMaxOdemeTL = cap > int.MaxValue ? int.MaxValue : Mathf.Max(0, Mathf.RoundToInt(cap));
        }
        _bonusOdenenTL = 0;
        Debug.Log($"[BONUS CAP] HavuzSnapshot={_bonusBaslangicHavuzTL} TL | CapOran={bonusMaxOdemeHavuzOrani} | CapTL={_bonusMaxOdemeTL}");
    }

    /// <summary>SenaryoServisi delegasyonu için: ödenen tutarı kaydeder (_bonusOdenenTL ve _bonusBudgetKalanTL günceller).</summary>
    private void RecordBonusPayment(int odenenTL)
    {
        if (odenenTL > 0) _bonusOdenenTL = Mathf.Clamp(_bonusOdenenTL + odenenTL, 0, int.MaxValue);
        if (bonusBudgetAktif && _bonusBudgetKalanTL != int.MaxValue)
        {
            _bonusBudgetKalanTL -= odenenTL;
            if (_bonusBudgetKalanTL < 0) _bonusBudgetKalanTL = 0;
        }

        // Invariant: Bonus toplam ödemesi tanımlı tavanı aşmamalı.
        if (_bonusMaxOdemeTL != int.MaxValue && _bonusOdenenTL > _bonusMaxOdemeTL)
        {
            var senaryo = SenaryoYoneticisi.I;
            int spinNo = senaryo != null ? senaryo.toplamSpin : -1;
            string asamaAdi = senaryo != null ? senaryo.GetAsamaAdi() : "Bilinmiyor";
            Debug.LogError($"[SentetikOyuncu][İHLAL] Bonus toplam ödemesi tavanı aştı. OdenenToplam={_bonusOdenenTL} TL, Cap={_bonusMaxOdemeTL} TL, SpinNo={spinNo}, Asama={asamaAdi}");
        }
    }

    private IEnumerator CollapseRefillAndAnimate()
    {
        return _cokmeAkisServisi != null ? _cokmeAkisServisi.CokmeDoldurVeCanlandir() : null;
    }

    private const int SIMULASYON_MAX_REROLL = 200;
    private const int SIMULASYON_MAX_REROLL_ZORLA_CARPAN_TUMBLE = 600;

    private SpinSimulasyonKaydi SimuleEtVeKaydetImpl(int odenebilirLimit, bool bonusSpin)
    {
        if (!bonusSpin)
        {
            int bakiye = _ekonomiServisi != null ? _ekonomiServisi.Bakiye : 0;
            if (bakiye >= 50000 && _bakiye50KUstundeTumbleKapaliKalanSpin == 0)
                _bakiye50KUstundeTumbleKapaliKalanSpin = 20;
        }

        int limit = bonusSpin ? (_senaryoServisi != null ? _senaryoServisi.GetBonusRemainingPayableTL() : int.MaxValue) : odenebilirLimit;
        int zorlaCarpanDegeri = zorlaSiradakiCarpan;
        // Toggle SADECE zorla çarpan (5x/10x/50x/100x) butonuna basıldığında kullanılır; normal/bonus oyununda butona basılmadıysa toggle'ın hiçbir etkisi yoktur.
        bool carpanToggleSecili = (zorlaCarpanDegeri > 0 && carpanAktifToggle != null && carpanAktifToggle.isOn);
        if (zorlaCarpanDegeri > 0)
            limit = int.MaxValue;
        int maxReroll = (zorlaCarpanDegeri > 0 && carpanToggleSecili) ? SIMULASYON_MAX_REROLL_ZORLA_CARPAN_TUMBLE : SIMULASYON_MAX_REROLL;
        bool zorunluBosSpin = !bonusSpin && SenaryoYoneticisi.I != null && SenaryoYoneticisi.I.ShouldForceNoPaySenaryo12();
        if (zorunluBosSpin) maxReroll = Mathf.Max(maxReroll, 400);
        SpinSimulasyonKaydi sonKayit = null;
        SpinSimulasyonKaydi sonDenemeKayit = null;

        for (int deneme = 0; deneme < maxReroll; deneme++)
        {
            bool ucScatterAzDaha = false;
            zorlaSiradakiCarpan = zorlaCarpanDegeri;
            UI_CarpanSifirla();
            _izgaraServisi?.ResetScatterCountPerSpin();
            spinKazancHam = 0;

            int fillLimit = odenebilirLimit;
            if (zorlaCarpanDegeri > 0 && !carpanToggleSecili)
                fillLimit = 0;
            _izgaraServisi?.FillRandomAll(fillLimit);
            // Senaryo: Aşama 1 → 50 spinde 4 scatter garantisi; Aşama 2 → 75 spinde 4 scatter; Aşama 3 → near-miss (tam 3 scatter sık)
            if (!bonusSpin && SenaryoYoneticisi.I != null)
            {
                var a = SenaryoYoneticisi.I.mevcutAsama;
                int since = SenaryoYoneticisi.I.SpinsSinceLastScatter();
                if (a == SenaryoYoneticisi.SenaryoAsama.Asama3_AzDahaKayipKovalama && since >= 8 && since <= 50 && Random.value < 0.4f)
                {
                    GrideTamUcScatterKoy();
                    ucScatterAzDaha = true;
                }
                else if ((a == SenaryoYoneticisi.SenaryoAsama.Asama1_IsindirmaUmut && since >= 50) ||
                    (a == SenaryoYoneticisi.SenaryoAsama.Asama2_KontrolBende && since >= 75))
                    GrideEnAzDortScatterKoy();
            }
            CarpanUretVeBirik();
            CarpanlariDoluGriddeUygula();

            // Senaryo 3 "Az daha oluyordu": bonus + zorla 100x+ iken tumble hiç olmasın; sadece büyük çarpanlar düşsün. Cluster zorlama yapma, varsa kümeleri kır.
            bool azDahaNearMiss = bonusSpin && SenaryoYoneticisi.I != null && SenaryoYoneticisi.I.mevcutAsama == SenaryoYoneticisi.SenaryoAsama.Asama3_AzDahaKayipKovalama && zorlaCarpanDegeri >= 100;
            if (zorlaCarpanDegeri > 0 && carpanToggleSecili && !azDahaNearMiss)
            {
                GrideZorlaEnAzBirCluster();
                _tumbleServisi?.SetGrid(grid);
            }
            if (azDahaNearMiss)
                GrideKazancsizYap(); // Kazançlı kümeleri kır; böylece tumble hiç tetiklenmez.

            var kayit = new SpinSimulasyonKaydi { Sutun = sutun, Satir = satir };
            sonDenemeKayit = kayit;
            kayit.IlkGrid = (int[,])grid.Clone();
            kayit.IlkCarpanGrid = (int[,])carpanDegerGrid.Clone();
            kayit.IlkCarpanDegerleri.Clear();
            for (int x = 0; x < sutun; x++)
                for (int y = 0; y < satir; y++)
                    if (grid[x, y] == CARPAN_SEMBOL && carpanDegerGrid[x, y] != 0)
                        kayit.IlkCarpanDegerleri.Add(carpanDegerGrid[x, y]);

            // Bakiye ≥ 50.000 TL görüldüğünde 20 spin boyunca tumble imkansız
            if (!bonusSpin && _bakiye50KUstundeTumbleKapaliKalanSpin > 0)
                GrideKazancsizYap();

            int turSayaci = 0;
            bool limitAsildi = false;
            while (turSayaci < OyunKorumaServisi.MAX_TUMBLE_TUR)
            {
                var toRemove = _tumbleServisi != null ? _tumbleServisi.FindClustersToRemove(minClusterSize) : new List<Vector2Int>();
                if (toRemove == null || toRemove.Count == 0) break;

                CarpanUretVeBirik();
                int turHam = tumbleAyarlari != null ? tumbleAyarlari.CalculateWinWithOwnPayTable(toRemove, grid, satir, sutun, _ekonomiServisi != null ? _ekonomiServisi.Bahis : 0, minClusterSize) : 0;
                int turKazanc = ZorlukKazancCarpaniUygula(turHam);
                if (bonusSpin && zorlaCarpanDegeri <= 0 && _senaryoServisi != null)
                {
                    int kalan = _senaryoServisi.GetBonusRemainingPayableTL();
                    int m = _carpanServisi != null ? _carpanServisi.GetCurrentMultiplierInt() : 1;
                    if (m < 1) m = 1;
                    long proj = (long)(spinKazancHam + turKazanc) * (long)m;
                    if (kalan <= 0 || proj > (long)kalan)
                    {
                        limitAsildi = true;
                        break;
                    }
                }
                spinKazancHam += turKazanc;

                var adim = new TumbleAdimKaydi { TurKazanci = turKazanc };
                adim.PatlayanHucreler.AddRange(toRemove);

                GridHucreleriniTemizle(toRemove);
                if (_cokmeAkisServisi != null)
                    _cokmeAkisServisi.CokmeDoldurSadeceMantik(adim);
                kayit.Adimlar.Add(adim);
                turSayaci++;
            }

            if (limitAsildi)
                continue;

            int toplamCarpan = _carpanServisi != null ? _carpanServisi.GetTotalMultiplierForSpin() : 1;
            int nihaiOdeme = _carpanServisi != null ? _carpanServisi.MulClampInt(spinKazancHam, toplamCarpan) : spinKazancHam;
            kayit.ToplamHamKazanc = spinKazancHam;
            kayit.NihaiCarpanToplam = toplamCarpan;

            if (limit != int.MaxValue && nihaiOdeme > limit)
                continue;
            // Senaryo 1: 4 üst üste ödeme sonrası 3 spin zorunlu boş (ödeme yok)
            if (!bonusSpin && SenaryoYoneticisi.I != null && SenaryoYoneticisi.I.ShouldForceNoPaySenaryo12() && nihaiOdeme > 0)
                continue;
            // Zorla çarpan varken diğer "min tumble / min ödeme" kuralları uygulanmaz; sadece toggle kuralı geçerli.
            bool zorlaCarpanVardi = zorlaCarpanDegeri > 0;
            if (!zorlaCarpanVardi)
            {
                // Zorluk 4 (kolay) + bonus: ödenebilir limite yakın tumble iste; ilk düşük sonucu kabul etme.
                if (bonusSpin && limit > 100 && _easyBias01 > 0.3f)
                {
                    int minHedef = Mathf.RoundToInt(limit * 0.25f);
                    if (minHedef > 0 && nihaiOdeme < minHedef)
                        continue;
                }
                // Zorluk 4 (kolay) normal spin: limit makulken limit altında en az bir tumble olsun diye tumble'sız spin'i reddet (re-roll).
                if (!bonusSpin && _easyBias01 > 0.3f && limit >= 50 && kayit.Adimlar != null && kayit.Adimlar.Count == 0)
                    continue;
            }
            // CarpanAktifToggle: seçiliyse zorla çarpanla mutlaka tumble, seçili değilse mutlaka tumble olmasın. "Az daha" senaryosunda tumble asla olmasın.
            int adimSayisi = kayit.Adimlar != null ? kayit.Adimlar.Count : 0;
            if (azDahaNearMiss && adimSayisi > 0)
                continue; // Tumble olan rolleri reddet; sadece büyük çarpanlı, tumble'sız sonuç kabul et.
            if (zorlaCarpanVardi)
            {
                if (carpanToggleSecili && adimSayisi == 0)
                    continue;
                if (!carpanToggleSecili && adimSayisi > 0)
                    continue;
            }
            // Senaryo 3 "Az daha oluyordu": bonus oyunda 100x/250x/500x görünsün, tumble olmasın, ödeme 0.
            if (azDahaNearMiss)
                kayit.ToplamHamKazanc = 0;
            kayit.AzDahaNearMiss = azDahaNearMiss || ucScatterAzDaha;
            sonKayit = kayit;
            kayit.ZorlaCarpanKullanildi = zorlaCarpanDegeri > 0;
            if (!bonusSpin && _bakiye50KUstundeTumbleKapaliKalanSpin > 0) _bakiye50KUstundeTumbleKapaliKalanSpin--;
            return kayit;
        }

        // Senaryo 1 zorunlu boş spin: 400 denemede 0 gelmediyse gridi kazançsız yapıp garanti 0 ödeme döndür
        if (zorunluBosSpin && sonKayit == null && sonDenemeKayit != null)
        {
            zorlaSiradakiCarpan = zorlaCarpanDegeri;
            UI_CarpanSifirla();
            _izgaraServisi?.ResetScatterCountPerSpin();
            
            spinKazancHam = 0;
            int fillLimit = odenebilirLimit;
            if (zorlaCarpanDegeri > 0 && !carpanToggleSecili) fillLimit = 0;
            _izgaraServisi?.FillRandomAll(fillLimit);
            CarpanUretVeBirik();
            CarpanlariDoluGriddeUygula();
            GrideKazancsizYap();
            var zorlaKayit = new SpinSimulasyonKaydi { Sutun = sutun, Satir = satir };
            zorlaKayit.IlkGrid = (int[,])grid.Clone();
            zorlaKayit.IlkCarpanGrid = (int[,])carpanDegerGrid.Clone();
            zorlaKayit.IlkCarpanDegerleri.Clear();
            for (int x = 0; x < sutun; x++)
                for (int y = 0; y < satir; y++)
                    if (grid[x, y] == CARPAN_SEMBOL && carpanDegerGrid[x, y] != 0)
                        zorlaKayit.IlkCarpanDegerleri.Add(carpanDegerGrid[x, y]);
            zorlaKayit.ToplamHamKazanc = 0;
            zorlaKayit.NihaiCarpanToplam = 1;
            zorlaKayit.ZorlaCarpanKullanildi = false;
            if (!bonusSpin && _bakiye50KUstundeTumbleKapaliKalanSpin > 0) _bakiye50KUstundeTumbleKapaliKalanSpin--;
            return zorlaKayit;
        }

        // Senaryo 3 "Az daha oluyordu" fallback: döngüden çıkıp sonDenemeKayit kullanılıyorsa, bonus + zorla 100x+ ise ödeme 0.
        if (sonKayit == null && sonDenemeKayit != null
            && bonusSpin
            && SenaryoYoneticisi.I != null
            && SenaryoYoneticisi.I.mevcutAsama == SenaryoYoneticisi.SenaryoAsama.Asama3_AzDahaKayipKovalama
            && zorlaCarpanDegeri >= 100)
        {
            sonKayit = sonDenemeKayit;
            sonKayit.ToplamHamKazanc = 0;
            sonKayit.AzDahaNearMiss = true;
        }

        if (sonKayit == null && sonDenemeKayit != null)
            sonKayit = sonDenemeKayit;
        if (sonKayit != null && !bonusSpin && _bakiye50KUstundeTumbleKapaliKalanSpin > 0) _bakiye50KUstundeTumbleKapaliKalanSpin--;
        return sonKayit;
    }

    /// <summary>Senaryo 1: 50 spin sonrası scatter garantisi – gridde en az 4 hücreyi scatter yapar.</summary>
    private void GrideEnAzDortScatterKoy()
    {
        if (grid == null || sutun <= 0 || satir <= 0) return;
        int scatterIdx = _scatterIndexCache;
        var aday = new List<Vector2Int>();
        for (int x = 0; x < sutun; x++)
            for (int y = 0; y < satir; y++)
                if (grid[x, y] != CARPAN_SEMBOL)
                    aday.Add(new Vector2Int(x, y));
        int kac = Mathf.Min(4, aday.Count);
        for (int i = 0; i < kac; i++)
        {
            int r = Random.Range(i, aday.Count);
            var t = aday[r]; aday[r] = aday[i]; aday[i] = t;
            grid[aday[i].x, aday[i].y] = scatterIdx;
        }
        _tumbleServisi?.SetGrid(grid);
    }

    /// <summary>Aşama 3 near-miss: gridde tam 3 scatter koyar (eşik 4 olduğu için bonus tetiklenmez; "az daha" hissi). Önce mevcut scatter'ları kaldırır, sonra 3 hücreye scatter koyar.</summary>
    private void GrideTamUcScatterKoy()
    {
        if (grid == null || sutun <= 0 || satir <= 0) return;
        int scatterIdx = _scatterIndexCache;
        int n = (tumbleAyarlari != null && tumbleAyarlari.PayTable_8_9 != null) ? tumbleAyarlari.PayTable_8_9.Length : 9;
        int baskaSembol = 0;
        if (baskaSembol == scatterIdx) baskaSembol = (scatterIdx + 1) % Mathf.Max(1, n);
        if (baskaSembol == CARPAN_SEMBOL) baskaSembol = (baskaSembol + 1) % Mathf.Max(1, n);
        for (int x = 0; x < sutun; x++)
            for (int y = 0; y < satir; y++)
                if (grid[x, y] == scatterIdx)
                    grid[x, y] = baskaSembol;
        var aday = new List<Vector2Int>();
        for (int x = 0; x < sutun; x++)
            for (int y = 0; y < satir; y++)
                if (grid[x, y] != CARPAN_SEMBOL)
                    aday.Add(new Vector2Int(x, y));
        int kac = Mathf.Min(3, aday.Count);
        for (int i = 0; i < kac; i++)
        {
            int r = Random.Range(i, aday.Count);
            var t = aday[r]; aday[r] = aday[i]; aday[i] = t;
            grid[aday[i].x, aday[i].y] = scatterIdx;
        }
        _tumbleServisi?.SetGrid(grid);
    }

    /// <summary>Gridde minClusterSize ve üstü kümeleri kırar; zorunlu boş spin için kazançsız grid üretir.</summary>
    private void GrideKazancsizYap()
    {
        if (grid == null || _tumbleServisi == null || tumbleAyarlari?.PayTable_8_9 == null) return;
        int n = tumbleAyarlari.PayTable_8_9.Length;
        int scatterIdx = _scatterIndexCache;
        for (int iter = 0; iter < 30; iter++)
        {
            _tumbleServisi.SetGrid(grid);
            var toRemove = _tumbleServisi.FindClustersToRemove(minClusterSize);
            if (toRemove == null || toRemove.Count == 0) break;
            var bySym = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Vector2Int>>();
            for (int i = 0; i < toRemove.Count; i++)
            {
                int x = toRemove[i].x, y = toRemove[i].y;
                if (x < 0 || x >= sutun || y < 0 || y >= satir) continue;
                int s = grid[x, y];
                if (!bySym.ContainsKey(s)) bySym[s] = new System.Collections.Generic.List<Vector2Int>();
                bySym[s].Add(new Vector2Int(x, y));
            }
            bool degisti = false;
            foreach (var kv in bySym)
            {
                if (kv.Value.Count < minClusterSize) continue;
                int degisecek = kv.Value.Count - minClusterSize + 1;
                int baskaSembol = (kv.Key + 1) % n;
                if (baskaSembol == scatterIdx) baskaSembol = (baskaSembol + 1) % n;
                if (baskaSembol == CARPAN_SEMBOL) baskaSembol = (baskaSembol + 1) % n;
                for (int t = 0; t < degisecek && t < kv.Value.Count; t++)
                {
                    var p = kv.Value[t];
                    grid[p.x, p.y] = baskaSembol;
                    degisti = true;
                }
            }
            if (!degisti) break;
        }
        _tumbleServisi?.SetGrid(grid);
    }

    /// <summary>Zorla çarpan + toggle açıkken tumble garantisi: gridde en az 8 aynı sembol (bir cluster) oluşturur.</summary>
    private void GrideZorlaEnAzBirCluster()
    {
        if (grid == null || sutun <= 0 || satir <= 0) return;
        int n = (tumbleAyarlari != null && tumbleAyarlari.PayTable_8_9 != null) ? tumbleAyarlari.PayTable_8_9.Length : 9;
        if (n <= 0) return;
        int scatterIdx = _scatterIndexCache;
        var adayHucreler = new List<Vector2Int>();
        for (int x = 0; x < sutun; x++)
            for (int y = 0; y < satir; y++)
                if (grid[x, y] != CARPAN_SEMBOL && grid[x, y] >= 0)
                    adayHucreler.Add(new Vector2Int(x, y));
        if (adayHucreler.Count < minClusterSize) return;
        int sembol = Random.Range(0, n);
        if (sembol == scatterIdx) sembol = (sembol + 1) % n;
        for (int i = 0; i < minClusterSize && i < adayHucreler.Count; i++)
        {
            var p = adayHucreler[i];
            grid[p.x, p.y] = sembol;
        }
        _tumbleServisi?.SetGrid(grid);
    }

    private void GridHucreleriniTemizle(List<Vector2Int> toRemove)
    {
        if (toRemove == null || grid == null || carpanDegerGrid == null) return;
        for (int i = 0; i < toRemove.Count; i++)
        {
            int x = toRemove[i].x, y = toRemove[i].y;
            grid[x, y] = -1;
            carpanDegerGrid[x, y] = 0;
            int ridx = _izgaraServisi != null ? _izgaraServisi.XYToIndex(x, y) : -1;
            if (carpanDegerByCellIndex != null && ridx >= 0 && ridx < carpanDegerByCellIndex.Length)
                carpanDegerByCellIndex[ridx] = 0;
        }
        _tumbleServisi?.SetGrid(grid);
    }

    /// <summary>Meyvelerin drop-in animasyonu öncesi başlangıç konumuna (yukarıda, şeffaf) alır; zorla çarpan dahil tüm spinlerde animasyon görünsün diye.</summary>
    private void HucreleriDropInBaslangicKonumunaAl()
    {
        if (hucreler == null || hucreler.Length == 0) return;
        float offset = dropStartYOffset;
        for (int i = 0; i < hucreler.Length; i++)
        {
            var img = hucreler[i];
            if (img == null) continue;
            Vector2 hedef = (cellPos != null && i < cellPos.Length) ? cellPos[i] : img.rectTransform.anchoredPosition;
            img.rectTransform.anchoredPosition = hedef + Vector2.up * offset;
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }
    }

    private IEnumerator SimulasyonKaydiniOynatImpl(SpinSimulasyonKaydi kayit)
    {
        if (kayit == null) yield break;

        UI_CarpanSifirla();
        spinKazancHam = 0;

        for (int x = 0; x < kayit.Sutun && x < sutun; x++)
            for (int y = 0; y < kayit.Satir && y < satir; y++)
            {
                grid[x, y] = kayit.IlkGrid[x, y];
                carpanDegerGrid[x, y] = kayit.IlkCarpanGrid[x, y];
            }
        ApplyNewGridAndSync(grid, carpanDegerGrid);
        if (kayit.IlkCarpanDegerleri != null && kayit.IlkCarpanDegerleri.Count > 0 && _carpanServisi != null)
            _carpanServisi.RecordPlacedCarpanlar(kayit.IlkCarpanDegerleri);
        _izgaraServisi?.RenderAllSprites(true, true);
        _uiServisi?.UI_Guncelle();

        _izgaraServisi?.CacheCellPositionsThenDisableLayout();
        var guncelPos = _izgaraServisi?.GetCellPos();
        if (guncelPos != null)
        {
            cellPos = guncelPos;
            _animasyonServisi?.SetCellPos(guncelPos);
        }
        HucreleriDropInBaslangicKonumunaAl();
        yield return null;
        if (_animasyonServisi != null)
            yield return _animasyonServisi.AnimateGridDropIn();
        _uiServisi?.UI_Guncelle();

        for (int a = 0; a < kayit.Adimlar.Count; a++)
        {
            var adim = kayit.Adimlar[a];
            spinKazancHam += adim.TurKazanci;
            sonSpinKazancHamGoster = spinKazancHam;
            sonSpinCarpanGoster = _carpanServisi != null ? _carpanServisi.GetTotalMultiplierForSpin() : 1;
            sonSpinKazancToplamGoster = _carpanServisi != null ? _carpanServisi.MulClampInt(spinKazancHam, sonSpinCarpanGoster) : spinKazancHam;
            sonSpinKazanci = sonSpinKazancToplamGoster;
            _uiServisi?.UI_Guncelle();

            if (adim.PatlayanHucreler != null && adim.PatlayanHucreler.Count > 0)
            {
                _hizVeSesServisi?.PlayTumbleSfx(tumblePopClip, ref _lastTumblePopTime, tumblePopMinInterval, 1f);
                if (_tumbleServisi != null)
                {
                    var popCoro = _tumbleServisi.AnimatePop(adim.PatlayanHucreler);
                    if (popCoro != null) yield return popCoro;
                }
            }
            yield return new WaitForSeconds(0.30f);
            GridHucreleriniTemizle(adim.PatlayanHucreler);
            if (_cokmeAkisServisi != null)
                yield return _cokmeAkisServisi.CokmeDoldurOynat(adim);
            if (adim.CarpanDegerleriBuTur != null && adim.CarpanDegerleriBuTur.Count > 0 && _carpanServisi != null)
                _carpanServisi.RecordPlacedCarpanlar(adim.CarpanDegerleriBuTur);
            yield return new WaitForSeconds(0.15f);
            _uiServisi?.UI_Guncelle();
            yield return new WaitForSeconds(betweenStepsDelay);
        }

        tumbleToplamKazanc = spinKazancHam;
    }

    private List<Vector2Int> FloodFillCluster(int sx, int sy, int sym, bool[,] visited)
    {
        List<Vector2Int> outList = new List<Vector2Int>();
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(sx, sy));
        visited[sx, sy] = true;

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            outList.Add(p);

            TryEnqueue(p.x + 1, p.y);
            TryEnqueue(p.x - 1, p.y);
            TryEnqueue(p.x, p.y + 1);
            TryEnqueue(p.x, p.y - 1);
        }

        void TryEnqueue(int nx, int ny)
        {
            if (nx < 0 || nx >= sutun || ny < 0 || ny >= satir) return;
            if (visited[nx, ny]) return;
            if (grid[nx, ny] != sym) return;
            visited[nx, ny] = true;
            q.Enqueue(new Vector2Int(nx, ny));
        }

        return outList;
    }

    // GRID FILL / RENDER
    // ==========================
    private float CurrentScatterChance() => bonusAktif ? scatterChanceBonus : scatterChanceNormal;

    /// <summary>SenaryoServisi delegasyonu: Senaryo 1'de 50, Senaryo 2'de 75 spin sonrası scatter garantisi. Garanti spininde dolumda 0 döner (sonra GrideEnAzDortScatterKoy tam 4 koyar).</summary>
    private float GetScatterChanceFor(bool bonusAktif)
    {
        if (bonusAktif) return scatterChanceBonus;
        if (SenaryoYoneticisi.I == null) return scatterChanceNormal;
        var asama = SenaryoYoneticisi.I.mevcutAsama;
        int sinceScatter = SenaryoYoneticisi.I.SpinsSinceLastScatter();
        if (asama == SenaryoYoneticisi.SenaryoAsama.Asama1_IsindirmaUmut)
        {
            if (sinceScatter >= 50) return 0f; // Garanti spininde dolumda scatter yok; GrideEnAzDortScatterKoy tam 4 koyar
            return 0.006f; // Seviye 1 sakin: ortalama ~50 spinde bir garanti yeterli, rastgele scatter seyrek
        }
        if (asama == SenaryoYoneticisi.SenaryoAsama.Asama2_KontrolBende)
        {
            if (sinceScatter >= 75) return 0f; // Garanti spininde dolumda scatter yok; GrideEnAzDortScatterKoy tam 4 koyar
            return 0.015f;
        }
        if (asama == SenaryoYoneticisi.SenaryoAsama.Asama3_AzDahaKayipKovalama)
            return 0.008f; // Near-miss ağırlıklı; sık 3 scatter GrideTamUcScatterKoy ile veriliyor
        return scatterChanceNormal;
    }

    // KURAL SABİT: tumble eşiği minClusterSize=8
    // Zorluk arttıkça, 8'e TAMAMLAYACAK sembollerin seçilme ihtimali azalır (anti-8 bias).

// v=8'de nötr; v<8'de easy bias, v>8'de hard bias uygular.
private float BiasMultiplier(float easyMult, float hardMult)
{
    float m = 1f;
    if (_easyBias01 > 0f) m *= Mathf.Lerp(1f, easyMult, _easyBias01);
    if (_hardBias01 > 0f) m *= Mathf.Lerp(1f, hardMult, _hardBias01);
    return m;
}

    /// <summary>Düşük zorlukta bakiyenin erimesini azaltmak için kazanç çarpanı (paytable etkisi). Zorluk 5 ve altı: 1.35x, 6: 1.2x, 7: 1.1x.</summary>
    private int ZorlukKazancCarpaniUygula(int hamKazanc)
    {
        if (hamKazanc <= 0) return 0;
        if (zorlukSeviyesi <= 5) return Mathf.RoundToInt(hamKazanc * 1.35f);
        if (zorlukSeviyesi <= 6) return Mathf.RoundToInt(hamKazanc * 1.2f);
        if (zorlukSeviyesi <= 7) return Mathf.RoundToInt(hamKazanc * 1.1f);
        return hamKazanc;
    }

    // ==========================
// ÇARPAN (Yeni Sistem: ekrana bomba/jeton düşer, değerler ÇARPILIR)
// ==========================

    private void CarpanUretVeBirik()
    {
        if (_carpanServisi == null) return;
        _carpanServisi.SetForceCarpan(zorlaSiradakiCarpan);
        _carpanServisi.TryScheduleCarpanDrop(bonusAktif);
        zorlaSiradakiCarpan = 0;
        if (carpanAyarlari != null)
            carpanAyarlari.ZorlaSiradakiCarpan = 0;
    }

    private void CarpanlariDoluGriddeUygula()
    {
        _carpanYerlestirmeServisi?.CarpanlariDoluGriddeUygula();
    }

    int ICarpanYerlestirmeBaglami.GetSutun() => sutun;
    int ICarpanYerlestirmeBaglami.GetSatir() => satir;
    int[,] ICarpanYerlestirmeBaglami.GetGrid() => grid;
    int[,] ICarpanYerlestirmeBaglami.GetCarpanDegerGrid() => carpanDegerGrid;
    int[] ICarpanYerlestirmeBaglami.GetCarpanDegerByCellIndex() => carpanDegerByCellIndex;
    int ICarpanYerlestirmeBaglami.GetCarpanSembol() => CARPAN_SEMBOL;
    int ICarpanYerlestirmeBaglami.GetScatterIndexCache() => _scatterIndexCache;
    CarpanServisi ICarpanYerlestirmeBaglami.GetCarpanServisi() => _carpanServisi;
    IzgaraServisi ICarpanYerlestirmeBaglami.GetIzgaraServisi() => _izgaraServisi;

private int RastgeleCarpan()
{
    // Yüksek çarpan oranı: 100x / 250x / 500x daha sık gelsin.
    float oran = Mathf.Clamp01(yuksekCarpanOrani);
    if (oran > 0f && Random.value < oran)
    {
        int[] yuksek = new int[] { 100, 250, 500 };
        return yuksek[Random.Range(0, yuksek.Length)];
    }
    int[] pool = new int[] { 2, 3, 5, 10, 20, 50, 100, 200, 500, 1000 };
    int n = Mathf.Clamp(carpanHavuzu, 1, pool.Length);
    return pool[Random.Range(0, n)];
}
    private int UygulaSpinCarpani(int spinKazanci) => _ekonomiServisi != null ? _ekonomiServisi.UygulaSpinCarpani(spinKazanci) : 0;

private void TrySpawnCarpanOverlay(int carpanDegeri)
{
    if (carpanSembolSprite == null) return;
    if (hucreler == null || hucreler.Length == 0) return;
    int idx = Random.Range(0, hucreler.Length);
    _carpanOverlayServisi?.SpawnCarpanOverlayAt(idx, carpanDegeri);
}

    private void ClearAllCarpanOverlays()
    {
        _carpanOverlayServisi?.ClearAll();

        // Grid içindeki çarpan sembollerini de sıfırla (bir sonraki spin temiz başlasın)
        if (grid != null && carpanDegerGrid != null)
        {
            for (int y = 0; y < satir; y++)
            {
                for (int x = 0; x < sutun; x++)
                {
                    if (grid[x, y] == CARPAN_SEMBOL)
                    {
                        grid[x, y] = -1; // boş yap; FillRandomAll/yerçekimi zaten dolduracak
                        carpanDegerGrid[x, y] = 0;
                    }
                }
            }
        }

        if (carpanHücreTextleri != null)
        {
            for (int i = 0; i < carpanHücreTextleri.Length; i++)
                if (carpanHücreTextleri[i] != null) carpanHücreTextleri[i].gameObject.SetActive(false);
        }
    }



    
// ==========================
// ÇARPAN UI (Yeni Sistem)
// ==========================
private void UI_CarpanSifirla()
{
    int maxAdet = _senaryoServisi != null ? _senaryoServisi.GetMaxCarpanAdedi() : 0;
    if (zorlaSiradakiCarpan > 0 && maxAdet < 1)
        maxAdet = 1;
    _carpanServisi?.ResetForNewSpin(maxAdet);
    ClearAllCarpanOverlays();
    UI_CarpanGuncelle();
}

private void UI_CarpanGuncelle()
{
    if (carpanText == null) return;

    long mlt = _carpanServisi != null ? _carpanServisi.GetCurrentMultiplier() : 0;
    if (mlt < 1) mlt = 1;

  //  carpanText.text = $"ÇARPAN: x{mlt}";
}

    private IEnumerator ScatterBuyutEfekti()
    {
        if (_scatterEfektServisi == null) yield break;
        yield return _scatterEfektServisi.ScatterBuyutEfektiCalistir();
    }
}