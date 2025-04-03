Published: 15.3.2024
Updated: 3.4.2025
Title: Post-kvantová kryptografia v C#
Menu: Post-kvantová kryptografia v C#
Cathegory: Dev
Description: Ťahák pre post-kvantovú kryptografiu a ukážka v Bouncy Castle a C#.
OgImage: images/PostKvantovaKryptografia/media.jpg
---

Tento blog má byť taký malý praktický ťahák ku post-kvantovej kryptografii v C#. Nie je to ani prehľad, sú to len poznámky, príklady a zjednodušenia.

## Post-kvantová kryptografia
Post-kvantová kryptografia je klasická kryptografia, ktorá je odolná voči útokom kvantových počítačov.

Momentálne máme vymyslené algoritmy pre kvantové počítače, ktoré dokážu lámať asymetrickú kryptografiu - teda RSA a EC (eliptické krivky dokonca z trikrát menšou námahou ako RSA rovnakej bitovej bezpečnosti). Pre to sa už dlhšiu dobu vymýšľajú algoritmy asymetrickej kryptografie založené na iných problémoch ako faktorizácia prvočísiel alebo problém diskrétneho logaritmu. 

Štandardné algoritmy symetrického šifrovania a kryptografických hashov ako _AES_ (256), _SHA2_, _SHA3_, _PBKDF2_,... sa zatiaľ zdajú odolné voči útokom pomocou kvantových počítačov.

## Algoritmy pre enakapsuláciu (KEM)
Tieto algoritmy sa používajú ako náhrada za asymetrické šifrovanie (RSA) alebo Diffie–Hellmanovu výmenu kľúčov (_ECDH_),
no fungujú trochu inak. Pomocou verejného kľúča ide vytvoriť náhodné tajomstvo a jeho zašifrovanú podobu, ktorá ide rozšifrovať verejným kľúčom.

Momentálne je organizáciou _NIST_ schválený algoritmus _ML-KEN_ (_CRYSTALS-Kyber_).
Algoritmy _HQC_, _Bike_ a _Classic Mceliece_ sú v procese schvalovania.

Nasledujúca tabuľka ukazuje parametre jednotlivých algoritmov.

| Algoritmus          | Veľkosť verejného kľúča | Veľkosť súkromného kľúča | Zašifrované dáta |
| :---                |   ---:                  |   ---:                   |  ---:            |
| `ML-KEM 512`        | 800 B                   | 1&nbsp;632 B             | 768 B            |
| `ML-KEM 738`        | 1&nbsp;184 B            | 2&nbsp;400 B             | 1&nbsp;088 B     |
| `ML-KEM 1024`       | 1&nbsp;568 B            | 3&nbsp;168 B             | 1&nbsp;568 B     |
| `Bike128`           | 1&nbsp;541 B            | 3&nbsp;114 B             | 1&nbsp;573 B     | 
| `Bike192`           | 3&nbsp;083 B            | 6&nbsp;198 B             | 3&nbsp;115 B     |
| `Bike256`           | 5&nbsp;122 B            | 10&nbsp;276 B            | 5&nbsp;154 B     |
| `HQC128`            | 2&nbsp;249 B            | 2&nbsp;289 B             | 4&nbsp;497 B     |
| `HQC192`            | 4&nbsp;522 B            | 4&nbsp;562 B             | 9&nbsp;042 B     |
| `HQC256`            | 7&nbsp;245 B            | 7&nbsp;285 B             | 14&nbsp;485 B    |
| `McEliece348864`    | 261&nbsp;120 B          | 6&nbsp;492 B             | 196 B            |
| `McEliece460896`    | 524&nbsp;160 B          | 13&nbsp;608 B            | 156 B            |
| `McEliece6688128`   | 1&nbsp;044&nbsp;992 B   | 13&nbsp;932 B            | 208 B            |
| `McEliece6960119`   | 1&nbsp;047&nbsp;319 B   | 13&nbsp;948 B            | 194 B            |
| `McEliece8192128`   | 1&nbsp;357&nbsp;824 B   | 14&nbsp;120 B            | 208 B            |
| `NTRUhps2048509`    | 699 B                   | 935 B                    | 699 B            |
| `NTRUhps2048677`    | 930 B                   | 1&nbsp;234 B             | 930 B            |
| `NTRUhps4096821`    | 1&nbsp;230 B            | 1&nbsp;590 B             | 1&nbsp;230 B     |
| `SIKEp434`          | 330 B                   | 44 B                     | 346 B            |
| `SIKEp503`          | 378 B                   | 56 B                     | 402 B            |
| `SIKEp751`          | 564 B                   | 80 B                     | 596 B            |
| `SIDH`              | 564 B                   | 48 B                     | 596 B            |
| `LightSABER`        | 672 B                   | 1&nbsp;568 B             | 736 B            |
| `SABER`             | 992 B                   | 2&nbsp;304 B             | 1&nbsp;088 B     |
| `FireSABER`         | 1&nbsp;312 B            | 3&nbsp;040 B             | 1&nbsp;472 B     |

Nasledujúci kód zobrazuje ukážku použitia algoritmu _ML_KEM_ (podľa _FIPS 203_, pôvodne _Kyber_) v C# pomocou knižnice [BouncyCastle](https://github.com/bcgit/bc-csharp).

Ostatné dostupné algoritmy využívajú takmer rovnaký kód jediný rozdiel je v názvoch použitých tried.

```cs
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Kems;

SecureRandom random = new SecureRandom();

MLKemKeyGenerationParameters keyGenParameters = new MLKemKeyGenerationParameters(random, MLKemParameters.ml_kem_512);
MLKemKeyPairGenerator kyberKeyPairGenerator = new MLKemKeyPairGenerator();
kyberKeyPairGenerator.Init(keyGenParameters);

// Ganerovanie klucoveho paru pre Alicu
AsymmetricCipherKeyPair aliceKeyPair = kyberKeyPairGenerator.GenerateKeyPair();

// Zobrazenie klucov
MLKemPublicKeyParameters alicePublic = (MLKemPublicKeyParameters)aliceKeyPair.Public;
MLKemPrivateKeyParameters alicePrivate = (MLKemPrivateKeyParameters)aliceKeyPair.Private;
byte[] pubEncoded = alicePublic.GetEncoded();
byte[] privateEncoded = alicePrivate.GetEncoded();
Console.WriteLine($"Alice's Public key: {PrintData(pubEncoded)}");
Console.WriteLine($"Alice's Private key: {PrintData(privateEncoded)}");

// Bob enakpsuluje secret a pomocou verejneho klucu Alice
MLKemEncapsulator bobKyberKemGenerator = new MLKemEncapsulator(MLKemParameters.ml_kem_512);
bobKyberKemGenerator.Init(new ParametersWithRandom(alicePublic, random));

byte[] encodedBytes = new byte[bobKyberKemGenerator.EncapsulationLength];
byte[] bobSecret = new byte[bobKyberKemGenerator.SecretLength];
bobKyberKemGenerator.Encapsulate(encodedBytes, 0, encodedBytes.Length, bobSecret, 0, bobSecret.Length);

Console.WriteLine($"Encoded secret from Bob: {PrintData(encodedBytes)}");
Console.WriteLine($"Bob secret: {PrintData(bobSecret)}");

// Alice dekapsuluje secret a pomocou svojho privatneho klucu
MLKemDecapsulator decapsulator = new MLKemDecapsulator(MLKemParameters.ml_kem_512);
decapsulator.Init(alicePrivate);

byte[] aliceSecret = new byte[decapsulator.SecretLength];
decapsulator.Decapsulate(encodedBytes, 0, encodedBytes.Length, aliceSecret, 0, aliceSecret.Length);

Console.WriteLine($"Alice secret: {PrintData(aliceSecret)}");
```

## Algoritmy pre podpis
Tieto algoritmy sú pre podpis dát privátnym kľúčom a overenie podpisu verejným kľúčom.

Momentálne sú NIST-om schválené algoritmy _ML-DSA_ (_Dilithium_), _Falcon_ a _SLH-DSA_ (_SPHINCS+)_.

Nasledujúca tabuľka ukazuje parametre jednotlivých algoritmov.

 | Algoritmus                           | Veľkosť verejného kľúča | Veľkosť súkromného kľúča | Veľkosť podpisu  | Security        |
 | :---                                 |  ---:                   | ---:                     | ---:             | ---:            |
 | `ML-DSA 44`                          |     1&nbsp;312 B        |       2&nbsp;528 B       |    2&nbsp;420 B  | 128-bit         |
 | `ML-DSA 65`                          |     1&nbsp;952 B        |       4&nbsp;000 B       |    3&nbsp;293 B  | 192-bit         |
 | `ML-DSA 87`                          |     2&nbsp;592 B        |       4&nbsp;864 B       |    4&nbsp;595 B  | 256-bit         |
 | `Falcon 512 (Lattice)`               |       897 B             |       1&nbsp;281 B       |      690 B       | 128-bit         |
 | `Falcon 1024`                        |     1&nbsp;793 B        |       2&nbsp;305 B       |    1&nbsp;330 B  | 256-bit         |
 | `Rainbow Level Ia (Oil-and-Vineger)` |   161&nbsp;600 B        |     103&nbsp;648 B       |       66 B       | 128-bit         |
 | `Rainbow Level IIIa`                 |   861&nbsp;400 B        |     611&nbsp;300 B       |      164 B       | 192-bit         |
 | `Rainbow Level Vc`                   | 1&nbsp;885&nbsp;400 B   |   1&nbsp;375&nbsp;700 B  |      204 B       | 256-bit         |
 | `Sphincs SHA256-128f Simple`         |        32 B             |          64 B            |   17&nbsp;088 B  | 128-bit         |
 | `Sphincs SHA256-192f Simple`         |        48 B             |          96 B            |   35&nbsp;664 B  | 192-bit         |
 | `Sphincs SHA256-256f Simple`         |        64 B             |         128 B            |   49&nbsp;856 B  | 256-bit         |
 | `Picnic 3 Full`                      |        49 B             |          73 B            |   71&nbsp;179 B  | 192-bit         |
 | `GeMSS 128`                          |   352&nbsp;188 B        |          16 B            |       33 B       | 128-bit         |
 | `GeMSS 192`                          | 1&nbsp;237&nbsp;964 B   |          24 B            |       53 B       | 128-bit         |

Nasledujúci kód zobrazuje ukážku použitia algoritmu _ML-DSA_ (podľa _FIPS 204_, pôvodne _Crystals Dilithium_) v C# pomocou knižnice [BouncyCastle](https://github.com/bcgit/bc-csharp).

Ostatné dostupné algoritmy využívajú takmer rovnaký kód jediný rozdiel je v názvoch použitých tried.

```cs
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

SecureRandom random = new SecureRandom();

//Data na podpis
byte[] data = new byte[32];
random.NextBytes(data);

MLDsaKeyPairGenerator kpg = new MLDsaKeyPairGenerator();
kpg.Init(new MLDsaKeyGenerationParameters(random, MLDsaParameters.ml_dsa_65));

AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
MLDsaPublicKeyParameters publicKey = (MLDsaPublicKeyParameters)kp.Public;
MLDsaPrivateKeyParameters privateKey = (MLDsaPrivateKeyParameters)kp.Private;

Console.WriteLine($"Public key: {PrintData(publicKey.GetEncoded())}");
Console.WriteLine($"Private key: {PrintData(privateKey.GetEncoded())}");

MLDsaSigner signer = new MLDsaSigner(MLDsaParameters.ml_dsa_65, false);
signer.Init(true, privateKey);
signer.BlockUpdate(data);
byte[] signature = signer.GenerateSignature();

Console.WriteLine($"Signature: {PrintData(signature)}");

MLDsaSigner verifitier = new MLDsaSigner(MLDsaParameters.ml_dsa_65, false);
verifitier.Init(false, publicKey);
verifitier.BlockUpdate(data);

bool signatureIsVerified = verifitier.VerifySignature(signature);

Console.WriteLine($"Signature is verified: {signatureIsVerified}");
```

Metóda na výpis dát:
```cs
private static string PrintData(byte[] bytes)
{
    string base64 = Convert.ToBase64String(bytes);
    return (base64.Length > 50)
        ? $"({bytes.Length}B) {base64[..25]}...{base64[^25..]}"
        : $"({bytes.Length}B) {base64}";
}
```

## Upozornenie
Nie som odborník na interné fungovanie týchto algoritmov a ako pri inej kryptografii, treba sa riadiť odporúčaním organizácií venujúcim sa kryptografii. Takže použitie na vlastné riziko.

_BouncyCastle_ má zatiaľ experimentálne implementácie _CRYSTALS-Dilithium_, _CRYSTALS-Kyber_, _Falcon_, _SPHINCS+_, _Classic&nbsp;McEliece_, _FrodoKEM_, _NTRU_, _NTRU Prime_, _Picnic_, _Saber_, _BIKE_ a _SIKE_. Pričom algoritmus _SIKE_ bude odstránený.

Pre ďalšie čítanie odporúčam nasledujúce zdroje.

## Update maerc 2025
- _FIPS 203_ premenoval _CRYSTALS-Kyber_ na _ML-KEM_
- _FIPS 204_ premenoval _CRYSTALS-Dilithium_ na _ML-DSA_
- _FIPS 205_ premenoval _Sphincs+_ na _SLH-DSA_

Tieto algoritmy boli aj schválené.

## Zdroje
1. <https://www.nist.gov/news-events/news/2022/07/nist-announces-first-four-quantum-resistant-cryptographic-algorithms>
1. <https://billatnapier.medium.com/post-quantum-cryptography-pqc-with-bouncy-castle-and-c-4424dea684ec>
1. <https://www.strathweb.com/2023/02/post-quantum-cryptography-in-net/>
1. <https://medium.com/asecuritysite-when-bob-met-alice/goodbye-ecdh-and-hello-to-kyber-46415ef23d30>
1. <https://github.com/filipw?tab=repositories&q=&type=source&language=c%23&sort=>
1. <https://www.youtube.com/watch?v=JcfcSmwlFZ0>
