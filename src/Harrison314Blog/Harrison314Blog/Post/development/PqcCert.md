Published: 11.4.2025
Title: Hybridné certifikáty v  C#
Menu: Hybridné certifikáty v  C#
Cathegory: Dev
Description: Hybridné certifikáty s alternatívnym post-kvantovým kľúčom vytvorené v BouncyCastle a C#
OgImage: images/PostKvantovaKryptografia/media.jpg
---

Momentálne sme v prechodnom období, keď sa začína používať post-kvantová kryptografia,
ale ešte jej neveríme natoľko aby sme sa vzdali tej klasickej,
a hlavne post-kvantové algoritmy nemajú softvérovú a hardvérovú podporu (momentálne čipové karty a IoT procesory ako _ESP32_ alebo _STM32_ na dané algoritmy buď nemajú pamäť alebo dosť výkonu aby ich vypočítali v normálnom čase).

Pre elektronické podpisy a certifikáty vznikajú štandardy pre hybridné kľúče, ktoré sú kombináciou štandardného algoritmu (RSA, EC)
a post-kvantových podpisových kľúčov (ML-DSA, SLH-DSA), ale tie sú stále v procese schvalovania,
navyše týchto draftov a spôsobov kódovania je hneď niekoľko.

No vznikla alternatíva pre certifikáty s „alternatívnym kľúćom“,
kde sa použije EC alebo RSA kľúč, tak ako doteraz, takže certifikát je použiteľný v starých systémoch,
ale pomocou rozšírení (X509Extension) sa doň pridá post-kvantový verejný kľúč a podpis.

Nasledujúci kód ukazuje vytvorenie takéhoto self-signed certifikátu pomocou _BouncyCastle.Cryptography_ (od verzie 2.5.1).


```cs
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

SecureRandom secureRandom = new SecureRandom();

ECKeyPairGenerator ecKeyPairGenerator = new ECKeyPairGenerator();
ecKeyPairGenerator.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP521r1, secureRandom));
var ecKeyPair = ecKeyPairGenerator.GenerateKeyPair();
var ecSpki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(ecKeyPair.Public);
var ecSignatureFactory = new Asn1SignatureFactory("SHA512withECDSA", ecKeyPair.Private, secureRandom);

MLDsaKeyPairGenerator mlKemKeyPairGenerator = new MLDsaKeyPairGenerator();
mlKemKeyPairGenerator.Init(new MLDsaKeyGenerationParameters(secureRandom, MLDsaParameters.ml_dsa_65));
var mlKenKeyPair = mlKemKeyPairGenerator.GenerateKeyPair();
var mlKemSpki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(mlKenKeyPair.Public);
var mlKemSignatureFactory = new Asn1SignatureFactory("ML-DSA-65", mlKenKeyPair.Private, secureRandom);

X509V3CertificateGenerator certGenerator = new X509V3CertificateGenerator();
certGenerator.SetIssuerDN(new X509Name("CN=Hybrid certificate"));
certGenerator.SetSubjectDN(new X509Name("CN=Hybrid certificate"));
certGenerator.SetSubjectPublicKeyInfo(ecSpki);
certGenerator.SetNotBefore(DateTime.UtcNow);
certGenerator.SetNotAfter(DateTime.UtcNow.AddDays(1));
certGenerator.SetSerialNumber(new Org.BouncyCastle.Math.BigInteger("1"));
certGenerator.AddExtension(X509Extensions.KeyUsage, false, new KeyUsage(KeyUsage.NonRepudiation | KeyUsage.DigitalSignature));

certGenerator.AddExtension(X509Extensions.SubjectAltPublicKeyInfo, false, new SubjectAltPublicKeyInfo(mlKemSpki));

var certificate = certGenerator.Generate(ecSignatureFactory, false, mlKemSignatureFactory);

Console.WriteLine(System.Security.Cryptography.PemEncoding.WriteString("CERTIFICATE", certificate.GetEncoded()));
```

## Zdroje
1. <https://docs.keyfactor.com/ejbca/9.0/hybrid-ca>
1. <https://www.keyfactor.com/blog/quantum-safe-certificates-what-are-they-and-what-do-they-want-from-us/>
1. <https://www.bouncycastle.org/resources/preparing-for-the-migration-to-post-quantum-public-key-algorithms-with-hybrid-certificates/>
