// See https://stackoverflow.com/questions/44550970/firefox-54-stopped-trusting-self-signed-certs/49563778#49563778.


# Create authority certificate.
# TextExtension adds the Server Authentication enhanced key usage and the CA basic constraint.
$authorityCert = New-SelfSignedCertificate `
    -Subject "CN=Brainstorm CA,OU=IT,O=Brainstorm Certificate Authority,C=US" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyUsage CertSign, CRLSign, DigitalSignature, KeyEncipherment, DataEncipherment `
    -KeyExportPolicy Exportable `
    -NotBefore (Get-Date) `
    -NotAfter (Get-Date).AddYears(10) `
    -HashAlgorithm SHA256 `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -FriendlyName "Brainstorm CA" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1", "2.5.29.19={critical}{text}ca=1")


# Create development certificate.
# Sign it with authority certificate.
# TextExtension adds the Server Authentication enhanced key usage.
$devCert = New-SelfSignedCertificate `
    -Subject "CN=Brainstorm,OU=App Dev,O=Brainstorm,C=US" `
    -DnsName dev.brainstorm.com `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyUsage DigitalSignature, KeyEncipherment, DataEncipherment `
    -KeyExportPolicy Exportable `
    -NotBefore (Get-Date) `
    -NotAfter (Get-Date).AddYears(10) `
    -HashAlgorithm SHA256 `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -FriendlyName "Brainstorm" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") `
    -Signer $authorityCert
	
# Create development certificate.
# Sign it with authority certificate.
# TextExtension adds the Server Authentication enhanced key usage.
$devCert = New-SelfSignedCertificate `
    -Subject "CN=MadPoker,OU=App Dev,O=Brainstorm,C=US" `
    -DnsName dev.madpoker.net `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyUsage DigitalSignature, KeyEncipherment, DataEncipherment `
    -KeyExportPolicy Exportable `
    -NotBefore (Get-Date) `
    -NotAfter (Get-Date).AddYears(10) `
    -HashAlgorithm SHA256 `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -FriendlyName "MadPoker" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") `
    -Signer $authorityCert

# Export authority certificate to file.
$directory = "C:\Users\Erik\Documents\Temp\Certificates\"
if(!(test-path $directory))
{
New-Item -ItemType Directory -Force -Path $directory
}
$authorityCertPath = 'Cert:\LocalMachine\My\' + ($authorityCert.ThumbPrint)
$authorityCertFilename = $directory + "Authority.cer"
Export-Certificate -Cert $authorityCertPath -FilePath $authorityCertFilename

# Import authority certificate from file to Trusted Root store.
Import-Certificate -FilePath $authorityCertFilename -CertStoreLocation "Cert:\LocalMachine\Root"

# Delete authority certificate file.
Remove-Item -Path $authorityCertFilename