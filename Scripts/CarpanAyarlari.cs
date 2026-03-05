using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// CarpanAyarlari - Çarpan sistemi ayarları ve mantığı

public class CarpanAyarlari : MonoBehaviour
{
    [Header("Carpan Uretimi")]
    public bool CarpanUretimiAktif = true;
    public bool CarpanSadeceBonus = true;

    [Range(0f, 1f)]
    public float CarpanUretimOlasiligi = 0.15f;

    [Min(0)]
    public int MaxCarpanAdedi = 2;

    [Min(0)]
    public int CarpanHavuzu = 10;

    [Tooltip("Rastgele çarpan seçilirken 100x/250x/500x gelme olasılığı (0-1). Yüksek = büyük çarpanlar daha sık düşer.")]
    [Range(0f, 1f)]
    public float YuksekCarpanOrani = 0.30f;

    [Tooltip("Test/Debug: Bir sonraki carpani zorla (0 ise kapali).")]
    public int ZorlaSiradakiCarpan = 0;

    [Header("Carpan Gorseli (Sweet Bonanza tarzi)")]
    public Sprite CarpanSembolSprite;

    [Tooltip("Carpan overlay boyutu")]
    public Vector2 CarpanOverlaySize = new Vector2(110f, 110f);

    [Tooltip("Carpan yazisi font boyutu")]
    public int CarpanOverlayFontSize = 36;

    [Header("Carpan Yazi Gorunumu")]
    public Color CarpanYaziRengi = Color.white;
    public Color CarpanYaziDisCizgiRengi = Color.black;

    [Range(0f, 1f)]
    public float CarpanYaziDisCizgiKalinlik = 0.35f;

    public bool CarpanYaziKalin = true;
    public bool CarpanYaziGolge = true;
    public Color CarpanYaziGolgeRengi = Color.black;
    public Vector2 CarpanYaziGolgeOffset = new Vector2(2f, -2f);

    [Header("Carpan Text Offset")]
    public Vector2 CarpanOverlayTextOffset = new Vector2(0f, -6f);

    [Header("Carpan Drop Anim")]
    public float CarpanOverlayDropStartYOffset = 250f;

    [Min(0f)]
    public float CarpanOverlayDropDuration = 0.18f;

    void Start()
    {
        // OyunYoneticisi referansını bul
        if (oyunYoneticisi == null)
            oyunYoneticisi = FindObjectOfType<OyunYoneticisi>();
        
        Debug.Log($"[CarpanAyarlari] OyunYoneticisi referansı: {(oyunYoneticisi != null ? "BAĞLANDI" : "BULUNAMADI")}");
        
        // Butonları otomatik bul ve bağla
        OtomatikButonlariBul();
    }
    
    private void OtomatikButonlariBul()
    {
        Button[] butonlar = FindObjectsOfType<Button>(true);
        foreach (Button b in butonlar)
        {
            if (b == null) continue;
            string adi = b.gameObject.name.ToLower(); // küçük harfe çevir
            
            if (adi.Contains("forcex2") && !adi.Contains("forcex100") && !adi.Contains("forcex20"))
            {
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => ZorlaCarpan(2));
                Debug.Log($"[CarpanAyarlari] ForceX2 butonu bulundu: {b.gameObject.name}");
            }
            else if (adi.Contains("forcex5") && !adi.Contains("forcex50"))
            {
                b.onClick.RemoveAllListeners();
                // Eski x5 butonu artık x100 olarak kullanılacak.
                b.onClick.AddListener(() => ZorlaCarpan(100));
                Debug.Log($"[CarpanAyarlari] ForceX100 (eski 5x) butonu bulundu: {b.gameObject.name}");
            }
            else if (adi.Contains("forcex10") && !adi.Contains("forcex100"))
            {
                b.onClick.RemoveAllListeners();
                // Eski x10 butonu artık x150 olarak kullanılacak.
                b.onClick.AddListener(() => ZorlaCarpan(150));
                Debug.Log($"[CarpanAyarlari] ForceX150 (eski 10x) butonu bulundu: {b.gameObject.name}");
            }
            else if (adi.Contains("forcex50"))
            {
                b.onClick.RemoveAllListeners();
                // Eski x50 butonu artık x250 olarak kullanılacak.
                b.onClick.AddListener(() => ZorlaCarpan(250));
                Debug.Log($"[CarpanAyarlari] ForceX250 (eski 50x) butonu bulundu: {b.gameObject.name}");
            }
            else if (adi.Contains("forcex100"))
            {
                b.onClick.RemoveAllListeners();
                // Eski x100 butonu artık x500 olarak kullanılacak.
                b.onClick.AddListener(() => ZorlaCarpan(500));
                Debug.Log($"[CarpanAyarlari] ForceX500 (eski 100x) butonu bulundu: {b.gameObject.name}");
            }
            else if (adi.Contains("carpan") && (adi.Contains("sifirla") || adi.Contains("reset")))
            {
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => ZorlaSiradakiCarpan = 0);
                Debug.Log($"[CarpanAyarlari] CarpanSifirla butonu bulundu: {b.gameObject.name}");
            }
        }
    }

    // ========== ÇARPAN MANTIĞI ==========
    
    private int _carpanKalanBuSpin = 0;
    private readonly List<int> _pendingCarpanDusurecek = new List<int>();
    private long _spinCarpanCarpim = 0;
    private readonly List<int> _spinCarpanDegerleri = new List<int>();
    
    // Referans
    public OyunYoneticisi oyunYoneticisi;

    // ========== BUTONLAR İÇİN FONKSİYONLAR ==========
    
    public void ZorlaCarpan(int deger)
    {
        ZorlaSiradakiCarpan = deger;
        
        // OyunYoneticisi ile senkronize et - DİREKT GÜNCELLE!
        if (oyunYoneticisi != null)
        {
            oyunYoneticisi.zorlaSiradakiCarpan = deger;
            // OyunYoneticisi'nin carpanUretimiAktif değerini de güncelle
            oyunYoneticisi.carpanUretimiAktif = CarpanUretimiAktif;
            oyunYoneticisi.carpanSadeceBonus = CarpanSadeceBonus;
            oyunYoneticisi.carpanUretimOlasiligi = Mathf.Clamp01(CarpanUretimOlasiligi);
            oyunYoneticisi.maxCarpanAdedi = Mathf.Max(0, MaxCarpanAdedi);
            oyunYoneticisi.carpanHavuzu = Mathf.Max(0, CarpanHavuzu);
            oyunYoneticisi.yuksekCarpanOrani = Mathf.Clamp01(YuksekCarpanOrani);
        }
        
        Debug.Log($"[CarpanAyarlari] Zorla çarpan: x{deger} -> OyunYoneticisi.zorlaSiradakiCarpan={oyunYoneticisi.zorlaSiradakiCarpan}");
    }

    public void ZorlaCarpanSifirla()
    {
        ZorlaSiradakiCarpan = 0;
        Debug.Log("[CarpanAyarlari] Zorla çarpan sıfırlandı");
    }

    public void ZorlaCarpan2() => ZorlaCarpan(2);
    public void ZorlaCarpan5() => ZorlaCarpan(100);
    public void ZorlaCarpan10() => ZorlaCarpan(150);
    public void ZorlaCarpan50() => ZorlaCarpan(250);
    public void ZorlaCarpan100() => ZorlaCarpan(500);

    // ========== ÇARPAN MANTIĞI METOTLARI ==========

    public void SpinBasindaSifirla()
    {
        _carpanKalanBuSpin = Mathf.Max(0, MaxCarpanAdedi);
        _spinCarpanCarpim = 0;
        _spinCarpanDegerleri.Clear();
        _pendingCarpanDusurecek.Clear();
    }

    public long GetCarpanCarpim() => _spinCarpanCarpim;
    public int GetCarpanKalan() => _carpanKalanBuSpin;

    public void CarpanUretVeBirik(bool bonusAktif, System.Func<int> kalanOdenebilirFunc, int mevcutKazanc)
    {
        if (_carpanKalanBuSpin <= 0) return;
        if (!CarpanUretimiAktif) return;
        if (CarpanSadeceBonus && !bonusAktif) return;

        int force = ZorlaSiradakiCarpan;
        int adet = 0;

        if (force > 0)
        {
            adet = 1;
        }
        else
        {
            if (CarpanUretimOlasiligi <= 0f) return;
            if (Random.value > CarpanUretimOlasiligi) return;
            adet = Random.Range(1, Mathf.Min(MaxCarpanAdedi, _carpanKalanBuSpin) + 1);
        }

        for (int i = 0; i < adet; i++)
        {
            int carpan = (force > 0) ? force : RastgeleCarpan();
            
            if (bonusAktif)
            {
                int kalanOdenebilir = kalanOdenebilirFunc != null ? kalanOdenebilirFunc() : int.MaxValue;
                if (kalanOdenebilir <= 0) continue;

                long yeniM = (long)GetCurrentMultiplierInt() + (long)carpan;
                if (yeniM < 1) yeniM = 1;
                long proj = ((long)mevcutKazanc + 1) * yeniM;
                if (proj > kalanOdenebilir) continue;
            }
            
            if (carpan <= 0) continue;
            _pendingCarpanDusurecek.Add(carpan);
        }

        ZorlaSiradakiCarpan = 0;
    }

    public int RastgeleCarpan()
    {
        float oran = Mathf.Clamp01(YuksekCarpanOrani);
        if (oran > 0f && Random.value < oran)
        {
            int[] yuksek = new int[] { 100, 250, 500 };
            return yuksek[Random.Range(0, yuksek.Length)];
        }
        int[] pool = new int[] { 2, 3, 5, 10, 20, 50, 100, 200, 500, 1000 };
        int n = Mathf.Clamp(CarpanHavuzu, 1, pool.Length);
        return pool[Random.Range(0, n)];
    }

    public int GetCurrentMultiplierInt()
    {
        long m = _spinCarpanCarpim;
        if (m > int.MaxValue) return int.MaxValue;
        return (int)m;
    }

    public void CarpanlariDoluGriddeUygula(int[,] grid, int[,] carpanDegerGrid, int[] carpanDegerByCellIndex, int satir, int sutun)
    {
        if (_pendingCarpanDusurecek == null || _pendingCarpanDusurecek.Count == 0) return;
        if (grid == null || carpanDegerGrid == null) return;

        List<Vector2Int> adaylar = new List<Vector2Int>();
        for (int y = 0; y < satir; y++)
        {
            for (int x = 0; x < sutun; x++)
            {
                if (grid[x, y] == -2) continue; // CARPAN_SEMBOL
                if (grid[x, y] < 0) continue;
                adaylar.Add(new Vector2Int(x, y));
            }
        }

        if (adaylar.Count == 0) return;

        int placeCount = Mathf.Min(_pendingCarpanDusurecek.Count, adaylar.Count, _carpanKalanBuSpin);

        for (int i = 0; i < placeCount; i++)
        {
            int pick = Random.Range(0, adaylar.Count);
            Vector2Int p = adaylar[pick];
            adaylar.RemoveAt(pick);

            int carpan = _pendingCarpanDusurecek[i];
            if (carpan <= 0) continue;

            grid[p.x, p.y] = -2; // CARPAN_SEMBOL
            carpanDegerGrid[p.x, p.y] = carpan;
            int cellIdx = (p.y * sutun) + p.x;
            if (carpanDegerByCellIndex != null && cellIdx >= 0 && cellIdx < carpanDegerByCellIndex.Length)
                carpanDegerByCellIndex[cellIdx] = carpan;

            _spinCarpanDegerleri.Add(carpan);
            _spinCarpanCarpim += carpan;
            if (_spinCarpanCarpim > long.MaxValue) _spinCarpanCarpim = long.MaxValue;
        }

        _pendingCarpanDusurecek.Clear();
        _carpanKalanBuSpin -= placeCount;
        if (_carpanKalanBuSpin < 0) _carpanKalanBuSpin = 0;
    }

    public int UygulaSpinCarpani(int spinKazanci)
    {
        if (spinKazanci <= 0) return 0;

        long m = GetCarpanCarpim();
        if (m > 1)
        {
            long sonuc = (long)spinKazanci * m;
            if (sonuc > int.MaxValue) sonuc = int.MaxValue;
            return (int)sonuc;
        }
        return spinKazanci;
    }
}
