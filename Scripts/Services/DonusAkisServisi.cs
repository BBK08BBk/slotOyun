using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// DonusAkisServisi için state ve servis erişimi arayüzü. OyunYoneticisi implement eder.
/// </summary>
public interface IDonusAkisBaglami
{
    UIServisi UIServisi { get; }
    IzgaraServisi IzgaraServisi { get; }
    OdemeServisi OdemeServisi { get; }
    AnimasyonServisi AnimasyonServisi { get; }
    TumbleServisi TumbleServisi { get; }
    CarpanServisi CarpanServisi { get; }
    EkonomiServisi EkonomiServisi { get; }
    LogServisi DonusKayitServisi { get; }
    SenaryoServisi SenaryoServisi { get; }
    HizVeSesServisi HizVeSesServisi { get; }
    bool SpinCalisiyor { get; set; }
    bool BonusAktif { get; set; }
    int BonusHakKalan { get; set; }
    int BonusKazanc { get; set; }
    int OturumKazanc { get; set; }
    int BonusPendingOdemeTL { get; set; }
    int BonusZorlaCarpanBirikenTL { get; set; }
    bool SpinKazanciOturumaEklendi { get; set; }
    int SpinKazancHam { get; set; }
    int TumbleToplamKazanc { get; set; }
    int SonSpinKazanci { get; set; }
    int SpinPrevBakiye { get; }
    int SpinBahisTL { get; }
    float BonusSpinBekleme { get; }
    int SonSpinKazancHamGoster { set; }
    int SonSpinCarpanGoster { set; }
    int SonSpinKazancToplamGoster { set; }
    int BonusOturumOdenenToplamTL { get; set; }
    int BonusMaxOdemeTL { get; }
    int BonusOdenenTL { get; }
    bool BonusBudgetAktif { get; }
    int BonusBudgetKalanTL { get; }
    int[,] Grid { get; }
    int Satir { get; }
    int Sutun { get; }
    void UI_CarpanSifirla();
    void CarpanUretVeBirik();
    void CarpanlariDoluGriddeUygula();
    void BaslatBonus();
    IEnumerator ScatterBuyutEfekti();
    IEnumerator ShowBonusEndMessage(int bonusToplamKazanc);
    void SetSpinIconRotate(bool rotate);
    void SetOturumKazancTextActive(bool active);
    void NormalOyunMusicPlay();
    void NormalOyunMusicUnPause();
    /// <summary>Arkada simülasyon çalıştırır; limiti aşmayan sonucu kaydeder. Re-roll max 200.</summary>
    SpinSimulasyonKaydi SimuleEtVeKaydet(int odenebilirLimit, bool bonusSpin);
    /// <summary>Kaydedilen spin sonucunu ekranda oynatır (ilk grid + drop-in + tumble adımları).</summary>
    IEnumerator SimulasyonKaydiniOynat(SpinSimulasyonKaydi kayit);
    /// <summary>Bonus bittikten sonra kalan otomatik spin varsa döngüyü yeniden başlatır.</summary>
    void TryResumeOtomatikSpin();
    /// <summary>Senaryo sahnesinde: ödeme yapıldıysa ödenebilir bütçeden düş, yapılmadıysa bahisi ekle.</summary>
    void SenaryoOdenebilirGuncelle(int odenen, int bahis);
    /// <summary>Az daha near-miss panelini gösterir, kısa bekler, kapatır. Panel atanmamışsa atlanır.</summary>
    IEnumerator ShowAzDahaPanelAndWait();
}

/// <summary>
/// Normal dönüş ve bonus döngüsü akışı. State ve servis erişimi IDonusAkisBaglami ile; alt coroutine'ler Func ile çalıştırılır.
/// </summary>
public class DonusAkisServisi
{
    private IDonusAkisBaglami _ctx;
    private Func<IEnumerator, Coroutine> _runCoroutine;

    public void SetBaglam(IDonusAkisBaglami ctx)
    {
        _ctx = ctx;
    }

    public void SetRunCoroutine(Func<IEnumerator, Coroutine> run)
    {
        _runCoroutine = run;
    }

    public IEnumerator NormalSpinAkisi()
    {
        if (_ctx == null) yield break;

        int spinNo = (SenaryoYoneticisi.I != null ? SenaryoYoneticisi.I.toplamSpin : 0) + 1;
        int bakiyeSnap = _ctx.EkonomiServisi != null ? _ctx.EkonomiServisi.Bakiye : 0;
        SenaryoYoneticisi.I?.LogEkle(SenaryoOlayKaydi.OlayTipi_NormalSpinBasladi, $"Normal spin başladı. Spin no: {spinNo}. Bakiye: {bakiyeSnap:N0} TL. Bahis: {_ctx.SpinBahisTL} TL.");

        _ctx.SpinCalisiyor = true;
        _ctx.UIServisi?.ButonDurumu(false);
        _ctx.SetSpinIconRotate(true);

        _ctx.SpinKazancHam = 0;
        _ctx.TumbleToplamKazanc = 0;
        _ctx.SonSpinKazanci = 0;
        _ctx.SonSpinKazancHamGoster = 0;
        _ctx.SonSpinCarpanGoster = 1;
        _ctx.SonSpinKazancToplamGoster = 0;
        _ctx.UI_CarpanSifirla();
        _ctx.SpinKazanciOturumaEklendi = false;

        _ctx.IzgaraServisi?.ResetScatterCountPerSpin();
        _ctx.UIServisi?.UI_Guncelle();

        int odenebilirNormal = _ctx.OdemeServisi != null ? _ctx.OdemeServisi.GetSpinOdenebilirLimit() : int.MaxValue;
        SpinSimulasyonKaydi kayit = _ctx.SimuleEtVeKaydet(odenebilirNormal, false);
        if (_runCoroutine != null)
            yield return _runCoroutine(_ctx.SimulasyonKaydiniOynat(kayit));

        if (_ctx.BonusAktif && _runCoroutine != null && _ctx.AnimasyonServisi != null)
            yield return _runCoroutine(_ctx.AnimasyonServisi.AnimateCarpanSisme());

        // Ödeme hesabı: tumble toplamı kayıtta kayıtlı olsun (oynatma ile senkron); kayit yoksa context değerlerini kullan
        int hamKazanc = (kayit != null) ? kayit.ToplamHamKazanc : _ctx.SpinKazancHam;
        int toplamX = (kayit != null) ? kayit.NihaiCarpanToplam : _ctx.CarpanServisi.GetTotalMultiplierForSpin();
        int teorikToplam = _ctx.CarpanServisi.MulClampInt(hamKazanc, toplamX);
        bool zorlaCarpanKullanildi = kayit != null && kayit.ZorlaCarpanKullanildi;

        _ctx.SonSpinKazancHamGoster = hamKazanc;
        _ctx.SonSpinCarpanGoster = toplamX;
        _ctx.SonSpinKazancToplamGoster = teorikToplam;
        _ctx.SonSpinKazanci = 0;

        int odenen = 0;
        if (teorikToplam > 0)
        {
            if (_ctx.BonusAktif)
            {
                odenen = teorikToplam;
                _ctx.BonusPendingOdemeTL += odenen;
            }
            else if (zorlaCarpanKullanildi)
            {
                odenen = teorikToplam;
                _ctx.EkonomiServisi.AddWinnings(odenen, _ctx.SpinBahisTL);
            }
            else
            {
                odenen = _ctx.OdemeServisi != null ? _ctx.OdemeServisi.PayFromHavuz(teorikToplam) : teorikToplam;
                // Senaryo modunda Kasa havuzu boş olabilir; kazancı yine de bütçe limitine göre bakiyeye ekle.
                if (SenaryoYoneticisi.I != null && odenebilirNormal != int.MaxValue && odenen < teorikToplam)
                    odenen = Mathf.Min(teorikToplam, odenebilirNormal);
                _ctx.EkonomiServisi.AddWinnings(odenen, _ctx.SpinBahisTL);
            }
            _ctx.SonSpinKazanci = odenen;
            _ctx.DonusKayitServisi?.RecordSpinResult(_ctx.SpinPrevBakiye, _ctx.EkonomiServisi.Bakiye, _ctx.SpinBahisTL, odenen);
            if (!zorlaCarpanKullanildi && odenen < teorikToplam)
                Debug.LogWarning($"[KASA] Ödül havuzu yetmedi. İstenen={teorikToplam} Ödenen={odenen}");
        }

        Debug.Log($"[NORMAL] SpinKazanci(istenen)={teorikToplam} Odenen={odenen} | Bakiye={_ctx.EkonomiServisi.Bakiye}");

        // Invariant: Normal spinde (bonus ve zorla çarpan dışında) ödenen tutar, spin başında hesaplanan ödenebilir limiti aşmamalı.
        if (!_ctx.BonusAktif && !zorlaCarpanKullanildi && odenen > 0 && odenebilirNormal != int.MaxValue && odenen > odenebilirNormal)
        {
            int spinNoLimitIhlal = SenaryoYoneticisi.I != null ? SenaryoYoneticisi.I.toplamSpin : -1;
            string asamaAdi = SenaryoYoneticisi.I != null ? SenaryoYoneticisi.I.GetAsamaAdi() : "Bilinmiyor";
            Debug.LogError($"[SentetikOyuncu][İHLAL] Normal spin ödemesi spin limitini aştı. Odenen={odenen} TL, SpinLimiti={odenebilirNormal} TL, SpinNo={spinNoLimitIhlal}, Asama={asamaAdi}");
        }

        if (_ctx.Grid != null && _ctx.Satir > 0 && _ctx.Sutun > 0)
        {
            var counts = new System.Collections.Generic.Dictionary<int, int>();
            for (int y = 0; y < _ctx.Satir; y++)
                for (int x = 0; x < _ctx.Sutun; x++)
                {
                    int v = _ctx.Grid[x, y];
                    if (v >= 0) { if (!counts.ContainsKey(v)) counts[v] = 0; counts[v]++; }
                }
            var sb = new System.Text.StringBuilder("[SPIN] Meyve dağılımı: ");
            foreach (var kv in counts)
                sb.Append($"Meyve{kv.Key}={kv.Value} ");
            Debug.Log(sb.ToString());
        }

        if (_ctx.SpinBahisTL > 0 && !_ctx.SpinKazanciOturumaEklendi)
        {
            // Oturum kazancı artık sadece bonus oyun sırasında kullanılacak.
            // Normal spinde sadece flag güncellensin.
            if (_ctx.BonusAktif && odenen > 0)
                _ctx.OturumKazanc += odenen;
            _ctx.SpinKazanciOturumaEklendi = true;
        }

        _ctx.SenaryoOdenebilirGuncelle(odenen, _ctx.SpinBahisTL);

        int[,] grid = _ctx.Grid;
        // Bonus tetiklemesi ilk düşüşteki (IlkGrid) scatter sayısına göre; scatter sayısı SpinTamamlandi'den önce bilinmeli (oturum sayacı doğru sıfırlansın).
        int scIlk = 0;
        if (kayit != null && kayit.IlkGrid != null && _ctx.IzgaraServisi != null)
            scIlk = _ctx.IzgaraServisi.ScatterSay(kayit.IlkGrid);
        int scSimdi = (_ctx.IzgaraServisi != null && grid != null) ? _ctx.IzgaraServisi.ScatterSay(grid) : 0;
        int sc = Mathf.Max(scIlk, scSimdi);
        int esik = _ctx.SenaryoServisi.GetScatterEsik();
        int scatterIdx = _ctx.IzgaraServisi != null ? _ctx.IzgaraServisi.GetScatterSpriteIndex() : -1;
        Debug.Log($"🧪 BonusKontrol: ScatterSay(ilk)={scIlk} ScatterSay(simdi)={scSimdi} kullanilan={sc} / Esik={esik} | scatter index={scatterIdx}");

        if (sc >= esik)
        {
            Debug.Log("🧪 BonusKontrol: EŞİK GEÇİLDİ -> Bonus başlıyor");
            SenaryoYoneticisi.I?.OnBonusTriggered();
        }
        SenaryoYoneticisi.I?.SpinTamamlandi(odenen, _ctx.SpinBahisTL);
        int bakiyeSon = _ctx.EkonomiServisi != null ? _ctx.EkonomiServisi.Bakiye : 0;
        SenaryoYoneticisi.I?.LogEkle(SenaryoOlayKaydi.OlayTipi_NormalSpinBitti, $"Normal spin tamamlandı. Ödenen: {odenen:N0} TL. Bakiye: {bakiyeSon:N0} TL.");

        if (sc < esik && grid != null && _ctx.Satir > 0 && _ctx.Sutun > 0)
        {
            var counts = new System.Collections.Generic.Dictionary<int, int>();
            for (int y = 0; y < _ctx.Satir; y++)
                for (int x = 0; x < _ctx.Sutun; x++)
                {
                    int v = grid[x, y];
                    if (v >= 0) { if (!counts.ContainsKey(v)) counts[v] = 0; counts[v]++; }
                }
            var sb = new System.Text.StringBuilder("Grid sembol dağılımı (index->adet): ");
            foreach (var kv in counts)
                sb.Append($"{kv.Key}->{kv.Value} ");
            Debug.Log(sb.ToString() + $"\n-> 4-5 silah görüyorsan, o adette olan index scatter olmalı (şu an scatterIdx={scatterIdx}). TumbleAyarlari.ScatterIndex'i Inspector'dan ayarla.");
        }

        // Finale aşamasında oyunu bilinçli olarak yavaşlat: her spin sonrası kısa duraklama
        if (SenaryoYoneticisi.I != null && SenaryoYoneticisi.I.mevcutAsama == SenaryoYoneticisi.SenaryoAsama.Asama7_Finale)
            yield return new WaitForSeconds(1.5f);

        if (sc >= esik)
        {
            if (_runCoroutine != null)
                yield return _runCoroutine(_ctx.ScatterBuyutEfekti());
            _ctx.SpinCalisiyor = false;
            _ctx.BaslatBonus();
            yield break;
        }

        // Az daha near-miss: 3 scatter (bonus yok) veya benzeri — "Az daha! Hadi tekrar dene" paneli
        if (kayit != null && kayit.AzDahaNearMiss && _runCoroutine != null)
            yield return _runCoroutine(_ctx.ShowAzDahaPanelAndWait());

        _ctx.SpinCalisiyor = false;
        _ctx.UIServisi?.ButonDurumu(true);
        _ctx.SetSpinIconRotate(false);
        _ctx.UIServisi?.UI_Guncelle();
    }

    public IEnumerator BonusDongusu()
    {
        if (_ctx == null) yield break;

        _ctx.SpinCalisiyor = true;
        _ctx.UIServisi?.ButonDurumu(false);

        while (_ctx.BonusHakKalan > 0)
        {
            _ctx.UI_CarpanSifirla();
            _ctx.SpinKazancHam = 0;
            _ctx.TumbleToplamKazanc = 0;
            _ctx.SonSpinKazanci = 0;
            _ctx.SonSpinKazancHamGoster = 0;
            _ctx.SonSpinCarpanGoster = 1;
            _ctx.SonSpinKazancToplamGoster = 0;
            _ctx.SpinKazanciOturumaEklendi = false;
            _ctx.UIServisi?.UI_Guncelle();

            int bonusLimit = _ctx.SenaryoServisi.GetBonusRemainingPayableTL();
            SpinSimulasyonKaydi kayit = _ctx.SimuleEtVeKaydet(bonusLimit, true);
            if (_runCoroutine != null && kayit != null)
                yield return _runCoroutine(_ctx.SimulasyonKaydiniOynat(kayit));
            else if (kayit == null)
                yield return null;

            if (_runCoroutine != null && _ctx.AnimasyonServisi != null)
                yield return _runCoroutine(_ctx.AnimasyonServisi.AnimateCarpanSisme());

            // Ödeme hesabı: ham kazanç ve çarpanı kayıttan al (simülasyon tek doğru kaynak; oynatma sonrası CarpanServisi state'i bazen senkron kalmayabiliyor, bombalar düşse bile çarpan 1 kalabiliyor).
            int hamKazanc = (kayit != null) ? kayit.ToplamHamKazanc : _ctx.SpinKazancHam;
            int toplamX = (kayit != null && kayit.NihaiCarpanToplam >= 1) ? kayit.NihaiCarpanToplam : _ctx.CarpanServisi.GetTotalMultiplierForSpin();
            int teorikToplam = _ctx.CarpanServisi.MulClampInt(hamKazanc, toplamX);
            bool zorlaCarpanGoster = kayit != null && kayit.ZorlaCarpanKullanildi;
            int maxOdenebilir = zorlaCarpanGoster ? int.MaxValue : _ctx.SenaryoServisi.GetBonusRemainingPayableTL();
            int spinKazanci = teorikToplam;
            if (maxOdenebilir != int.MaxValue && spinKazanci > maxOdenebilir)
                spinKazanci = maxOdenebilir;

            _ctx.SonSpinKazancHamGoster = hamKazanc;
            _ctx.SonSpinCarpanGoster = toplamX;
            _ctx.SonSpinKazancToplamGoster = zorlaCarpanGoster ? teorikToplam : spinKazanci;
            _ctx.SonSpinKazanci = 0;

            int odenmekIstenen = zorlaCarpanGoster ? teorikToplam : spinKazanci;
            if (!zorlaCarpanGoster && _ctx.BonusMaxOdemeTL != int.MaxValue)
            {
                int capKalan = Mathf.Max(0, _ctx.BonusMaxOdemeTL - _ctx.BonusOdenenTL);
                odenmekIstenen = Mathf.Clamp(odenmekIstenen, 0, capKalan);
            }
            if (!zorlaCarpanGoster && _ctx.BonusBudgetAktif && _ctx.BonusBudgetKalanTL != int.MaxValue)
            {
                odenmekIstenen = Mathf.Clamp(odenmekIstenen, 0, _ctx.BonusBudgetKalanTL);
            }

            int odenen = 0;
            if (odenmekIstenen > 0)
            {
                odenen = odenmekIstenen;
                if (zorlaCarpanGoster)
                    _ctx.BonusZorlaCarpanBirikenTL += odenen;
                else
                    _ctx.BonusPendingOdemeTL += odenen;
            }

            if (odenen > 0 && !zorlaCarpanGoster) _ctx.SenaryoServisi?.RecordBonusPayment(odenen);
            _ctx.SonSpinKazanci = odenen;

            _ctx.DonusKayitServisi?.RecordBonusSpin(_ctx.SpinPrevBakiye, _ctx.EkonomiServisi.Bakiye, _ctx.SpinBahisTL, odenen);

            // Invariant: Bonus spinde (zorla çarpan hariç) tek spin ödemesi, bu spinin başında alınan bonusRemainingLimit'i aşmamalı.
            if (!zorlaCarpanGoster && odenen > 0 && bonusLimit != int.MaxValue && odenen > bonusLimit)
            {
                int spinNoBonus = SenaryoYoneticisi.I != null ? SenaryoYoneticisi.I.toplamSpin : -1;
                string asamaBonus = SenaryoYoneticisi.I != null ? SenaryoYoneticisi.I.GetAsamaAdi() : "Bilinmiyor";
                Debug.LogError($"[SentetikOyuncu][İHLAL] Bonus spin ödemesi bonus limitini aştı. Odenen={odenen} TL, BonusLimit={bonusLimit} TL, SpinNo={spinNoBonus}, Asama={asamaBonus}");
            }

            if (odenen > 0 || zorlaCarpanGoster)
            {
                int oturumaEklenecek = zorlaCarpanGoster ? teorikToplam : odenen;
                _ctx.BonusKazanc += oturumaEklenecek;
                _ctx.OturumKazanc += oturumaEklenecek;
                if (odenen > 0)
                    _ctx.BonusOturumOdenenToplamTL += odenen;
                _ctx.SpinKazanciOturumaEklendi = true;
            }

            _ctx.BonusHakKalan = Mathf.Max(0, _ctx.BonusHakKalan - 1);
            _ctx.SenaryoOdenebilirGuncelle(odenen, 0);
            _ctx.UIServisi?.UI_Guncelle();

            SenaryoYoneticisi.I?.SpinTamamlandi(odenen, 0);
            SenaryoYoneticisi.I?.UI_Guncelle();

            // Az daha near-miss: bonus içinde büyük çarpan ama tumble yok — "Az daha! Hadi tekrar dene" paneli
            if (kayit != null && kayit.AzDahaNearMiss && _runCoroutine != null)
                yield return _runCoroutine(_ctx.ShowAzDahaPanelAndWait());

            Debug.Log($"[BONUS] SpinKazanci=" + odenen + " | BonusToplam=" + _ctx.BonusKazanc);
            if (_ctx.Grid != null && _ctx.Satir > 0 && _ctx.Sutun > 0)
            {
                var bc = new System.Collections.Generic.Dictionary<int, int>();
                for (int y = 0; y < _ctx.Satir; y++)
                    for (int x = 0; x < _ctx.Sutun; x++)
                    { int v = _ctx.Grid[x, y]; if (v >= 0) { if (!bc.ContainsKey(v)) bc[v] = 0; bc[v]++; } }
                var bs = new System.Text.StringBuilder("[SPIN] Meyve dağılımı: ");
                foreach (var kv in bc) bs.Append($"Meyve{kv.Key}={kv.Value} ");
                Debug.Log(bs.ToString());
            }

            yield return new WaitForSeconds(0.35f);
            yield return new WaitForSeconds(_ctx.BonusSpinBekleme);
        }

        // Bonus boyunca ödenen kazancı oturum sonunda tek seferde bakiyeye ekle.
        if (_ctx.BonusOturumOdenenToplamTL > 0 && _ctx.EkonomiServisi != null)
            _ctx.EkonomiServisi.AddWinnings(_ctx.BonusOturumOdenenToplamTL, 0);

        int bonusCikisBakiyesi = _ctx.EkonomiServisi != null ? _ctx.EkonomiServisi.Bakiye : 0;
        SenaryoYoneticisi.I?.LogBonusCikisi(bonusCikisBakiyesi);
        SenaryoYoneticisi.I?.LogEkle(SenaryoOlayKaydi.OlayTipi_BonusBitti, $"Bonus oyunu bitti. Toplam bonus kazanç: {_ctx.OturumKazanc:N0} TL. Çıkış bakiyesi: {bonusCikisBakiyesi:N0} TL.");
        int oturumKazanciSnapshot = _ctx.OturumKazanc;
        if (_runCoroutine != null)
            yield return _runCoroutine(_ctx.ShowBonusEndMessage(oturumKazanciSnapshot));

        _ctx.NormalOyunMusicPlay();
        _ctx.EkonomiServisi?.EkonomiSenkronizeEt();
        _ctx.HizVeSesServisi?.RestoreNormalSpeed();
        _ctx.NormalOyunMusicUnPause();

        _ctx.BonusAktif = false;
        SenaryoYoneticisi.I?.SetBonusAktif(false);
        _ctx.SetOturumKazancTextActive(true);
        _ctx.SpinCalisiyor = false;
        _ctx.UIServisi?.ButonDurumu(true);
        _ctx.UIServisi?.UI_Guncelle();
        _ctx.TryResumeOtomatikSpin();
    }
}
