Published: 7.5.2021
Title: Triky s Windows CNG API
Menu: Triky s Windows CNG API
Cathegory: Dev
Description: Vytvorenie kľúčového páru a certifikátu chráneného TPM a zadanie PIN-u pre kartu.
OgImage: images/CngApiTips/pinDialog.jpg
---

V tomto krátkom bloku popisujem dva triky,
ktoré idú spraviť vo Windowsovom CNG Api ([Cryptohraphy Next Generation](https://docs.microsoft.com/en-us/windows/win32/seccng/cng-portal)) na C# kóde.

## Použitie TPM
TPM ([Trusted platform module](https://cs.wikipedia.org/wiki/Trusted_Platform_Module)) čip sa nachádza na stále viac notebookuoch a počítačoch,
je to čip na ktorý sa dajú ukladať šifrovacie a prihlasovacie kľúče (napríklad Windows ich využíva na FIDO2 autentifikáciu). 
Pomocou _Microsoft Platform Crypto Provider_ je možné vytvoriť kľúčový pár a k nemu certifikát, chránený pomocou TPM čipu.

```cs
using CngKey key = CngKey.Create(CngAlgorithm.Rsa,
      "KeyProtectedUsingTpm",
      new CngKeyCreationParameters()
      {
          ExportPolicy = CngExportPolicies.None,
          KeyCreationOptions = CngKeyCreationOptions.None,
          KeyUsage = CngKeyUsages.AllUsages,
          Provider = new CngProvider("Microsoft Platform Crypto Provider"),
          UIPolicy = new CngUIPolicy(CngUIProtectionLevels.ProtectKey)
      });

var cng = new RSACng(key);

CertificateRequest certificateRequest = new CertificateRequest("CN=ProtectedUsingTpm Example",
    cng,
    HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1);
X509Certificate2 certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(365));

using X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadWrite);
store.Add(certificate);
```
Kód vytvorí kľúčový pár chránený TPM čipom a používateľskou akciou,
vytvorí k nemu self-signed certifikát a uloží ho do _Windows certificate storu_ používateľa.

## Zadanie PINu ku čipovej karte
Pomocou CNG API je možné programovo zadať PIN k čipovej karte, ktorý by si inak CSP provider pýtal pomocou dialógu.

![CmartCard PIN dialog](images/CngApiTips/pinDialog.jpg){.img-center}

```cs
string thumbprint = "B6A2EC31AC1B48F7569BD5AF91CEBD2622F462E3";
string pin = "123456789";

using X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
X509Certificate2 cert = collection[0];

using RSA rsa = cert.GetRSAPrivateKey();
RSACng rsaCng = rsa as RSACng;

byte[] propertyBytes = Encoding.Unicode.GetBytes($"{pin}\0");
const string PIN_PROPERTY = "SmartCardPin";
CngProperty pinProperty = new CngProperty(
   PIN_PROPERTY,
   propertyBytes,
   CngPropertyOptions.None);
rsaCng.Key.SetProperty(pinProperty);

byte[] data = Encoding.UTF8.GetBytes("Hello World!");
byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

Console.WriteLine("Signature {0}", Convert.ToBase64String(signature));
```
Kód si zo storu certifikátov vytiahne príslušný certifikát podla thumbprintu. Jeho privátnemu kľúču nastaví PIN a podpíše ním dáta.

